namespace DZ
{
    /// <summary>
    /// Exports for the DZ mod.
    /// Provides external API access to DZ functionality.
    /// </summary>
    public static class DZExports
    {
        /// <summary>
        /// Get the current DZ version.
        /// </summary>
        public static string GetVersion()
        {
            return "1.0.0";
        }

        /// <summary>
        /// Check if DZ is loaded.
        /// </summary>
        public static bool IsLoaded()
        {
            return true;
        }
    }
}
