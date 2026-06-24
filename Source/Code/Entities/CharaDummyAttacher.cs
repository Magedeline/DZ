using Celeste;
using Celeste.Entities;
using MonoMod;
using System;

namespace Celeste.Entities
{
    /// <summary>
    /// Attaches a floating Chara dummy to the player as a companion.
    /// The dummy hovers around the player and follows their movement.
    /// Intended for later DLC use.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/CharaDummyAttacher")]
    [HotReloadable]
    public class CharaDummyAttacher : Component
    {
        private CharaDummy charaDummy;
        private Player player;
        private float hoverOffsetX = 25f;
        private float hoverOffsetY = -15f;
        private float hoverSpeed = 2.5f;
        private float hoverAmplitude = 5f;
        private float hoverTime;

        public CharaDummyAttacher() : base(true, true)
        {
        }

        public CharaDummyAttacher(EntityData data, Vector2 offset) : base(true, true)
        {
            hoverOffsetX = data.Float("hoverOffsetX", 25f);
            hoverOffsetY = data.Float("hoverOffsetY", -15f);
            hoverSpeed = data.Float("hoverSpeed", 2.5f);
            hoverAmplitude = data.Float("hoverAmplitude", 5f);
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            player = entity as Player;
            
            if (player != null && player.Scene is Level level)
            {
                // Create the Chara dummy
                charaDummy = new CharaDummy(player.Position);
                level.Add(charaDummy);
            }
        }

        public override void Update()
        {
            base.Update();

            if (charaDummy == null || player == null || player.Scene == null)
                return;

            // Update hover animation
            hoverTime += Engine.DeltaTime * hoverSpeed;
            
            // Calculate target position (hovering around player)
            Vector2 targetPosition = player.Position + new Vector2(
                hoverOffsetX * (player.Facing == Facings.Right ? 1 : -1),
                hoverOffsetY + (float)Math.Sin(hoverTime) * hoverAmplitude
            );

            // Smoothly move dummy to target position
            charaDummy.Position = Calc.Approach(charaDummy.Position, targetPosition, 120f * Engine.DeltaTime);
            
            // Make dummy face the same direction as player
            if (charaDummy.Sprite != null)
            {
                charaDummy.Sprite.Scale.X = player.Facing == Facings.Right ? -1f : 1f;
            }
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            
            // Remove the dummy when attacher is removed
            if (charaDummy != null && charaDummy.Scene != null)
            {
                charaDummy.RemoveSelf();
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