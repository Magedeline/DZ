namespace MaggyHelper.Entities.Projectiles
{
    public class FallingStarProjectile : Entity
    {
        private Vector2 velocity;
        private float lifeTime;
        private const float MAX_LIFETIME = 4f;
        private const float FALL_SPEED = 120f;
        private const float WOBBLE_STRENGTH = 40f;
        private Sprite sprite;
        private VertexLight light;
        private SineWave sine;

        public FallingStarProjectile(Vector2 position) : base(position)
        {
            this.velocity = new Vector2(0f, FALL_SPEED);
            this.lifeTime = MAX_LIFETIME;

            // Add collider
            Collider = new Circle(6f);
            Add(new PlayerCollider(OnPlayerCollide));

            // Add visual components
            var spriteBank = IngesteModule.SpriteBank as SpriteBank;
            if (spriteBank != null)
            {
                sprite = spriteBank.Create("fallingStar");
                if (sprite != null)
                {
                    sprite.Play("fall");
                    Add(sprite);
                }
            }

            // Add effects
            Add(light = new VertexLight(Color.Gold, 0.8f, 16, 32));
            Add(sine = new SineWave(1.5f));
        }

        public override void Update()
        {
            base.Update();

            // Add wobble movement
            velocity.X = sine.Value * WOBBLE_STRENGTH;

            // Update position
            Position += velocity * Engine.DeltaTime;

            // Update lifetime
            lifeTime -= Engine.DeltaTime;
            if (CollideCheck<Solid>())
            {
                Explode();
                return;
            }

            // Update light effect
            if (light != null)
            {
                light.Alpha = 0.8f + sine.Value * 0.2f;
            }
        }

        private void OnPlayerCollide(global::Celeste.Player player)
        {
            if (player != null)
            {
                float knockbackX = Math.Sign(velocity.X);
                if (knockbackX == 0f)
                {
                    knockbackX = 1f; // Default to right if no horizontal movement
                }
                player.Die(new Vector2(knockbackX, 0.5f));
            }
            Explode();
        }

        private void Explode()
        {
            // Create particle effect
            Level level = SceneAs<Level>();
            if (level != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                    Vector2 direction = Calc.AngleToVector(angle, 1f);
                    level.Particles.Emit(IngesteModule.P_StarExplosion,
                        Position + direction * 4f,
                        angle);
                }
            }

            // Play sound effect
            Audio.Play("event:/game/undertale/star_impact", Position);
            RemoveSelf();
        }
    }
}



