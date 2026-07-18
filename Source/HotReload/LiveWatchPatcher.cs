using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.DZ.HotReload
{
    /// <summary>
    /// Aggressive live watch patcher that hooks core game loops to catch errors
    /// and freeze the game state for debugging.
    /// </summary>
    public static class LiveWatchPatcher
    {
        private static bool _isFrozen = false;
        private static string _lastError = null;
        private static DateTime _errorTimestamp;
        private static List<string> _errorHistory = new List<string>();
        private static bool _wasF9Pressed = false;
        private static bool _wasF7Pressed = false;

        // Cached overlay/scene so we don't scan the entity list every frame while frozen.
        private static Scene _overlayScene;
        private static LiveWatchOverlay _cachedOverlay;

        // Cached reflection metadata for the RendererList internal lists.
        private static Type _rendererListType;
        private static FieldInfo _rendererListField;
        private static FieldInfo _rendererAddingField;
        private static FieldInfo _rendererRemovingField;
        
        public static readonly string LogPath = Path.Combine(
            Everest.PathGame, "LiveWatch_CrashLog.txt");

        public static void Load()
        {
            // Hook core update loops. On.* hooks are cheaper than reflection-based Hook
            // and still wrap the original method so exceptions propagate into our try/catch.
            On.Monocle.Engine.Update += OnEngineUpdate;
            On.Celeste.Level.Update += OnLevelUpdate;
            On.Monocle.Scene.Update += OnSceneUpdate;
            On.Monocle.RendererList.Update += OnRendererListUpdate;
            On.Monocle.RendererList.BeforeRender += OnRendererListBeforeRender;

            Logger.Log(LogLevel.Info, "DZ", "[LiveWatch] Patcher installed");
        }

        public static void Unload()
        {
            On.Monocle.Engine.Update -= OnEngineUpdate;
            On.Celeste.Level.Update -= OnLevelUpdate;
            On.Monocle.Scene.Update -= OnSceneUpdate;
            On.Monocle.RendererList.Update -= OnRendererListUpdate;
            On.Monocle.RendererList.BeforeRender -= OnRendererListBeforeRender;
        }

        private static void OnEngineUpdate(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
        {
            // Check for freeze/resume input at the start
            CheckInput();

            if (_isFrozen)
            {
                // Only update input and rendering, skip game logic
                UpdateWhileFrozen();
                return;
            }

            try
            {
                orig(self, gameTime);
            }
            catch (Exception ex)
            {
                Freeze(ex, "Engine.Update");
            }
        }

        private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            if (_isFrozen)
            {
                // Skip level updates while frozen
                return;
            }

            try
            {
                orig(self);
            }
            catch (Exception ex)
            {
                Freeze(ex, $"Level.Update ({self.Session?.LevelData?.Name ?? "unknown room"})");
            }
        }

        private static void OnSceneUpdate(On.Monocle.Scene.orig_Update orig, Scene self)
        {
            if (_isFrozen && !(self is Level))
            {
                // Allow scene updates for non-level scenes even when frozen
                // to keep menus functional
                try
                {
                    orig(self);
                }
                catch (Exception ex)
                {
                    Freeze(ex, $"Scene.Update ({self.GetType().Name})");
                }
                return;
            }

            if (!_isFrozen)
            {
                try
                {
                    orig(self);
                }
                catch (Exception ex)
                {
                    Freeze(ex, $"Scene.Update ({self.GetType().Name})");
                }
            }
        }

        private static void OnRendererListUpdate(On.Monocle.RendererList.orig_Update orig, RendererList self)
        {
            if (!_isFrozen)
            {
                CleanNullRenderers(self);
                try
                {
                    orig(self);
                }
                catch (Exception ex)
                {
                    Freeze(ex, "RendererList.Update");
                }
            }
        }

        private static void OnRendererListBeforeRender(On.Monocle.RendererList.orig_BeforeRender orig, RendererList self)
        {
            if (!_isFrozen)
            {
                CleanNullRenderers(self);
                try
                {
                    orig(self);
                }
                catch (Exception ex)
                {
                    Freeze(ex, "RendererList.BeforeRender");
                }
            }
        }

        private static void EnsureRendererListReflection()
        {
            if (_rendererListType != null)
                return;

            _rendererListType = typeof(Scene).Assembly.GetType("Monocle.RendererList");
            if (_rendererListType == null)
                return;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            _rendererListField = _rendererListType.GetField("Renderers", flags);
            _rendererAddingField = _rendererListType.GetField("adding", flags);
            _rendererRemovingField = _rendererListType.GetField("removing", flags);
        }

        private static void CleanNullRenderers(RendererList self)
        {
            try
            {
                EnsureRendererListReflection();
                if (_rendererListField == null)
                    return;

                int removed = 0;
                removed += RemoveNullRenderers(_rendererListField.GetValue(self));
                removed += RemoveNullRenderers(_rendererAddingField?.GetValue(self));
                removed += RemoveNullRenderers(_rendererRemovingField?.GetValue(self));

                if (removed > 0)
                    Logger.Log(LogLevel.Warn, "DZ", $"[LiveWatch] Removed {removed} null renderer(s) from RendererList");
            }
            catch
            {
                // Never let cleanup throw.
            }
        }

        private static int RemoveNullRenderers(object listObj)
        {
            if (listObj is not System.Collections.IList list)
                return 0;

            int removed = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                    removed++;
                }
            }
            return removed;
        }

        private static void CheckInput()
        {
            KeyboardState kb = Keyboard.GetState();
            bool f9 = kb.IsKeyDown(Keys.F9);
            bool f7 = kb.IsKeyDown(Keys.F7);

            // F9: Resume from freeze OR toggle if not frozen
            if (f9 && !_wasF9Pressed)
            {
                if (_isFrozen)
                {
                    Resume();
                }
            }
            _wasF9Pressed = f9;

            // F7: Copy error report
            if (f7 && !_wasF7Pressed)
            {
                CopyReportToClipboard();
            }
            _wasF7Pressed = f7;
        }

        private static void UpdateWhileFrozen()
        {
            // Keep the game rendering but frozen.
            // Input is already checked in CheckInput(). The overlay is only looked up
            // when the scene changes or the cached overlay has been removed, avoiding
            // a full entity-list scan every single frame while the game is paused.
            var scene = Engine.Scene;
            if (scene == null)
            {
                _overlayScene = null;
                _cachedOverlay = null;
                return;
            }

            if (scene != _overlayScene || _cachedOverlay?.Scene != scene)
            {
                _overlayScene = scene;
                _cachedOverlay = scene.Entities.FindFirst<LiveWatchOverlay>();
                if (_cachedOverlay == null)
                {
                    _cachedOverlay = new LiveWatchOverlay(_lastError, _errorTimestamp);
                    scene.Add(_cachedOverlay);
                }
            }
        }

        private static void Freeze(Exception ex, string context)
        {
            if (_isFrozen) return;

            _isFrozen = true;
            _errorTimestamp = DateTime.Now;
            _lastError = $"[{context}] {ex.GetType().Name}: {ex.Message}";

            string report = BuildCrashReport(ex, context);
            _errorHistory.Add(report);

            // Write to file immediately
            try
            {
                File.WriteAllText(LogPath, report);
            }
            catch { }

            // Log to game console
            Logger.Log(LogLevel.Error, "DZ", "[!] LIVE WATCH - GAME FROZEN [!]");
            Logger.Log(LogLevel.Error, "DZ", $"Error: {_lastError}");
            Logger.Log(LogLevel.Error, "DZ", "Press F9 to resume");
            Logger.Log(LogLevel.Error, "DZ", $"Full report: {LogPath}");

            // Add overlay to current scene (reusing the cached overlay slot).
            var scene = Engine.Scene;
            if (scene != null)
            {
                _overlayScene = scene;
                _cachedOverlay?.RemoveSelf();
                _cachedOverlay = new LiveWatchOverlay(_lastError, _errorTimestamp);
                scene.Add(_cachedOverlay);
            }

            // Play error sound if audio is working
            try
            {
                Audio.Play("event:/ui/main/button_invalid");
            }
            catch { }
        }

        public static void Resume()
        {
            if (!_isFrozen) return;

            _isFrozen = false;

            // Remove overlay using the cached reference instead of scanning the scene.
            _cachedOverlay?.RemoveSelf();
            _cachedOverlay = null;
            _overlayScene = null;

            Logger.Log(LogLevel.Info, "DZ", "[LiveWatch] Game resumed");
            
            try
            {
                Audio.Play("event:/ui/game/unlock_summary");
            }
            catch { }
        }

        private static string BuildCrashReport(Exception ex, string context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║           LIVE WATCH CRASH REPORT                            ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"Timestamp: {_errorTimestamp:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Context: {context}");
            sb.AppendLine($"Frozen: {_isFrozen}");
            sb.AppendLine();
            sb.AppendLine("=== EXCEPTION ===");
            sb.AppendLine($"Type: {ex.GetType().FullName}");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Stack:");
            sb.AppendLine(ex.StackTrace);
            sb.AppendLine();
            
            if (ex.InnerException != null)
            {
                sb.AppendLine("=== INNER EXCEPTION ===");
                sb.AppendLine($"Type: {ex.InnerException.GetType().FullName}");
                sb.AppendLine($"Message: {ex.InnerException.Message}");
                sb.AppendLine($"Stack:");
                sb.AppendLine(ex.InnerException.StackTrace);
                sb.AppendLine();
            }

            sb.AppendLine("=== SCENE STATE ===");
            try
            {
                if (Engine.Scene is Level level)
                {
                    sb.AppendLine($"Scene: Level");
                    sb.AppendLine($"Session: {level.Session?.ToString() ?? "null"}");
                    sb.AppendLine($"Entities: {level.Entities?.Count ?? 0}");
                    sb.AppendLine($"Player: {level.Tracker?.GetEntity<Player>()?.Position.ToString() ?? "null"}");
                }
                else
                {
                    sb.AppendLine($"Scene: {Engine.Scene?.GetType().Name ?? "null"}");
                }
            }
            catch (Exception sceneEx)
            {
                sb.AppendLine($"Error getting scene: {sceneEx.Message}");
            }
            sb.AppendLine();

            sb.AppendLine("=== SYSTEM ===");
            sb.AppendLine($"Everest: {Everest.Version}");
            sb.AppendLine($"DZ Mod: {typeof(DZModule).Assembly.GetName().Version}");
            sb.AppendLine();

            sb.AppendLine("=== MCP AI ANALYSIS NOTES ===");
            sb.AppendLine("• Look for NullReferenceException - check recent entity changes");
            sb.AppendLine("• Check if error occurs in specific room or globally");
            sb.AppendLine("• Verify assets (sprites, audio, dialogs) are loaded");
            sb.AppendLine("• Check for mod conflicts or missing dependencies");
            sb.AppendLine("• Review stack trace for custom entity methods");
            sb.AppendLine();
            sb.AppendLine("End of report.");

            return sb.ToString();
        }

        private static void CopyReportToClipboard()
        {
            try
            {
                if (_errorHistory.Count > 0)
                {
                    string lastReport = _errorHistory[_errorHistory.Count - 1];
                    // Write to clipboard via text file that can be easily opened
                    string clipboardPath = Path.Combine(Everest.PathGame, "LiveWatch_Clipboard.txt");
                    File.WriteAllText(clipboardPath, lastReport);
                    Logger.Log(LogLevel.Info, "DZ", $"[LiveWatch] Error report written to: {clipboardPath}");
                    HotReloadUI.Show("Report saved to LiveWatch_Clipboard.txt", Color.Yellow);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ", $"[LiveWatch] Failed to copy: {ex.Message}");
            }
        }

        public static bool IsFrozen => _isFrozen;
        public static string LastError => _lastError;
    }

    /// <summary>
    /// Visual overlay that displays when game is frozen
    /// </summary>
    public class LiveWatchOverlay : Entity
    {
        private string _errorMessage;
        private DateTime _errorTime;
        private float _blinkTimer = 0f;

        public LiveWatchOverlay(string errorMessage, DateTime errorTime)
        {
            _errorMessage = errorMessage ?? "Unknown error";
            _errorTime = errorTime;
            Tag = Tags.HUD | Tags.Global | Tags.FrozenUpdate | Tags.PauseUpdate;
            Depth = -100000; // Render on top of everything
        }

        public override void Update()
        {
            base.Update();
            _blinkTimer += Engine.RawDeltaTime;
        }

        public override void Render()
        {
            // Dark background
            Draw.Rect(0, 0, 1920, 1080, Color.Black * 0.85f);

            // Header with blinking effect
            bool blink = ((int)(_blinkTimer * 2f) % 2) == 0;
            Color headerColor = blink ? Color.Red : Color.DarkRed;
            string header = "[!] LIVE WATCH - GAME FROZEN [!]";
            ActiveFont.Draw(header, new Vector2(960, 80), new Vector2(0.5f, 0f), Vector2.One * 1.5f, headerColor);

            // Error info
            string timeStr = $"Time: {_errorTime:HH:mm:ss}";
            ActiveFont.Draw(timeStr, new Vector2(960, 150), new Vector2(0.5f, 0f), Vector2.One, Color.Yellow);

            // Error message (wrapped)
            string displayError = _errorMessage.Length > 150 
                ? _errorMessage.Substring(0, 150) + "..." 
                : _errorMessage;
            ActiveFont.Draw(displayError, new Vector2(100, 220), Vector2.Zero, Vector2.One * 0.75f, Color.White);

            // Instructions
            float y = 900;
            ActiveFont.Draw("Controls:", new Vector2(960, y), new Vector2(0.5f, 0f), Vector2.One, Color.Cyan);
            y += 40;
            ActiveFont.Draw("F9 = Resume game", new Vector2(960, y), new Vector2(0.5f, 0f), Vector2.One * 0.8f, Color.LimeGreen);
            y += 30;
            ActiveFont.Draw("F7 = Save report to LiveWatch_Clipboard.txt", new Vector2(960, y), new Vector2(0.5f, 0f), Vector2.One * 0.8f, Color.LimeGreen);
            y += 30;
            ActiveFont.Draw($"Log: {LiveWatchPatcher.LogPath}", new Vector2(960, y), new Vector2(0.5f, 0f), Vector2.One * 0.6f, Color.Gray);
        }
    }
}
