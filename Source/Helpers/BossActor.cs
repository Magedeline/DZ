using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Helpers
{
    /// <summary>
    /// Base class for boss entities that behave like Celeste's FinalBoss.
    /// Extends Entity with sprite management, collision, and basic boss infrastructure.
    /// </summary>
    [Tracked(true)]
    public class BossActor : Entity
    {
        public Sprite Sprite { get; protected set; }
        protected string spriteName;
        protected Vector2 spriteScale;
        protected bool Grounded => Scene != null && Collider != null && Scene.CollideCheck<Solid>(Position + Vector2.UnitY);

        public Vector2 Speed;
        protected float maxFall;
        protected float gravityMult;
        protected bool solidCollidable;

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool IsDefeated { get; set; }

        public BossActor(
            Vector2 position,
            string spriteName,
            Vector2 spriteScale,
            float maxFall,
            bool collidable,
            bool solidCollidable,
            float gravityMult,
            Collider collider)
            : base(position)
        {
            this.spriteName = spriteName;
            this.spriteScale = spriteScale;
            this.maxFall = maxFall;
            this.Collidable = collidable;
            this.solidCollidable = solidCollidable;
            this.gravityMult = gravityMult;

            if (collider != null)
                this.Collider = collider;

            Depth = -8500;

            try
            {
                if (GFX.SpriteBank != null && GFX.SpriteBank.Has(spriteName))
                {
                    Sprite = GFX.SpriteBank.Create(spriteName);
                    Sprite.Scale = spriteScale;
                    Add(Sprite);
                }
            }
            catch
            {
                // Sprite bank entry not found at load time; subclasses can handle this
            }
        }

        public override void Update()
        {
            base.Update();

            if (gravityMult > 0f)
            {
                Speed.Y = Math.Min(Speed.Y + 900f * gravityMult * Engine.DeltaTime, maxFall);
            }

            Position += Speed * Engine.DeltaTime;
        }

        public virtual void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
            if (Health <= 0) IsDefeated = true;
        }

        public virtual void StartBossFight()
        {
        }

        public Level GetLevel()
        {
            return SceneAs<Level>();
        }
    }
}
