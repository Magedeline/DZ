using global::Celeste.Extensions.Core;

namespace DZ
{
    /// <summary>
    /// Registry for character abilities and data.
    /// Provides a central location for looking up character-specific information.
    /// </summary>
    public static class CharacterAbilityRegistry
    {
        /// <summary>
        /// Character data structure containing sprite and ability information.
        /// </summary>
        public struct CharacterData
        {
            /// <summary>
            /// The sprite ID used for this character in GFX.SpriteBank.
            /// </summary>
            public string SpriteId { get; set; }
            
            /// <summary>
            /// Display name of the character.
            /// </summary>
            public string DisplayName { get; set; }
            
            /// <summary>
            /// Whether this character has special abilities.
            /// </summary>
            public bool HasSpecialAbilities { get; set; }
            
            /// <summary>
            /// Create character data with specified parameters.
            /// </summary>
            public CharacterData(string spriteId, string displayName = null, bool hasSpecialAbilities = false)
            {
                SpriteId = spriteId;
                DisplayName = displayName ?? spriteId;
                HasSpecialAbilities = hasSpecialAbilities;
            }
        }
        
        private static readonly Dictionary<string, CharacterData> characters = new Dictionary<string, CharacterData>(StringComparer.OrdinalIgnoreCase)
        {
            { PlayerCharacterIds.Default, new CharacterData("maggy_player", "Madeline") },
            { PlayerCharacterIds.Madeline, new CharacterData("maggy_player", "Madeline") },
            { "badeline", new CharacterData("maggy_badeline", "Badeline") },
            { PlayerCharacterIds.Kirby, new CharacterData("kirby", "Kirby", true) },
            { "kirby_classic", new CharacterData("kirby_classic", "Kirby (Classic)", true) },
            { "meta_knight", new CharacterData("meta_knight", "Meta Knight", true) },
            { "king_dedede", new CharacterData("king_dedede", "King Dedede", true) },
            { "bandana_waddle_dee", new CharacterData("bandana_waddle_dee", "Bandana Waddle Dee", true) },
            { "adeline", new CharacterData("adeline", "Adeline", true) },
            { "gooey", new CharacterData("gooey", "Gooey", true) },
            { "marx", new CharacterData("marx", "Marx", true) },
            { "magolor", new CharacterData("magolor", "Magolor", true) },
            { "taranza", new CharacterData("taranza", "Taranza", true) },
            { "susie", new CharacterData("susie_haltmann", "Susie", true) },
            { "dark_meta_knight", new CharacterData("dark_meta_knight", "Dark Meta Knight", true) },
            { "frisk", new CharacterData("frisk", "Frisk") },
            { "chara", new CharacterData("maggy_chara", "Chara") },
            { "asriel", new CharacterData("asriel", "Asriel") },
            { "ralsei", new CharacterData("ralsei", "Ralsei") },
            { "ness", new CharacterData("ness", "Ness") },
        };
        
        /// <summary>
        /// Get character data by ID.
        /// </summary>
        /// <param name="characterId">The character ID to look up.</param>
        /// <returns>Character data if found, null otherwise.</returns>
        public static CharacterData? GetCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return null;

            characterId = PlayerCharacter.NormalizeId(characterId);
                
            if (characters.TryGetValue(characterId, out var data))
                return data;
                
            return null;
        }
        
        /// <summary>
        /// Register a new character.
        /// </summary>
        /// <param name="characterId">Unique character ID.</param>
        /// <param name="data">Character data.</param>
        public static void RegisterCharacter(string characterId, CharacterData data)
        {
            if (string.IsNullOrEmpty(characterId))
                return;
                
            characters[characterId] = data;
        }
        
        /// <summary>
        /// Check if a character exists in the registry.
        /// </summary>
        public static bool HasCharacter(string characterId)
        {
            return !string.IsNullOrEmpty(characterId) && characters.ContainsKey(characterId);
        }
        
        /// <summary>
        /// Get all registered character IDs.
        /// </summary>
        public static IEnumerable<string> GetAllCharacterIds()
        {
            return characters.Keys;
        }
        
        /// <summary>
        /// Activate a character's abilities for a player.
        /// </summary>
        public static void ActivateCharacter(string characterId, global::Celeste.Player player, Level level)
        {
            if (string.IsNullOrEmpty(characterId) || player == null)
                return;
                
            // Store the active character in session data if available
            var session = IngesteModule.Session;
            if (session != null)
            {
                // Session could track active character here
            }
            
            Logger.Log(LogLevel.Info, "CharacterAbilityRegistry", 
                $"Activated character: {characterId}");
        }
        
        /// <summary>
        /// Deactivate all character abilities.
        /// </summary>
        public static void DeactivateAllCharacters()
        {
            Logger.Log(LogLevel.Info, "CharacterAbilityRegistry", 
                "Deactivated all character abilities");
        }
        
        /// <summary>
        /// Get the currently active character ID.
        /// </summary>
        public static string GetActiveCharacterId()
        {
            return PlayerCharacterIds.Madeline;
        }
    }
}
