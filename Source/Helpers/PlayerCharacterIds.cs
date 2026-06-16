namespace Celeste
{
    /// <summary>
    /// IDs for player characters.
    /// Provides constants for different playable characters.
    /// </summary>
    public static class PlayerCharacterIds
    {
        public const string Madeline = "madeline";
        public const string MadelineAlt = "madeline_alt";
        public const string Badeline = "badeline";
        public const string Kirby = "kirby";
        public const string Asriel = "asriel";

        /// <summary>
        /// Get the character ID for a given character name.
        /// </summary>
        public static string GetId(string characterName)
        {
            return characterName.ToLower();
        }
    }
}
