namespace Celeste
{
    /// <summary>
    /// Registry for character abilities.
    /// Manages which abilities are available to which characters.
    /// </summary>
    public static class CharacterAbilityRegistry
    {
        /// <summary>
        /// Register an ability for a character.
        /// </summary>
        public static void RegisterAbility(string character, string ability)
        {
            // Stub implementation - does nothing
        }

        /// <summary>
        /// Check if a character has an ability.
        /// </summary>
        public static bool HasAbility(string character, string ability)
        {
            // Stub implementation - returns false by default
            return false;
        }

        /// <summary>
        /// Get all abilities for a character.
        /// </summary>
        public static string[] GetAbilities(string character)
        {
            // Stub implementation - returns empty array
            return System.Array.Empty<string>();
        }

        /// <summary>
        /// Deactivate all characters.
        /// </summary>
        public static void DeactivateAllCharacters()
        {
            // Stub implementation - does nothing
        }

        /// <summary>
        /// Activate a specific character.
        /// </summary>
        public static void ActivateCharacter(string character)
        {
            // Stub implementation - does nothing
        }

        /// <summary>
        /// Activate a specific character with additional parameters.
        /// </summary>
        public static void ActivateCharacter(string character, bool activate, bool immediate)
        {
            // Stub implementation - does nothing
        }

        /// <summary>
        /// Get a character by name.
        /// </summary>
        public static string GetCharacter(string characterName)
        {
            // Stub implementation - returns the name
            return characterName;
        }
    }
}
