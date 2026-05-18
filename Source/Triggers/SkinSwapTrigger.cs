
namespace Celeste.Triggers {
    [CustomEntity("MaggyHelper/SkinSwapTrigger")]
    [HotReloadable]
    public class SkinSwapTrigger : Trigger {
        
        private readonly string skinId;
        private readonly bool revertOnLeave;

        private readonly bool playerVariant;
        private readonly bool otherselfVariant;
        private readonly bool silhouetteVariant;
        
        // Character system properties
        private readonly string characterId;
        private readonly bool enableCharacterAbilities;
        private string previousCharacterId;

        private string[] oldskinId = new string[3];
        private const string DEFAULT = "Default";
        
        public SkinSwapTrigger(EntityData data, Vector2 offset) 
            : base(data, offset) {
            skinId = data.Attr("skinId", DEFAULT);
            revertOnLeave = data.Bool("revertOnLeave", false);

            playerVariant = data.Bool("playerVariant", true);
            otherselfVariant = data.Bool("otherselfVariant", true);
            silhouetteVariant = data.Bool("silhouetteVariant", false);
            
            // Character system properties
            characterId = data.Attr("characterId", "");
            enableCharacterAbilities = data.Bool("enableCharacterAbilities", true);

            if (string.IsNullOrEmpty(skinId)) {
                skinId = "Null";
            } else if (skinId.EndsWith("_NB")) {
                skinId = skinId.Remove(skinId.Length - 3);
            }
        }

        public override void OnEnter(CelestePlayer player) {
            base.OnEnter(player);
            
            // Store previous character for reversion
            previousCharacterId = GetCurrentCharacterId();

            // Handle character ability swapping
            if (!string.IsNullOrEmpty(characterId) && enableCharacterAbilities)
            {
                SwapToCharacter(characterId, player);
            }
            
            Logger.Log(LogLevel.Info, "MaggyHelper/SkinSwapTrigger", $"Swapped to character: {characterId}");
        }

        public override void OnLeave(CelestePlayer player) {
            base.OnLeave(player);
            
            if (revertOnLeave) {
                // Revert character abilities
                if (!string.IsNullOrEmpty(previousCharacterId))
                {
                    SwapToCharacter(previousCharacterId, player);
                }
                else
                {
                    // Deactivate all character abilities
                    CharacterAbilityRegistry.DeactivateAllCharacters();
                }
            }
        }
        
        public override void SceneEnd(Scene scene) {
            if (revertOnLeave && PlayerIsInside) {
                // Revert character abilities
                if (!string.IsNullOrEmpty(previousCharacterId))
                {
                    var level = scene as Level;
                    var player = level?.Tracker?.GetEntity<CelestePlayer>();
                    if (player != null)
                    {
                        SwapToCharacter(previousCharacterId, player);
                    }
                }
                else
                {
                    CharacterAbilityRegistry.DeactivateAllCharacters();
                }
            }
            base.SceneEnd(scene);
        }
        
        private void SwapToCharacter(string newCharacterId, CelestePlayer player)
        {
            if (string.IsNullOrEmpty(newCharacterId)) return;
            
            var level = SceneAs<Level>();
            if (level == null) return;
            
            try
            {
                // Activate character abilities
                CharacterAbilityRegistry.ActivateCharacter(newCharacterId, player, level);
                
                Logger.Log(LogLevel.Info, "MaggyHelper/SkinSwapTrigger", 
                    $"Activated character abilities for: {newCharacterId}");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper/SkinSwapTrigger", 
                    $"Failed to activate character {newCharacterId}: {ex.Message}");
            }
        }
        
        private string GetCurrentCharacterId()
        {
            // This would need to be tracked in the session or elsewhere
            // For now, return empty string as default
            return "";
        }
    }
}



