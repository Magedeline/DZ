namespace MaggyHelper.Entities
{
    [HotReloadable]
    public static class BadelineDash
    {
        public static ParticleType PBurst;

        static BadelineDash()
        {
            PBurst = new ParticleType
            {
                Color = Color.Purple,
                Color2 = Color.DarkMagenta,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                Direction = (float)(-System.Math.PI / 2.0),
                DirectionRange = (float)(System.Math.PI / 8.0),
                SpeedMin = 60f,
                SpeedMax = 120f,
                SpeedMultiplier = 0.01f,
                Acceleration = Vector2.UnitY * 40f,
                LifeMin = 0.8f,
                LifeMax = 1.2f
            };
        }
    }
}



