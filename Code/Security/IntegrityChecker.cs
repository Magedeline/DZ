using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Celeste.Mod.MaggyHelper.Security
{
    /// <summary>
    /// Verifies assembly and asset integrity to detect tampering.
    /// Nintendo-style anti-tampering for Desolo Zantas.
    /// </summary>
    public static class IntegrityChecker
    {
        // Embedded expected hash of the DLL (set during build)
        private static readonly string ExpectedAssemblyHash = "PLACEHOLDER_HASH";
        
        // Anti-debug timing check threshold
        private const long TimingThresholdMs = 100;

        /// <summary>
        /// Verifies the current assembly hasn't been tampered with.
        /// Called during mod initialization.
        /// </summary>
        public static bool VerifyAssemblyIntegrity()
        {
            try
            {
                // Skip verification in debug builds
                #if DEBUG
                return true;
                #endif

                // Anti-debug: Check for debugger attachment
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper", "Debugger detected - integrity check skipped");
                    return false;
                }

                // Verify assembly hash
                Assembly assembly = Assembly.GetExecutingAssembly();
                string assemblyPath = assembly.Location;
                
                if (!File.Exists(assemblyPath))
                    return true; // In-memory assembly, can't verify

                byte[] assemblyData = File.ReadAllBytes(assemblyPath);
                string actualHash = ComputeHash(assemblyData);

                if (ExpectedAssemblyHash == "PLACEHOLDER_HASH")
                {
                    // First run - log the hash for embedding
                    Logger.Log(LogLevel.Info, "MaggyHelper", $"Assembly hash: {actualHash}");
                    return true;
                }

                return actualHash.Equals(ExpectedAssemblyHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper", $"Integrity check failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Timing check to detect debugging/single-stepping.
        /// </summary>
        public static bool PerformTimingCheck()
        {
            #if DEBUG
            return true;
            #endif

            var sw = System.Diagnostics.Stopwatch.StartNew();
            
            // Simple operation that should be fast
            int sum = 0;
            for (int i = 0; i < 1000; i++)
                sum += i;
            
            sw.Stop();
            
            // If it took too long, debugger might be attached
            return sw.ElapsedMilliseconds < TimingThresholdMs;
        }

        /// <summary>
        /// Computes SHA256 hash of file contents.
        /// </summary>
        public static string ComputeHash(byte[] data)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(data);
            var sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Verifies a specific file against its expected hash.
        /// </summary>
        public static bool VerifyFile(string filePath, string expectedHash)
        {
            if (!File.Exists(filePath))
                return false;

            byte[] data = File.ReadAllBytes(filePath);
            string actualHash = ComputeHash(data);
            return actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
