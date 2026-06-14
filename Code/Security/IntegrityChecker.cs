using System;
using System.IO;
using System.Reflection;

namespace Celeste.Mod.MaggyHelper.Security
{
    /// <summary>
    /// Verifies assembly and asset integrity to detect tampering.
    /// Hash computation is delegated to <see cref="AssetProtector.ComputeHash"/> to avoid duplication.
    /// </summary>
    public static class IntegrityChecker
    {
        /// <summary>
        /// Embedded expected hash of the DLL, injected during the release build.
        /// Leave as "PLACEHOLDER_HASH" during development — the first run will log the real hash.
        /// </summary>
        private const string ExpectedAssemblyHash = "PLACEHOLDER_HASH";

        private const long TimingThresholdMs = 100;

        /// <summary>
        /// Verifies the current assembly hasn't been tampered with.
        /// Always returns <c>true</c> in DEBUG builds.
        /// </summary>
        public static bool VerifyAssemblyIntegrity()
        {
#if DEBUG
            return true;
#else
            try
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper", "Debugger detected — integrity check skipped.");
                    return false;
                }

                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                if (!File.Exists(assemblyPath))
                    return true; // in-memory assembly, nothing to verify

                byte[] assemblyData = File.ReadAllBytes(assemblyPath);
                string actualHash = AssetProtector.ComputeHash(assemblyData);

                if (ExpectedAssemblyHash == "PLACEHOLDER_HASH")
                {
                    Logger.Log(LogLevel.Info, "MaggyHelper", $"[Integrity] Assembly hash (embed this in release): {actualHash}");
                    return true;
                }

                return actualHash.Equals(ExpectedAssemblyHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper", $"[Integrity] Assembly check failed: {ex.Message}");
                return false;
            }
#endif
        }

        /// <summary>
        /// Timing check to detect single-step debugging.
        /// Always returns <c>true</c> in DEBUG builds.
        /// </summary>
        public static bool PerformTimingCheck()
        {
#if DEBUG
            return true;
#else
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int sum = 0;
            for (int i = 0; i < 1000; i++)
                sum += i;
            sw.Stop();
            return sw.ElapsedMilliseconds < TimingThresholdMs;
#endif
        }

        /// <summary>
        /// Computes the SHA-256 hash of <paramref name="data"/>.
        /// Delegates to <see cref="AssetProtector.ComputeHash"/>.
        /// </summary>
        public static string ComputeHash(byte[] data) => AssetProtector.ComputeHash(data);

        /// <summary>
        /// Verifies a file on disk against an expected SHA-256 hash.
        /// </summary>
        public static bool VerifyFile(string filePath, string expectedHash)
        {
            if (!File.Exists(filePath))
                return false;
            return AssetProtector.ComputeHash(File.ReadAllBytes(filePath))
                .Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
