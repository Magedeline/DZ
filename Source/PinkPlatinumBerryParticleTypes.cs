namespace MaggyHelper
{
    public static class PinkPlatinumBerryParticles
    {
        public static ParticleType PGhostGlow { get; private set; }
        public static ParticleType PPlatGlow { get; private set; }

        static PinkPlatinumBerryParticles() {
            // Initialize ghost glow particle type
            PGhostGlow = new ParticleType();
            PGhostGlow.Color = Color.Pink;
            PGhostGlow.Color2 = Color.HotPink;
            PGhostGlow.ColorMode = ParticleType.ColorModes.Blink;
            PGhostGlow.FadeMode = ParticleType.FadeModes.Late;
            PGhostGlow.Size = 1f;
            PGhostGlow.SizeRange = 0.5f;
            PGhostGlow.Direction = -(float)System.Math.PI / 2f;
            PGhostGlow.DirectionRange = (float)System.Math.PI / 4f;
            PGhostGlow.SpeedMultiplier = 0.01f;
            PGhostGlow.Acceleration = Vector2.UnitY * 5f;
            PGhostGlow.Friction = 0f;
            PGhostGlow.LifeMin = 0.8f;
            PGhostGlow.LifeMax = 1.2f;
            PGhostGlow.RotationMode = ParticleType.RotationModes.Random;
            PGhostGlow.SpinMin = 1f;
            PGhostGlow.SpinMax = 2f;
        }
    }
}
