using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.HotReload
{
    /// <summary>
    /// Live watch monitor that catches exceptions, freezes the game,
    /// and logs detailed error information for AI analysis.
    /// </summary>
    [Tracked]
    public class LiveWatchMonitor : Entity
    {
        private static LiveWatchMonitor Instance;
        private static List<string> ErrorLog = new List<string>();
        private static bool _isFrozen = false;
        private static string _lastError = null;
        private static DateTime _errorTimestamp;
        
        // Error report file path
        private static readonly string ErrorReportPath = Path.Combine(
            Path.GetDirectoryName(typeof(LiveWatchMonitor).Assembly.Location) ?? ".", 
            "LiveWatch_ErrorReport.log");

        public LiveWatchMonitor()
        {
            Tag = Tags.Global | Tags.FrozenUpdate | Tags.PauseUpdate | Tags.HUD;
            Instance = this;
        }

        public static void Install()
        {
            // Hook into scene loading
            On.Celeste.Level.LoadLevel += OnLoadLevel;
            
            // Hook into update to catch errors
            On.Monocle.EntityList.Update += OnEntityListUpdate;
            On.Monocle.RendererList.Update += OnRendererListUpdate;
            
            Logger.Log(LogLevel.Info, "DZ", "[LiveWatch] Monitor installed");
        }

        public static void Uninstall()
        {
            On.Celeste.Level.LoadLevel -= OnLoadLevel;
            On.Monocle.EntityList.Update -= OnEntityListUpdate;
            On.Monocle.RendererList.Update -= OnRendererListUpdate;
        }

        private static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            
            // Ensure monitor exists in level
            if (self.Tracker.GetEntity<LiveWatchMonitor>() == null)
            {
                self.Add(new LiveWatchMonitor());
            }
        }

        private static void OnEntityListUpdate(On.Monocle.EntityList.orig_Update orig, EntityList self)
        {
            if (_isFrozen) 
            {
                // Still call orig to keep UI responsive, but skip entity updates
                // by only updating the monitor itself
                var monitor = self.Scene?.Tracker?.GetEntity<LiveWatchMonitor>();
                monitor?.Update();
                return;
            }
            
            try
            {
                orig(self);
            }
            catch (Exception ex)
            {
                HandleError(ex, "EntityList.Update");
            }
        }

        private static void OnRendererListUpdate(On.Monocle.RendererList.orig_Update orig, RendererList self)
        {
            if (_isFrozen) return;
            
            try
            {
                orig(self);
            }
            catch (Exception ex)
            {
                HandleError(ex, "RendererList.Update");
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Check for unhandled entity errors
            if (Scene is Level level)
            {
                CheckEntitiesForErrors(level);
            }
        }

        private void CheckEntitiesForErrors(Level level)
        {
            var entities = level.Entities;
            foreach (Entity entity in entities)
            {
                if (entity == null) continue;
                
                try
                {
                    // Check if entity has null critical components
                    CheckEntityHealth(entity);
                }
                catch (Exception ex)
                {
                    HandleError(ex, $"Entity {entity.GetType().Name} health check");
                }
            }
        }

        private void CheckEntityHealth(Entity entity)
        {
            // Check for common broken states
            if (entity is Solid solid)
            {
                if (solid.Width <= 0 || solid.Height <= 0)
                {
                    LogWarning($"Solid {entity.GetType().Name} has invalid size: {solid.Width}x{solid.Height}");
                }
            }
        }

        public override void Render()
        {
            base.Render();
            
            if (_isFrozen && _lastError != null)
            {
                DrawErrorOverlay();
            }
        }

        private void DrawErrorOverlay()
        {
            // Darken screen
            Draw.Rect(0, 0, 1920, 1080, Color.Black * 0.85f);
            
            // Draw error header
            string header = "[!] LIVE WATCH - GAME FROZEN [!]";
            ActiveFont.Draw(header, new Vector2(960, 100), new Vector2(0.5f, 0f), Vector2.One * 1.5f, Color.Red);
            
            // Draw error info
            string timeStr = $"Error Time: {_errorTimestamp:HH:mm:ss}";
            ActiveFont.Draw(timeStr, new Vector2(960, 180), new Vector2(0.5f, 0f), Vector2.One, Color.Yellow);
            
            // Draw last error (truncated)
            string errorDisplay = _lastError?.Length > 200 ? _lastError.Substring(0, 200) + "..." : _lastError;
            ActiveFont.Draw(errorDisplay, new Vector2(100, 240), Vector2.Zero, Vector2.One * 0.8f, Color.White);
            
            // Draw instructions
            string instructions = "Press F7 to copy error report | Press F9 to resume | Check LiveWatch_ErrorReport.log";
            ActiveFont.Draw(instructions, new Vector2(960, 1000), new Vector2(0.5f, 0f), Vector2.One, Color.Cyan);
        }

        public override void HandleGraphicsReset()
        {
            base.HandleGraphicsReset();
            // Keep frozen state across graphics resets
        }

        private static void HandleError(Exception ex, string context)
        {
            if (_isFrozen) return; // Already frozen
            
            _isFrozen = true;
            _lastError = $"[{context}] {ex.GetType().Name}: {ex.Message}";
            _errorTimestamp = DateTime.Now;
            
            // Build detailed error report
            var report = BuildErrorReport(ex, context);
            
            // Log to console
            Logger.Log(LogLevel.Error, "DZ", $"[LiveWatch] ERROR CAPTURED: {_lastError}");
            Logger.Log(LogLevel.Error, "DZ", $"[LiveWatch] Game frozen. Report saved to: {ErrorReportPath}");
            
            // Write to file
            try
            {
                File.WriteAllText(ErrorReportPath, report);
            }
            catch (Exception fileEx)
            {
                Logger.Log(LogLevel.Error, "DZ", $"[LiveWatch] Failed to write report: {fileEx.Message}");
            }
            
            // Store in memory log
            ErrorLog.Add(report);
            if (ErrorLog.Count > 50) ErrorLog.RemoveAt(0);
            
            // Show notification
            HotReloadUI.Show("ERROR DETECTED - GAME FROZEN - Press F9", Color.Red);
        }

        private static string BuildErrorReport(Exception ex, string context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== LIVE WATCH ERROR REPORT ===");
            sb.AppendLine($"Timestamp: {_errorTimestamp:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Context: {context}");
            sb.AppendLine($"Exception Type: {ex.GetType().FullName}");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine();
            sb.AppendLine("=== STACK TRACE ===");
            sb.AppendLine(ex.StackTrace);
            sb.AppendLine();
            
            // Get inner exception
            if (ex.InnerException != null)
            {
                sb.AppendLine("=== INNER EXCEPTION ===");
                sb.AppendLine($"Type: {ex.InnerException.GetType().FullName}");
                sb.AppendLine($"Message: {ex.InnerException.Message}");
                sb.AppendLine($"StackTrace: {ex.InnerException.StackTrace}");
                sb.AppendLine();
            }
            
            // Get scene info
            sb.AppendLine("=== SCENE INFO ===");
            try
            {
                if (Engine.Scene is Level level)
                {
                    var sid = level.Session != null && level.Session.Area.GetSID() != null ? level.Session.Area.GetSID() : "Unknown";
                    sb.AppendLine($"Current Level: {sid}");
                    sb.AppendLine($"Room: {level.Session?.LevelData?.Name ?? "Unknown"}");
                    sb.AppendLine($"Entity Count: {level.Entities?.Count ?? 0}");
                    sb.AppendLine($"Player Position: {level.Tracker?.GetEntity<Player>()?.Position.ToString() ?? "No player"}");
                }
                else
                {
                    sb.AppendLine($"Current Scene: {Engine.Scene?.GetType().Name ?? "None"}");
                }
            }
            catch (Exception sceneEx)
            {
                sb.AppendLine($"Error getting scene info: {sceneEx.Message}");
            }
            
            sb.AppendLine();
            sb.AppendLine("=== MOD INFO ===");
            sb.AppendLine($"DZ Mod Version: {typeof(DZModule).Assembly.GetName().Version}");
            sb.AppendLine($"Everest Version: {Everest.Version}");
            
            sb.AppendLine();
            sb.AppendLine("=== AI ANALYSIS HINTS ===");
            sb.AppendLine("1. Check if the error occurs in a specific room/level");
            sb.AppendLine("2. Look for null references in the stack trace");
            sb.AppendLine("3. Check if any entities were recently added/modified");
            sb.AppendLine("4. Verify asset files (sprites, dialogs) are present");
            sb.AppendLine();
            sb.AppendLine("=== END REPORT ===");
            
            return sb.ToString();
        }

        private static void LogWarning(string message)
        {
            Logger.Log(LogLevel.Warn, "DZ", $"[LiveWatch] {message}");
        }

        // Public methods for manual control
        public static bool IsFrozen => _isFrozen;
        
        public static void Resume()
        {
            _isFrozen = false;
            _lastError = null;
            HotReloadUI.Show("Live Watch - Game Resumed", Color.LimeGreen);
            Logger.Log(LogLevel.Info, "DZ", "[LiveWatch] Game resumed by user");
        }
        
        public static void CopyErrorReport()
        {
            if (_lastError == null) return;
            
            try
            {
                var report = ErrorLog.Count > 0 ? ErrorLog[ErrorLog.Count - 1] : "No error report available";
                // TextCopy.ClipboardService.SetText(report); // Would need TextCopy package
                Logger.Log(LogLevel.Info, "DZ", "[LiveWatch] Error report ready for copy");
                HotReloadUI.Show("Error report logged to file", Color.Yellow);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "DZ", $"[LiveWatch] Failed to copy report: {ex.Message}");
            }
        }
        
        public static string GetLastErrorReport()
        {
            return ErrorLog.Count > 0 ? ErrorLog[ErrorLog.Count - 1] : null;
        }
    }
}
