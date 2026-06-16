using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ
{
    // Plan:
    // 1. Define a tracked projectile entity in the DZ namespace so Scene.Tracker can find it.
    // 2. Expose a public Velocity property because GravityWell modifies projectile motion through it.
    // 3. Read basic projectile settings from entity data: speed, angle, velocity components, lifetime, radius, and color.
    // 4. Move the projectile every frame using Velocity and Engine.DeltaTime.
    // 5. Expire and remove the projectile when its lifetime runs out.
    // 6. Optionally remove the projectile if it collides with a Solid.
    // 7. Render it as a simple circle so it is visible in-game.

    [CustomEntity("DZ/Projectile")]
    [Tracked]
    public class Projectile : Entity
    {
        public Vector2 Velocity { get; set; }

        private float lifetime;
        private readonly float radius;
        private readonly Color color;
        private readonly bool destroyOnSolidCollision;

        public Projectile(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            float speed = data.Float("speed", 120f);
            float angleDegrees = data.Float("angle", 0f);
            float angleRadians = MathHelper.ToRadians(angleDegrees);

            float velocityX = data.Float("velocityX", float.NaN);
            float velocityY = data.Float("velocityY", float.NaN);

            Velocity = !float.IsNaN(velocityX) && !float.IsNaN(velocityY)
                ? new Vector2(velocityX, velocityY)
                : Calc.AngleToVector(angleRadians, speed);

            lifetime = data.Float("lifetime", 3f);
            radius = data.Float("radius", 4f);
            destroyOnSolidCollision = data.Bool("destroyOnSolidCollision", true);
            color = Calc.HexToColor(data.Attr("color", "ffffff"));

            Collider = new Hitbox(radius * 2f, radius * 2f, -radius, -radius);
            Depth = -100;
        }

        public override void Update()
        {
            base.Update();

            Position += Velocity * Engine.DeltaTime;

            if (destroyOnSolidCollision && CollideCheck<Solid>())
            {
                RemoveSelf();
                return;
            }

            lifetime -= Engine.DeltaTime;
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            Draw.Circle(Position, radius, color * 0.8f, 12);
        }
    }
}
