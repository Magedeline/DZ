namespace MaggyHelper.Entities
{
    [HotReloadable]
    public class CelestialBlade : Entity
    {
        private const float Speed = 180f;
        private const float RotationSpeed = 8f;
        
        private Vector2 velocity;
        private float rotation;
        private float lifeTime;
        private float maxLifeTime;
        private bool returning;
        private Entity owner;
        private VertexLight light;
        
        public CelestialBlade(Vector2 position, Vector2 direction, Entity owner, float lifetime = 2f) : base(position)
        {
            this.owner = owner;
            velocity = direction * Speed;
            rotation = (float)Math.Atan2(direction.Y, direction.X);
            maxLifeTime = lifetime;
            lifeTime = maxLifeTime;
            returning = false;
            
            // Add light effect
            Add(light = new VertexLight(Color.LightBlue, 0.5f, 16, 24));
            
            // Add collision
            Collider = new Hitbox(12f, 4f, -6f, -2f);
        }
        
        public override void Update()
        {
            base.Update();
            
            rotation += RotationSpeed * Engine.DeltaTime;
            
            if (!returning)
            {
                lifeTime -= Engine.DeltaTime;
                if (lifeTime <= 0)
                {
                    returning = true;
                }
            }
            else if (owner != null && owner.Scene != null)
            {
                // Return to owner
                Vector2 toOwner = (owner.Center - Position).SafeNormalize();
                velocity = toOwner * Speed * 1.5f;
                
                // Check if caught by owner
                if (Vector2.DistanceSquared(Position, owner.Center) < 100f)
                {
                    RemoveSelf();
                }
            }
            else
            {
                RemoveSelf();
            }
            
            // Update position
            Position += velocity * Engine.DeltaTime;
            
            // Check for player collision
            global::Celeste.Player player = CollideFirst<global::Celeste.Player>();
            if (player != null)
            {
                player.Die(Position);
            }
        }
        
        public override void Render()
        {
            base.Render();
            
            // Draw blade
            float bladeLength = 12f;
            float bladeWidth = 4f;
            
            Vector2 center = Position;
            Vector2 right = Calc.AngleToVector(rotation, bladeLength / 2);
            Vector2 up = Calc.AngleToVector(rotation + MathHelper.PiOver2, bladeWidth / 2);
            
            Color bladeColor = Color.Lerp(Color.White, Color.LightBlue, 0.5f);
            
            // Draw blade shape
            Draw.Line(center - right - up, center + right - up, bladeColor);
            Draw.Line(center + right - up, center + right + up, bladeColor);
            Draw.Line(center + right + up, center - right + up, bladeColor);
            Draw.Line(center - right + up, center - right - up, bladeColor);
            
            // Draw energy trail
            if (Scene is Level level)
            {
                Vector2 trailPos = Position - velocity.SafeNormalize() * 4f;
                level.ParticlesFG.Emit(P_BladeTrail, trailPos);
            }
        }
        
        // Custom particle type for blade trail
        public static ParticleType P_BladeTrail = new ParticleType
        {
            Source = GFX.Game["particles/pixel"],
            Color = Color.LightBlue,
            Color2 = Color.White * 0f,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.2f,
            LifeMax = 0.4f,
            Size = 1f,
            SpeedMin = 10f,
            SpeedMax = 20f,
            DirectionRange = 0.5f,
            SpeedMultiplier = 0.8f
        };
    }
}



