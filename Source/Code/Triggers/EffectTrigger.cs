namespace Celeste.Triggers {
    /// <summary>
    /// Trigger for various visual effects like particles, screen shake, etc.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/EffectTrigger")]
    public class EffectTrigger : Trigger {
        public enum EffectType {
            Sparkles,
            ScreenShake,
            ColorGrade,
            Wind,
            Lightning
        }

        private EffectType effectType;
        private float intensity;
        private float duration;
        private bool triggerOnce;
        private bool hasTriggered;
        private Level level;

        public EffectTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            string effectName = data.Attr(nameof(effectType), "sparkles");
            if (!System.Enum.TryParse(effectName, true, out effectType)) {
                effectType = EffectType.Sparkles;
            }

            intensity = data.Float(nameof(intensity), 1.0f);
            duration = data.Float(nameof(duration), 3.0f);
            triggerOnce = data.Bool(nameof(triggerOnce), true);
            hasTriggered = false;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = scene as Level;
        }

        public override void OnEnter(global::Celeste.Player player) {
            base.OnEnter(player);

            if (!triggerOnce || !hasTriggered) {
                triggerEffect();
                hasTriggered = true;
            }
        }

        private void triggerEffect() {
            if (level == null) return;

            switch (effectType) {
                case EffectType.Sparkles:
                    createSparkleEffect();
                    break;
                case EffectType.ScreenShake:
                    createScreenShake();
                    break;
                case EffectType.ColorGrade:
                    createColorGradeEffect();
                    break;
                case EffectType.Wind:
                    createWindEffect();
                    break;
                case EffectType.Lightning:
                    createLightningEffect();
                    break;
            }

            // Play appropriate sound
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        private void createSparkleEffect() {
            for (int i = 0; i < (int)(20 * intensity); i++) {
                Vector2 particlePos = Position + new Vector2(
                    Calc.Random.Range(-Width / 2, Width / 2),
                    Calc.Random.Range(-Height / 2, Height / 2)
                );

                level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, Color.Gold);
            }
        }

        private void createScreenShake() {
            level.Shake(duration * intensity);
        }

        private void createColorGradeEffect() {
            // This would need custom implementation for color grading
            // For now, just add some visual feedback
            level.Flash(Color.Purple * (intensity * 0.3f));
        }

        private void createWindEffect() {
            // Create wind particles
            for (int i = 0; i < (int)(15 * intensity); i++) {
                Vector2 particlePos = Position + new Vector2(
                    Calc.Random.Range(-Width, Width),
                    Calc.Random.Range(-Height, Height)
                );

                level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos);
            }
        }

        private void createLightningEffect() {
            // Flash effect to simulate lightning
            level.Flash(Color.White * intensity);

            // Add some particle effects
            for (int i = 0; i < (int)(10 * intensity); i++) {
                Vector2 particlePos = Position + new Vector2(
                    Calc.Random.Range(-Width / 2, Width / 2),
                    Calc.Random.Range(-Height / 2, Height / 2)
                );

                level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, Color.Cyan);
            }
        }
    }
}




