using Celeste.Effects;

namespace Celeste.Entities
{
    /// <summary>
    /// Trigger entity that creates elemental effects when activated
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/ElementalEffectTrigger")]
    [HotReloadable]
    public class ElementalEffectTrigger : Trigger
    {
        private string effectType;
        private string elementType;
        private float intensity;
        private float radius;
        private float duration;
        private bool triggerOnEnter;
        private bool triggerOnExit;
        private bool oneUse;
        private bool hasTriggered;

        public ElementalEffectTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            effectType = data.Attr("effectType", "fire_burst");
            elementType = data.Attr("elementType", "Fire");
            intensity = data.Float("intensity", 1f);
            radius = data.Float("radius", 32f);
            duration = data.Float("duration", 1f);
            triggerOnEnter = data.Bool("triggerOnEnter", true);
            triggerOnExit = data.Bool("triggerOnExit", false);
            oneUse = data.Bool("oneUse", false);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            if (triggerOnEnter && (!oneUse || !hasTriggered))
            {
                TriggerEffect(player);
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            if (triggerOnExit && (!oneUse || !hasTriggered))
            {
                TriggerEffect(player);
            }
        }

        private void TriggerEffect(global::Celeste.Player player)
        {
            if (hasTriggered && oneUse) return;
            
            hasTriggered = true;
            
            var level = Scene as Level;
            if (level == null) return;

            // Create effect parameters based on trigger settings
            var parameters = new Dictionary<string, object>
            {
                ["intensity"] = intensity,
                ["radius"] = radius,
                ["duration"] = duration
            };

            // Add player-relative positioning for directional effects
            Vector2 playerDirection = (player.Position - Center).SafeNormalize();
            parameters["direction"] = playerDirection;

            // Play the elemental effect
            ElementalEffectsManager.PlayEffect(effectType, level, Center, parameters);
            
            // Log the trigger activation
            IngesteLogger.Debug($"ElementalEffectTrigger: Activated {effectType} effect at {Center}");
        }
    }
}
