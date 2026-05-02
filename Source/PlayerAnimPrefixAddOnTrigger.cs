using MonoMod.Utils;

namespace Celeste.Triggers {
    [CustomEntity("SkinModHelper/PlayerAnimPrefixAddOnTrigger")]
    [HotReloadable]
    public class PlayerAnimPrefixAddOnTrigger : Trigger {

        private static readonly Dictionary<string, string> CharacterAnimationPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "king_ddd", "kingddd_" },
            { "meta_knight", "metaknight_" },
            { "bandana_waddle_dee", "bwdee_" },
            { "magolor", "magolor_" },
            { "susie_haltmann", "susie_" },
            { "taranza", "taranza_" },
            { "squeaker", "squeaker_" },
            { "dark_meta_knight", "dmk_" },
            { "marx", "marx_" },
            { "francisca", "francisca_" },
            { "flamberge", "flamberge_" },
            { "zan_partizanne", "zan_" },
            { "kirby_classic", "kirby_" },
            { "gooey", "gooey_" },
            { "asriel", "asriel_" },
            { "frisk", "frisk_" },
            { "charlo", "charlo_" },
            { "clover", "clover_" },
            { "melody", "melody_" },
            { "batty", "batty_" },
            { "emily", "emily_" },
            { "cody", "cody_" },
            { "odin", "odin_" },
            { "adeleine", "adeleine_" },
            { "ness", "ness_" }
        };

        private static readonly Dictionary<string, string> CharacterIdleAnimations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "king_ddd", "kingddd_idle" },
            { "meta_knight", "metaknight_idle" },
            { "bandana_waddle_dee", "bwdee_idle" },
            { "magolor", "magolor_idle" },
            { "susie_haltmann", "susie_idle" },
            { "taranza", "taranza_idle" },
            { "squeaker", "squeaker_idle" },
            { "dark_meta_knight", "dmk_idle" },
            { "marx", "marx_idle" },
            { "francisca", "francisca_idle" },
            { "flamberge", "flamberge_idle" },
            { "zan_partizanne", "zan_idle" },
            { "kirby_classic", "kirby_idle" },
            { "gooey", "gooey_idle" },
            { "asriel", "asriel_idle" },
            { "frisk", "frisk_idle" },
            { "charlo", "charlo_idle" },
            { "clover", "clover_idle" },
            { "melody", "melody_idle" },
            { "batty", "batty_idle" },
            { "emily", "emily_idle" },
            { "cody", "cody_idle" },
            { "odin", "odin_idle" },
            { "adeleine", "adeleine_idle" },
            { "ness", "ness_idle" }
        };

        private string lastAnimPrefixAddOn;
        private readonly string animPrefixAddOn;
        private readonly bool revertOnLeave;
        
        // Character system properties
        private readonly string characterId;
        private readonly string characterAnimPrefix;
        private readonly bool useCharacterAnimations;

        public PlayerAnimPrefixAddOnTrigger(EntityData data, Vector2 offset) 
            : base(data, offset) {
            revertOnLeave = data.Bool("revertOnLeave", false);
            animPrefixAddOn = data.Attr("animPrefixAddOn", null);
            
            // Character system properties
            characterId = data.Attr("characterId", "");
            characterAnimPrefix = data.Attr("characterAnimPrefix", "");
            useCharacterAnimations = data.Bool("useCharacterAnimations", true);
        }

        public override void OnEnter(CelestePlayer player) {
            base.OnEnter(player);
            if (player?.Sprite == null) {
                return;
            }
            
            // Store previous animation prefix for reversion
            if (revertOnLeave)
            {
                var dynamicData = DynamicData.For(player.Sprite);
                lastAnimPrefixAddOn = dynamicData.Get<string>("smh_AnimPrefix");
            }

            // Determine which animation prefix to use
            string newAnimPrefix = GetAnimationPrefix();
            
            // Apply the animation prefix
            if (!string.IsNullOrEmpty(newAnimPrefix))
            {
                ApplyAnimationPrefix(player, newAnimPrefix);
            }
        }

        public override void OnLeave(CelestePlayer player) {
            base.OnLeave(player);
            if (player?.Sprite == null) {
                return;
            }
            
            if (revertOnLeave) {
                // Restore previous animation prefix
                if (!player.Dead) {
                    DynamicData.For(player.Sprite).Set("smh_AnimPrefix", lastAnimPrefixAddOn);
                    player.Sprite.Play("idle");
                }
                lastAnimPrefixAddOn = null;
            }
        }
        
        public override void SceneEnd(Scene scene) {
            if (revertOnLeave && PlayerIsInside) {
                lastAnimPrefixAddOn = null;
            }
            base.SceneEnd(scene);
        }
        
        private string GetAnimationPrefix()
        {
            // Priority: character-specific prefix > manual prefix > character default
            if (!string.IsNullOrEmpty(characterAnimPrefix))
            {
                return characterAnimPrefix;
            }
            
            if (!string.IsNullOrEmpty(animPrefixAddOn))
            {
                return animPrefixAddOn;
            }
            
            // Get character-specific animation prefix if available
            if (useCharacterAnimations && !string.IsNullOrEmpty(characterId))
            {
                var characterData = CharacterAbilityRegistry.GetCharacter(characterId);
                if (characterData.HasValue)
                {
                    return GetCharacterAnimationPrefix(characterId);
                }
            }
            
            return null;
        }
        
        private string GetCharacterAnimationPrefix(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return null;
            return CharacterAnimationPrefixes.TryGetValue(charId, out string prefix) ? prefix : null;
        }
        
        private void ApplyAnimationPrefix(CelestePlayer player, string prefix)
        {
            try
            {
                // Set the animation prefix
                DynamicData.For(player.Sprite).Set("smh_AnimPrefix", prefix);
                
                // Apply character-specific animation if available
                string animationId = GetCharacterIdleAnimation(characterId) ?? "idle";
                player.Sprite.Play(animationId);
                
                Logger.Log(LogLevel.Debug, "PlayerAnimPrefixAddOnTrigger", 
                    $"Applied animation prefix '{prefix}' with animation '{animationId}' for character '{characterId}'");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "PlayerAnimPrefixAddOnTrigger", 
                    $"Failed to apply animation prefix: {ex.Message}");
            }
        }
        
        private string GetCharacterIdleAnimation(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return "idle";
            return CharacterIdleAnimations.TryGetValue(charId, out string animation) ? animation : "idle";
        }
    }
}



