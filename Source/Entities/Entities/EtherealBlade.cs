namespace Celeste.Entities.Projectiles
{
    public class EtherealBlade : Entity
    {
        private Vector2 velocity;
        private float rotation;
        private float lifeTime;
        private const float MAX_LIFETIME = 1.5f;
        private const float LENGTH = 32f;
        private VertexLight light;
        private SineWave sine;
        private Color baseColor = Color.Cyan;

        public EtherealBlade(Vector2 position, Vector2 velocity) : base(position)
        {
            this.velocity = velocity;
            this.rotation = Calc.Angle(velocity);
            this.lifeTime = MAX_LIFETIME;

            // Add collider
            Collider = new Hitbox(LENGTH, 8f, -LENGTH / 2f, -4f);
            Add(new PlayerCollider(OnPlayerCollide));

            // Add visual effects
            Add(light = new VertexLight(baseColor, 0.5f, 16, 32));
            Add(sine = new SineWave(0.5f));

            // Set depth to appear in front of most entities
            Depth = -100;
        }

        public override void Update()
        {
            base.Update();

            // Update position
            Position += velocity * Engine.DeltaTime;

            // Update lifetime
            lifeTime -= Engine.DeltaTime;
            if (lifeTime <= 0f)
            {
                RemoveSelf();
                return;
            }

            // Check collision with K_Player and SoulPlayer
            foreach (K_Player kirby in Scene.Tracker.GetEntities<K_Player>())
            {
                if (kirby != null && kirby.Collidable && !kirby.Dead && CollideCheck(kirby))
                {
                    kirby.Die(velocity.SafeNormalize());
                    return;
                }
            }

            foreach (SoulPlayer soul in Scene.Tracker.GetEntities<SoulPlayer>())
            {
                if (soul != null && soul.Collidable && !soul.IsDead && CollideCheck(soul))
                {
                    soul.TakeDamage(1);
                    return;
                }
            }

            // Update visual effects
            light.Alpha = 0.5f + sine.Value * 0.3f;
        }

        public override void Render()
        {
            base.Render();

            // Calculate blade points
            Vector2 direction = Calc.AngleToVector(rotation, LENGTH);
            Vector2 start = Position - direction * 0.5f;
            Vector2 end = Position + direction * 0.5f;

            // Render blade trail
            float trailAlpha = (0.7f + sine.Value * 0.3f) * (lifeTime / MAX_LIFETIME);
            Color trailColor = baseColor * trailAlpha;
            
            for (int i = 0; i < 3; i++)
            {
                float offset = (i - 1) * 2f;
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X).SafeNormalize() * offset;
                Draw.Line(start + perpendicular, end + perpendicular, trailColor);
            }

            // Render blade core
            Draw.Line(start, end, Color.White * trailAlpha);
        }

        private void OnPlayerCollide(global::Celeste.Player player)
        {
            // Deal damage to the player
            player.Die(velocity.SafeNormalize());
        }
    }
}



