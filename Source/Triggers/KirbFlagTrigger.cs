namespace Celeste.Triggers
{
    [CustomEntity(ids: "MaggyHelper/KirbFlagTrigger")]
    [Monocle.Tracked]
    public class KirbFlagTrigger : Trigger
    {
        public static ParticleType P_Sparkle;
        public static ParticleType P_Burst;

        private bool triggered = false;

        public KirbFlagTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggered)
                return;

            var level = Scene as Level;
            if (level == null)
                return;

            // Check if already collected
            if (level.Session.GetFlag("kirb_flag_collected"))
                return;

            triggered = true;

            // Set the session flag
            level.Session.SetFlag("kirb_flag_collected", true);

            // Play the final flag sound effect
            Audio.Play("event:/pusheen/final_content/game/19_the_end/kirb_flag", player.Center);

            // Visual effects
            Add(new Coroutine(CollectRoutine(player)));
        }

        private IEnumerator CollectRoutine(global::Celeste.Player player)
        {
            var level = Scene as Level;
            if (level == null)
                yield break;
            
            // Ensure particles are loaded
            if (P_Sparkle == null || P_Burst == null)
            {
                LoadParticles();
            }
            
            // Burst of pink sparkles at player position
            level.ParticlesFG?.Emit(P_Burst, 30, player.Center, Vector2.One * 24f, Color.Pink);
            level.ParticlesBG?.Emit(P_Sparkle, 20, player.Center, Vector2.One * 20f, Color.Pink);
            
            // Screen effects
            level.Shake(0.4f);
            level.Flash(Color.Pink * 0.6f, true);
            
            // Continue emitting particles for a bit
            for (float t = 0f; t < 1.0f; t += Engine.DeltaTime)
            {
                if (t < 0.5f)
                {
                    level.ParticlesFG?.Emit(P_Sparkle, 3, player.Center + new Vector2(Calc.Random.Range(-16f, 16f), Calc.Random.Range(-16f, 16f)), Vector2.One * 8f, Color.Pink);
                }
                yield return null;
            }
        }

        private static void LoadParticles()
        {
            P_Sparkle = new ParticleType
            {
                Size = 1.2f,
                Color = Color.Pink,
                Color2 = Color.HotPink,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.6f,
                SpeedMin = 8f,
                SpeedMax = 16f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMultiplier = 0.7f
            };
            
            P_Burst = new ParticleType
            {
                Size = 2.0f,
                Color = Color.Pink,
                Color2 = Color.HotPink,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 15f,
                SpeedMax = 35f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 15f)
            };
        }
    }
}




