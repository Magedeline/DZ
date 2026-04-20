namespace Celeste.Entities
{
    // Simple particle class for hit effects
    public class HitParticle : Entity
    {
        private Vector2 velocity;
        private Color color;
        private float life = 1.0f;

        public HitParticle(Vector2 position, Vector2 velocity, Color color) : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            Depth = -10000;
        }

        public override void Update()
        {
            base.Update();

            Position += velocity * Engine.DeltaTime;
            velocity.Y += 200f * Engine.DeltaTime; // Gravity
            velocity *= 0.98f; // Air resistance

            life -= Engine.DeltaTime;
            if (life <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = life;
            Draw.Point(Position, color * alpha);
        }
    }

    public class DefeatParticle : HitParticle
    {
        public DefeatParticle(Vector2 position, Vector2 velocity)
            : base(position, velocity, Color.Orange)
        {
        }
    }
}




