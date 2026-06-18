using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.RuntimeDetour;
using System.Reflection;

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
        private static Hook _engineUpdateHook;
        private static Hook _levelUpdateHook;
        private static bool _wasF9Pressed = false;
        private static bool _wasF7Pressed = false;
        
        public static readonly string LogPath = Path.Combine(
            Everest.PathGame, "LiveWatch_CrashLog.txt");

        public static void Load()
        {
            // Hook Engine.Update - the root of all updates
            var engineUpdate = typeof(Engine).GetMethod("Update", 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (engineUpdate != null)
            {
                _engineUpdateHook = new Hook(engineUpdate, OnEngineUpdate);
            }

            // Hook Level.Update - gameplay updates
            var levelUpdate = typeof(Level).GetMethod("Update", 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (levelUpdate != null)
            {
                _levelUpdateHook = new Hook(levelUpdate, OnLevelUpdate);
            }

            // Hook Scene.Update for additional coverage
            On.Monocle.Scene.Update += OnSceneUpdate;

            Logger.Log(LogLevel.Info, "DZ", "[LiveWatch] Patcher installed");
        }

        public static void Unload()
        {
            _engineUpdateHook?.Dispose();
            _levelUpdateHook?.Dispose();
            On.Monocle.Scene.Update -= OnSceneUpdate;
        }

        private static void OnEngineUpdate(Action<Engine, GameTime> orig, Engine self, GameTime gameTime)
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

        private static void OnLevelUpdate(Action<Level> orig, Level self)
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
            // Keep the game rendering but frozen
            // Input is already checked in CheckInput()
            
            // Draw error overlay will be handled by the scene's render
            if (Engine.Scene != null)
            {
                // Ensure we have a frozen overlay
                var existing = Engine.Scene.Entities.FindFirst<LiveWatchOverlay>();
                if (existing == null)
                {
                    Engine.Scene.Add(new LiveWatchOverlay(_lastError, _errorTimestamp));
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

            // Add overlay to current scene
            if (Engine.Scene != null)
            {
                // Remove old overlays
                var old = Engine.Scene.Entities.FindFirst<LiveWatchOverlay>();
                old?.RemoveSelf();
                
                Engine.Scene.Add(new LiveWatchOverlay(_lastError, _errorTimestamp));
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
            
            // Remove overlay
            var overlay = Engine.Scene?.Entities.FindFirst<LiveWatchOverlay>();
            overlay?.RemoveSelf();

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
    [Tracked]
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
