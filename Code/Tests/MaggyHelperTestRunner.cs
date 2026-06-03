using System;
using System.Collections.Generic;
using System.Diagnostics;
using Celeste.Entities;
using Monocle;
using Celeste.Mod.KIRBY_CELESTE.Bosses;

namespace Celeste.Mod.KIRBY_CELESTE
{
    /// <summary>
    /// Lightweight in-game test runner for KIRBY_CELESTE components.
    /// Run via console command: kirby_test_run
    /// </summary>
    public static class KIRBY_CELSTETestRunner
    {
        private static List<TestResult> _results = new();
        private static int _passed = 0;
        private static int _failed = 0;

        public static void RunAllTests()
        {
            _results.Clear();
            _passed = 0;
            _failed = 0;

            Logger.Log(LogLevel.Info, "KIRBY_CELESTE/Tests", "=== Starting KIRBY_CELESTE Test Suite ===");
            var sw = Stopwatch.StartNew();

            RunTriggerManagerTests();
            RunSaveDataValidatorTests();
            RunBossConfigHelperTests();

            sw.Stop();
            Logger.Log(LogLevel.Info, "KIRBY_CELESTE/Tests",
                $"=== Test Complete: {_passed} passed, {_failed} failed, {sw.ElapsedMilliseconds}ms ===");

            foreach (var result in _results)
            {
                var level = result.Passed ? LogLevel.Info : LogLevel.Error;
                Logger.Log(level, "KIRBY_CELESTE/Tests", $"  [{result.Category}] {result.Name}: {(result.Passed ? "PASS" : "FAIL")}");
                if (!result.Passed && !string.IsNullOrEmpty(result.Message))
                {
                    Logger.Log(LogLevel.Error, "KIRBY_CELESTE/Tests", $"    -> {result.Message}");
                }
            }
        }

        #region TriggerManager Tests

        private static void RunTriggerManagerTests()
        {
            LogTestCategory("TriggerManager");

            // Test SafeInt with valid data
            var validData = new EntityData();
            typeof(EntityData).GetProperty("Values")?.SetValue(validData, new Dictionary<string, object> { { "health", 100 } });
            
            // Test null safety
            Assert("SafeInt_NullData_ReturnsDefault",
                TriggerManager.SafeInt(null, "health", 50) == 50,
                "Should return default when EntityData is null");

            Assert("SafeFloat_NullData_ReturnsDefault",
                TriggerManager.SafeFloat(null, "speed", 1.5f) == 1.5f,
                "Should return default when EntityData is null");

            Assert("SafeBool_NullData_ReturnsDefault",
                TriggerManager.SafeBool(null, "active", true) == true,
                "Should return default when EntityData is null");

            Assert("SafeString_NullData_ReturnsDefault",
                TriggerManager.SafeString(null, "name", "default") == "default",
                "Should return default when EntityData is null");
        }

        #endregion

        #region SaveDataValidator Tests

        private static void RunSaveDataValidatorTests()
        {
            LogTestCategory("SaveDataValidator");

            // Test validation state reset
            SaveDataValidator.ResetValidationState();
            Assert("ResetValidationState_ClearsLog",
                SaveDataValidator.GetLastValidationLog().Count == 0,
                "Validation log should be empty after reset");

            // Test that validation doesn't crash with null save data
            try
            {
                SaveDataValidator.ValidateOnLoad();
                Assert("ValidateOnLoad_DoesNotCrash", true, "");
            }
            catch (Exception ex)
            {
                Assert("ValidateOnLoad_DoesNotCrash", false, $"Exception: {ex.Message}");
            }
        }

        #endregion

        #region BossConfigHelper Tests

        private static void RunBossConfigHelperTests()
        {
            LogTestCategory("BossConfigHelper");

            Assert("ReadHealthConfig_NullData_ReturnsDefaults",
                TestDoesNotThrow(() => BossConfigHelper.ReadHealthConfig(null, "TestBoss")),
                "Should handle null EntityData gracefully");
        }

        #endregion

        #region Test Helpers

        private static void LogTestCategory(string category)
        {
            Logger.Log(LogLevel.Info, "KIRBY_CELESTE/Tests", $"--- {category} ---");
        }

        private static void Assert(string name, bool condition, string message)
        {
            _results.Add(new TestResult { Category = "General", Name = name, Passed = condition, Message = message });
            if (condition) _passed++; else _failed++;
        }

        private static bool TestDoesNotThrow(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private struct TestResult
        {
            public string Category;
            public string Name;
            public bool Passed;
            public string Message;
        }

        #endregion

        #region Console Registration

        public static void RegisterConsoleCommand()
        {
            try
            {
                var cmdType = typeof(Everest).Assembly.GetType("Celeste.Mod.Everest+Commands");
                if (cmdType != null)
                {
                    var registerMethod = cmdType.GetMethod("Register", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    registerMethod?.Invoke(null, new object[] { "maggy_test_run", (Action)(() =>
                    {
                        RunAllTests();
                    })});
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/Tests", $"Could not register test command: {ex.Message}");
            }
        }

        #endregion
    }
}
