using System;
using System.Collections.Generic;
using System.Diagnostics;
using Monocle;

namespace Celeste.Mod.DZ
{
    /// <summary>
    /// Lightweight performance profiler for critical mod operations.
    /// Tracks timing of level load hooks, entity validation, and audio transitions.
    /// </summary>
    public static class PerformanceProfiler
    {
        private static Dictionary<string, List<long>> _timings = new();
        private static Dictionary<string, long> _activeTimers = new();
        private static bool _enabled = false;

        public static bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// <summary>
        /// Start timing an operation.
        /// </summary>
        public static void Begin(string operation)
        {
            if (!_enabled) return;
            _activeTimers[operation] = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// End timing an operation and record the duration.
        /// </summary>
        public static void End(string operation)
        {
            if (!_enabled) return;
            if (!_activeTimers.TryGetValue(operation, out long start))
                return;

            long elapsed = Stopwatch.GetTimestamp() - start;
            double ms = (elapsed * 1000.0) / Stopwatch.Frequency;

            if (!_timings.TryGetValue(operation, out var list))
            {
                list = new List<long>();
                _timings[operation] = list;
            }
            list.Add((long)ms);

            // Log slow operations (> 5ms)
            if (ms > 5.0)
            {
                Logger.Log(LogLevel.Warn, "DZ/Perf",
                    $"Slow operation: {operation} took {ms:F2}ms");
            }

            _activeTimers.Remove(operation);
        }

        /// <summary>
        /// Profile an action and return its duration.
        /// </summary>
        public static double Profile(string operation, Action action)
        {
            if (!_enabled)
            {
                action();
                return 0;
            }

            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();

            double ms = sw.Elapsed.TotalMilliseconds;
            if (!_timings.TryGetValue(operation, out var list))
            {
                list = new List<long>();
                _timings[operation] = list;
            }
            list.Add((long)ms);

            if (ms > 5.0)
            {
                Logger.Log(LogLevel.Warn, "DZ/Perf",
                    $"Slow operation: {operation} took {ms:F2}ms");
            }

            return ms;
        }

        /// <summary>
        /// Get statistics for a tracked operation.
        /// </summary>
        public static (double avg, double max, double min, int count) GetStats(string operation)
        {
            if (!_timings.TryGetValue(operation, out var list) || list.Count == 0)
                return (0, 0, 0, 0);

            double avg = 0;
            double max = double.MinValue;
            double min = double.MaxValue;

            foreach (var t in list)
            {
                avg += t;
                if (t > max) max = t;
                if (t < min) min = t;
            }
            avg /= list.Count;

            return (avg, max, min, list.Count);
        }

        /// <summary>
        /// Log a summary of all tracked operations.
        /// </summary>
        public static void LogSummary()
        {
            if (!_enabled || _timings.Count == 0)
            {
                Logger.Log(LogLevel.Info, "DZ/Perf", "No performance data collected.");
                return;
            }

            Logger.Log(LogLevel.Info, "DZ/Perf", "=== Performance Summary ===");
            foreach (var kvp in _timings)
            {
                var (avg, max, min, count) = GetStats(kvp.Key);
                Logger.Log(LogLevel.Info, "DZ/Perf",
                    $"  {kvp.Key}: avg={avg:F2}ms, max={max:F2}ms, min={min:F2}ms, samples={count}");
            }
        }

        /// <summary>
        /// Clear all collected performance data.
        /// </summary>
        public static void Reset()
        {
            _timings.Clear();
            _activeTimers.Clear();
        }

        #region Console Commands

        public static void RegisterConsoleCommands()
        {
            try
            {
                var cmdType = typeof(Everest).Assembly.GetType("Celeste.Mod.Everest+Commands");
                if (cmdType != null)
                {
                    var registerMethod = cmdType.GetMethod("Register", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    registerMethod?.Invoke(null, new object[] { "DZ_perf_enable", (Action)(() =>
                    {
                        Enabled = true;
                        Logger.Log(LogLevel.Info, "DZ/Perf", "Performance profiling enabled");
                    })});

                    registerMethod?.Invoke(null, new object[] { "DZ_perf_summary", (Action)(() =>
                    {
                        LogSummary();
                    })});

                    registerMethod?.Invoke(null, new object[] { "DZ_perf_reset", (Action)(() =>
                    {
                        Reset();
                        Logger.Log(LogLevel.Info, "DZ/Perf", "Performance data reset");
                    })});
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ/Perf", $"Could not register perf commands: {ex.Message}");
            }
        }

        #endregion
    }
}
