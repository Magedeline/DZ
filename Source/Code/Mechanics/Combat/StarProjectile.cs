namespace Celeste.Entities.Projectiles
{
    public class StarProjectile : Entity
    {
        private Vector2 velocity;
        private Color color;
        private float lifeTime;
        private const float MAX_LIFETIME = 5f;
        private Sprite sprite;
        private VertexLight light;

        public StarProjectile(Vector2 position, Vector2 velocity, Color color) : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            this.lifeTime = MAX_LIFETIME;

            // Collider for player interaction
            Collider = new Circle(6f);
            Add(new PlayerCollider(OnPlayerCollide));

            // Add visual components
            sprite = ((SpriteBank)IngesteModule.SpriteBank).Create("starProjectile");
            sprite.Play("spin");
            Add(sprite);

            // Add light effect
            light = new VertexLight(color, 0.8f, 16, 32);
            Add(light);
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

            // Check for collision with solid walls
            if (CollideCheck<Solid>())
            {
                Explode();
                return;
            }
        }

        private void OnPlayerCollide(global::Celeste.Player player)
        {
            player.Die(new Vector2(Math.Sign(velocity.X), 0f));
            Explode();
        }


        private void Explode()
        {
            // Create particle effect
            Level level = SceneAs<Level>();
            for (int i = 0; i < 8; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                Vector2 direction = Calc.AngleToVector(angle, 1);
                level.Particles.Emit(GetP_StarExplosion(),
                    Position + direction * 4f);
            }

            // Play sound effect
            Audio.Play("event:/game/undertale/star_burst", Position);
            RemoveSelf();
        }

        private static ParticleType GetP_StarExplosion()
        {
            return IngesteModule.P_StarExplosion;
        }
    }
}



