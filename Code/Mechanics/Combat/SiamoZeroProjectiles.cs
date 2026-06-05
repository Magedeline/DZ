using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.Projectiles
{
    /// <summary>
    /// Crescent-shaped beam projectile for Siamo Zero's beam attacks.
    /// Travels in a straight line, fading over its lifetime.
    /// </summary>
    public class SiamoZeroCrescentProjectile : Entity
    {
        private Vector2 velocity;
        private float rotation;
        private float lifeTime;
        private const float MAX_LIFETIME = 3.5f;
        private const float WIDTH = 24f;
        private const float HEIGHT = 12f;
        private VertexLight light;
        private SineWave sine;
        private Color baseColor;

        public SiamoZeroCrescentProjectile(Vector2 position, Vector2 velocity, Color color) : base(position)
        {
            this.velocity = velocity;
            this.rotation = Calc.Angle(velocity);
            this.lifeTime = MAX_LIFETIME;
            this.baseColor = color;

            Collider = new Hitbox(WIDTH, HEIGHT, -WIDTH / 2f, -HEIGHT / 2f);
            Add(new PlayerCollider(OnPlayerCollide));
            Add(light = new VertexLight(color, 0.6f, 24, 48));
            Add(sine = new SineWave(0.6f));

            Depth = -100;
        }

        public override void Update()
        {
            base.Update();

            Position += velocity * Engine.DeltaTime;

            lifeTime -= Engine.DeltaTime;
            if (lifeTime <= 0f)
            {
                RemoveSelf();
                return;
            }

            if (CollideCheck<Solid>())
            {
                OnHitWall();
                return;
            }

            light.Alpha = 0.4f + sine.Value * 0.3f;
        }

        public override void Render()
        {
            base.Render();

            float alpha = (0.8f + sine.Value * 0.2f) * (lifeTime / MAX_LIFETIME);
            Color renderColor = baseColor * alpha;

            Vector2 dir = Calc.AngleToVector(rotation, 1f);
            Vector2 perp = new Vector2(-dir.Y, dir.X);

            // Crescent shape: arc of 5 lines
            for (int i = -2; i <= 2; i++)
            {
                float offsetScale = 1f - Math.Abs(i) * 0.3f;
                Vector2 arcOff = perp * (i * 3f);
                Vector2 start = Position - dir * (WIDTH * 0.5f * offsetScale) + arcOff;
                Vector2 end = Position + dir * (WIDTH * 0.5f * offsetScale) + arcOff;
                Draw.Line(start, end, renderColor);
            }

            // Bright core
            Draw.Line(Position - dir * (WIDTH * 0.4f), Position + dir * (WIDTH * 0.4f), Color.White * alpha * 0.9f);
        }

        private void OnPlayerCollide(global::Celeste.Player player)
        {
            player.Die(velocity.SafeNormalize());
        }

        private void OnHitWall()
        {
            var lvl = Scene as Level;
            lvl?.ParticlesFG.Emit(ParticleTypes.Dust, 4, Position, Vector2.One * 4f);

            Audio.Play("event:/game/general/thing_booped", Position);
            RemoveSelf();
        }
    }

    /// <summary>
    /// Energy blade projectile for Siamo Zero's sword attacks.
    /// Short-lived, fast-moving slash effect.
    /// </summary>
    public class SiamoZeroEnergyBlade : Entity
    {
        private Vector2 velocity;
        private float rotation;
        private float lifeTime;
        private float maxLifetime;
        private const float LENGTH = 36f;
        private VertexLight light;
        private SineWave sine;
        private Color baseColor;

        public SiamoZeroEnergyBlade(Vector2 position, Vector2 velocity, Color color, float lifetime = 0.8f) : base(position)
        {
            this.velocity = velocity;
            this.rotation = Calc.Angle(velocity);
            this.lifeTime = lifetime;
            this.maxLifetime = lifetime;
            this.baseColor = color;

            Collider = new Hitbox(LENGTH, 10f, -LENGTH / 2f, -5f);
            Add(new PlayerCollider(OnPlayerCollide));
            Add(light = new VertexLight(color, 0.5f, 16, 32));
            Add(sine = new SineWave(0.4f));

            Depth = -100;
        }

        public override void Update()
        {
            base.Update();

            Position += velocity * Engine.DeltaTime;

            lifeTime -= Engine.DeltaTime;
            if (lifeTime <= 0f)
            {
                RemoveSelf();
                return;
            }

            // Decelerate over time
            velocity *= 1f - Engine.DeltaTime * 1.5f;

            light.Alpha = 0.5f + sine.Value * 0.3f;
        }

        public override void Render()
        {
            base.Render();

            float alpha = (0.9f + sine.Value * 0.1f) * (lifeTime / maxLifetime);
            Color renderColor = baseColor * alpha;

            Vector2 dir = Calc.AngleToVector(rotation, LENGTH);
            Vector2 start = Position - dir * 0.5f;
            Vector2 end = Position + dir * 0.5f;
            Vector2 perp = new Vector2(-dir.Y, dir.X).SafeNormalize();

            // Blade shape: tapered lines
            for (int i = -2; i <= 2; i++)
            {
                float widthFactor = 1f - Math.Abs(i) * 0.25f;
                Vector2 off = perp * (i * 2f);
                Draw.Line(start + off * widthFactor, end + off * widthFactor, renderColor);
            }

            // White core
            Draw.Line(start, end, Color.White * alpha * 0.85f);
        }

        private void OnPlayerCollide(global::Celeste.Player player)
        {
            player.Die(velocity.SafeNormalize());
        }
    }

    /// <summary>
    /// Spine pillar projectile for Siamo Zero's Rising Spine and Emerge attacks.
    /// Stays in place, deals contact damage, then fades.
    /// </summary>
    public class SiamoZeroSpinePillar : Entity
    {
        private float lifeTime;
        private const float MAX_LIFETIME = 2f;
        private const float PILLAR_HEIGHT = 80f;
        private const float PILLAR_WIDTH = 10f;
        private VertexLight light;
        private Color baseColor;
        private float extendProgress = 0f;

        public SiamoZeroSpinePillar(Vector2 position, Color color) : base(position)
        {
            this.lifeTime = MAX_LIFETIME;
            this.baseColor = color;

            Collider = new Hitbox(PILLAR_WIDTH, PILLAR_HEIGHT, -PILLAR_WIDTH / 2f, -PILLAR_HEIGHT);
            Add(new PlayerCollider(OnPlayerCollide));
            Add(light = new VertexLight(color, 0.6f, 32, 64));

            Depth = -90;
        }

        public override void Update()
        {
            base.Update();

            lifeTime -= Engine.DeltaTime;
            if (lifeTime <= 0f)
            {
                RemoveSelf();
                return;
            }

            // Extend upward rapidly, then hold
            if (extendProgress < 1f)
            {
                extendProgress = Math.Min(1f, extendProgress + Engine.DeltaTime * 6f);
            }

            light.Alpha = lifeTime / MAX_LIFETIME;
        }

        public override void Render()
        {
            base.Render();

            float alpha = lifeTime / MAX_LIFETIME;
            float height = PILLAR_HEIGHT * Ease.BackOut(extendProgress);

            Color renderColor = baseColor * alpha;

            // Spine pillar: vertical taper
            for (int i = 0; i < (int)height; i += 3)
            {
                float t = i / height;
                float width = PILLAR_WIDTH * (1f - t * 0.7f); // Taper toward top
                float x = Position.X - width / 2f;
                float y = Position.Y - i;

                Draw.Line(new Vector2(x, y), new Vector2(x + width, y), renderColor);
            }

            // Bright tip
            Vector2 tip = new Vector2(Position.X, Position.Y - height);
            Draw.Line(tip - Vector2.UnitX, tip + Vector2.UnitX, Color.White * alpha);
        }

        private void OnPlayerCollide(global::Celeste.Player player)
        {
            player.Die((player.Center - Position).SafeNormalize());
        }
    }
}

