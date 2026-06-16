using Celeste;
using Celeste.Entities;
using MonoMod;
using System;

namespace Celeste.Entities
{
    /// <summary>
    /// Attaches a floating Ralsei dummy to the player as a companion.
    /// The dummy hovers around the player and follows their movement.
    /// </summary>
    [CustomEntity(ids: "DZ/RalseiDummyAttacher")]
    [HotReloadable]
    public class RalseiDummyAttacher : Component
    {
        private RalseiDummy ralseiDummy;
        private Player player;
        private float hoverOffsetX = 20f;
        private float hoverOffsetY = -10f;
        private float hoverSpeed = 2f;
        private float hoverAmplitude = 4f;
        private float hoverTime;

        public RalseiDummyAttacher() : base(true, true)
        {
        }

        public RalseiDummyAttacher(EntityData data, Vector2 offset) : base(true, true)
        {
            hoverOffsetX = data.Float("hoverOffsetX", 20f);
            hoverOffsetY = data.Float("hoverOffsetY", -10f);
            hoverSpeed = data.Float("hoverSpeed", 2f);
            hoverAmplitude = data.Float("hoverAmplitude", 4f);
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            player = entity as Player;
            
            if (player != null && player.Scene is Level level)
            {
                // Create the Ralsei dummy
                ralseiDummy = new RalseiDummy(player.Position);
                level.Add(ralseiDummy);
            }
        }

        public override void Update()
        {
            base.Update();

            if (ralseiDummy == null || player == null || player.Scene == null)
                return;

            // Update hover animation
            hoverTime += Engine.DeltaTime * hoverSpeed;
            
            // Calculate target position (hovering around player)
            Vector2 targetPosition = player.Position + new Vector2(
                hoverOffsetX * (player.Facing == Facings.Right ? 1 : -1),
                hoverOffsetY + (float)Math.Sin(hoverTime) * hoverAmplitude
            );

            // Smoothly move dummy to target position
            ralseiDummy.Position = Calc.Approach(ralseiDummy.Position, targetPosition, 120f * Engine.DeltaTime);
            
            // Make dummy face the same direction as player
            if (ralseiDummy.Sprite != null)
            {
                ralseiDummy.Sprite.Scale.X = player.Facing == Facings.Right ? -1f : 1f;
            }
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            
            // Remove the dummy when attacher is removed
            if (ralseiDummy != null && ralseiDummy.Scene != null)
            {
                ralseiDummy.RemoveSelf();
            }
        }

        public void SetHoverOffset(float offsetX, float offsetY)
        {
            hoverOffsetX = offsetX;
            hoverOffsetY = offsetY;
        }

        public void SetHoverSpeed(float speed)
        {
            hoverSpeed = speed;
        }

        public void SetHoverAmplitude(float amplitude)
        {
            hoverAmplitude = amplitude;
        }
    }
}