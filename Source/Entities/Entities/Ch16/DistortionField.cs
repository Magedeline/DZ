namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// DistortionField - Area that distorts player movement
    /// Reverses controls or applies random forces
    /// Sprite path: effects/distortion_field/
    /// </summary>
    [CustomEntity("DZ/DistortionField")]
    [Tracked]
    public class DistortionField : Trigger
    {
        #region Enums
        public enum DistortionType
        {
            Reverse,
            Random,
            GravityFlip,
            SlowMotion
        }
        #endregion

        #region Properties
        public DistortionType Type { get; private set; }
        public float Intensity { get; private set; }
        #endregion

        #region Constructor
        public DistortionField(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Initialize(data.Enum("distortionType", DistortionType.Reverse), data.Float("intensity", 1f));
        }

        public DistortionField(EntityData data, Vector2 offset, DistortionType type, float intensity)
            : base(data, offset)
        {
            Initialize(type, intensity);
        }

        private void Initialize(DistortionType type, float intensity)
        {
            Type = type;
            Intensity = intensity;
            effectTimer = 0f;
        }
        #endregion

        #region Trigger Overrides
        public override void OnEnter(Player player)
        {
            switch (Type)
            {
                case DistortionType.Reverse:
                    // Reverse horizontal movement
                    player.Speed.X = -player.Speed.X * Intensity;
                    break;
                    
                case DistortionType.Random:
                    // Apply random forces
                    player.Speed += new Vector2(
                        Calc.Random.NextFloat() * 100f - 50f,
                        Calc.Random.NextFloat() * 100f - 50f
                    ) * Intensity * Engine.DeltaTime;
                    break;
                    
                case DistortionType.GravityFlip:
                    // Flip gravity effect
                    player.Speed.Y += 200f * Intensity * Engine.DeltaTime;
                    break;
                    
                case DistortionType.SlowMotion:
                    // Slow movement
                    player.Speed *= (1f - Intensity * 0.5f);
                    break;
            }
        }
        #endregion
    }
}
