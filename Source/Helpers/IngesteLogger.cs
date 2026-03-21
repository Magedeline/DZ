using Celeste.Mod;

namespace MaggyHelper.Helpers
{
    /// <summary>
    /// Centralized logging helper for the Ingeste/MaggyHelper mod.
    /// Wraps Celeste's Logger with a consistent tag.
    /// </summary>
    public static class IngesteLogger
    {
        private const string Tag = "MaggyHelper";

        public static void Info(string message)
        {
            Logger.Log(LogLevel.Info, Tag, message);
        }

        public static void Debug(string message)
        {
            Logger.Log(LogLevel.Debug, Tag, message);
        }

        public static void Warn(string message)
        {
            Logger.Log(LogLevel.Warn, Tag, message);
        }

        public static void Error(string message)
        {
            Logger.Log(LogLevel.Error, Tag, message);
        }

        public static void Error(Exception exception, string message)
        {
            Logger.Log(LogLevel.Error, Tag, $"{message}\n{exception}");
        }

        public static void Error(string message, Exception exception)
        {
            Error(exception, message);
        }
    }
}
