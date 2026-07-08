namespace Celeste
{
    /// <summary>
    /// Extensions for PlayerSpriteMode.
    /// Provides additional functionality for player sprite modes.
    /// </summary>
    public static class PlayerSpriteModeExtensions
    {
        /// <summary>
        /// Check if a sprite mode is valid.
        /// </summary>
        public static bool IsValid(this PlayerSpriteMode mode)
        {
            // Stub implementation - returns true by default
            return true;
        }

        /// <summary>
        /// Get the next sprite mode.
        /// </summary>
        public static PlayerSpriteMode Next(this PlayerSpriteMode mode)
        {
            // Stub implementation - returns the same mode
            return mode;
        }

        /// <summary>
        /// Get the sprite bank ID for a sprite mode.
        /// </summary>
        public static string GetSpriteBankId(this PlayerSpriteMode mode)
        {
            // Stub implementation - returns empty string
            return "";
        }

        /// <summary>
        /// Custom sprite mode for Chara character.
        /// </summary>
        public const PlayerSpriteMode Chara = (PlayerSpriteMode)100;
    }
}
