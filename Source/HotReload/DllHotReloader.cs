using System;
using System.IO;
using System.Threading;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.HotReload
{
    /// <summary>
    /// Forces the DZ mod assembly to reload at runtime without restarting the game.
    ///
    /// This uses Everest's <c>Everest.Loader.ReloadMod</c> API which:
    ///   1. Unloads the current DZ assembly (and any mods depending on it)
    ///   2. Loads the freshly-built DZ.dll from disk
    ///   3. Re-initializes the module (DZModule.Load is called on the new assembly)
    ///   4. Reloads the current level so entity changes take effect
    ///
    /// A <see cref="FileSystemWatcher"/> monitors the bin/ directory so that
    /// every successful rebuild automatically triggers a reload — no key press needed.
    /// F11 remains available as a manual trigger.
    /// </summary>
    public static class DllHotReloader
    {
        private static FileSystemWatcher _watcher;
        private static Timer _debounceTimer;
        private static DateTime _lastReloadTime = DateTime.MinValue;
        private static readonly object _lock = new object();
        private static bool _initialized;

        /// <summary>Minimum gap between auto-reloads (ms) to coalesce rapid rebuilds.</summary>
        private const int ReloadCooldownMs = 1000;

        /// <summary>
        /// Set up the file watcher and enable Everest's built-in code reload.
        /// Call from <see cref="DZModule.Load"/>.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // Enable Everest's experimental code reload so that the built-in
            // FileSystemWatcher in EverestModuleAssemblyContext also fires.
            try
            {
                if (CoreModule.Instance != null && !CoreModule.Settings.CodeReload_WIP)
                {
                    CoreModule.Settings.CodeReload_WIP = true;
                    Logger.Log(LogLevel.Info, "DZ", "[HotReload] Enabled Everest CodeReload_WIP");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ", $"[HotReload] Could not enable CodeReload_WIP: {ex.Message}");
            }

            StartWatcher();
        }

        /// <summary>
        /// Tear down the file watcher.
        /// Call from <see cref="DZModule.Unload"/>.
        /// </summary>
        public static void Shutdown()
        {
            StopWatcher();
            _initialized = false;
        }

        // ── File watcher ──────────────────────────────────────────────

        private static void StartWatcher()
        {
            string dllDir = GetDllDirectory();
            if (dllDir == null)
            {
                Logger.Log(LogLevel.Warn, "DZ", "[HotReload] Could not determine DLL directory for file watcher.");
                return;
            }

            if (!Directory.Exists(dllDir))
            {
                Logger.Log(LogLevel.Warn, "DZ", $"[HotReload] DLL directory does not exist (yet): {dllDir}");
                return;
            }

            try
            {
                _watcher = new FileSystemWatcher(dllDir)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };

                _watcher.Changed += OnDllChanged;
                _watcher.Created += OnDllChanged;
                _watcher.Renamed += OnDllChanged;
                _watcher.Error += OnWatcherError;

                Logger.Log(LogLevel.Info, "DZ", $"[HotReload] Watching {dllDir} for DZ.dll changes");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ", $"[HotReload] Failed to start file watcher: {ex.Message}");
                _watcher?.Dispose();
                _watcher = null;
            }
        }

        private static void StopWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Changed -= OnDllChanged;
                _watcher.Created -= OnDllChanged;
                _watcher.Renamed -= OnDllChanged;
                _watcher.Error -= OnWatcherError;
                _watcher.Dispose();
                _watcher = null;
            }
            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }

        private static void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Logger.Log(LogLevel.Error, "DZ", $"[HotReload] File watcher error: {e.GetException()?.Message}");
            // Try to restart the watcher after a brief delay.
            // Lock to avoid racing with OnDllChanged which also touches _debounceTimer.
            lock (_lock)
            {
                StopWatcher();
                _debounceTimer = new Timer(_ => StartWatcher(), null, 2000, Timeout.Infinite);
            }
        }

        private static void OnDllChanged(object sender, FileSystemEventArgs e)
        {
            // Only react to the DZ.dll (and .pdb) file
            string name = Path.GetFileName(e.Name ?? "");
            if (!name.Equals("DZ.dll", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("DZ.pdb", StringComparison.OrdinalIgnoreCase))
                return;

            // Debounce: FileSystemWatcher fires multiple events per write.
            // Use a timer to wait until writes settle, then reload once.
            lock (_lock)
            {
                _debounceTimer?.Dispose();
                _debounceTimer = new Timer(_ => TryAutoReload(), null, 300, Timeout.Infinite);
            }
        }

        private static void TryAutoReload()
        {
            if (DZModule.Settings?.HotReloadEnabled != true) return;

            lock (_lock)
            {
                if ((DateTime.UtcNow - _lastReloadTime).TotalMilliseconds < ReloadCooldownMs)
                    return;
                _lastReloadTime = DateTime.UtcNow;
            }

            Logger.Log(LogLevel.Info, "DZ", "[HotReload] DZ.dll changed on disk — auto-reloading assembly.");
            ForceReload();
        }

        // ── Force reload ───────────────────────────────────────────────

        /// <summary>
        /// Force-reload the DZ mod assembly at runtime via Everest's ReloadMod API.
        /// Safe to call from the game thread. The actual reload is deferred to the
        /// main thread by Everest via <c>QueuedTaskHelperV2</c> and shows a loading screen.
        /// </summary>
        public static void ForceReload()
        {
            var module = DZModule.Instance;
            if (module?.Metadata == null)
            {
                Logger.Log(LogLevel.Warn, "DZ", "[HotReload] Cannot reload — DZModule or Metadata is null.");
                HotReloadUI.Show("Reload failed: module not found", Color.Red);
                return;
            }

            if (module.Metadata.AssemblyContext == null)
            {
                Logger.Log(LogLevel.Warn, "DZ", "[HotReload] Cannot reload — AssemblyContext is null (mod may be a zip).");
                HotReloadUI.Show("Reload failed: no assembly context", Color.Red);
                return;
            }

            try
            {
                HotReloadUI.Show("Reloading DZ.dll...", Color.Yellow);

                // Stop our watcher before the assembly gets unloaded, otherwise
                // the callback may fire on a disposed assembly context.
                StopWatcher();

                Logger.Log(LogLevel.Info, "DZ", $"[HotReload] Triggering Everest.Loader.ReloadMod for {module.Metadata.Name}");

                // This is the key call: it unloads the current DZ assembly,
                // loads the new DZ.dll from disk, and re-runs DZModule.Load().
                Everest.Loader.ReloadMod(module.Metadata);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "DZ", $"[HotReload] ForceReload failed: {ex}");
                HotReloadUI.Show("Reload failed: " + ex.Message, Color.Red);
                // Restart the watcher so we can try again on the next build
                StartWatcher();
            }
        }

        // ── Helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Returns the directory containing DZ.dll, derived from the mod metadata.
        /// Falls back to the assembly location if metadata is unavailable.
        /// </summary>
        private static string GetDllDirectory()
        {
            try
            {
                var meta = DZModule.Instance?.Metadata;
                if (meta != null && !string.IsNullOrEmpty(meta.DLL))
                    return Path.GetDirectoryName(Path.GetFullPath(meta.DLL));

                // Fallback: use the location of the currently loaded assembly
                var asmPath = typeof(DllHotReloader).Assembly.Location;
                if (!string.IsNullOrEmpty(asmPath))
                    return Path.GetDirectoryName(asmPath);
            }
            catch { }

            return null;
        }
    }
}
