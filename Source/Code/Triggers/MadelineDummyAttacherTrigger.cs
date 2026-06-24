using Celeste;
using Celeste.Entities;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that attaches a floating Madeline dummy to the player.
    /// Intended for later DLC use.
    /// </summary>
    [CustomEntity("MaggyHelper/MadelineDummyAttacherTrigger")]
    [Tracked]
    public class MadelineDummyAttacherTrigger : Trigger
    {
        private float hoverOffsetX = -25f;
        private float hoverOffsetY = -15f;
        private float hoverSpeed = 2.5f;
        private float hoverAmplitude = 5f;
        private bool once = true;
        private bool removeOnExit = false;
        private bool triggered = false;

        public MadelineDummyAttacherTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            hoverOffsetX = data.Float("hoverOffsetX", -25f);
            hoverOffsetY = data.Float("hoverOffsetY", -15f);
            hoverSpeed = data.Float("hoverSpeed", 2.5f);
            hoverAmplitude = data.Float("hoverAmplitude", 5f);
            once = data.Bool("once", true);
            removeOnExit = data.Bool("removeOnExit", false);
        }

        public override void OnEnter(Player player)
        {
            if (once && triggered)
                return;

            triggered = true;

            // Check if player already has the attacher
            var existingAttacher = player.Get<MadelineDummyAttacher>();
            if (existingAttacher == null)
            {
                // Create and add the attacher
                var attacher = new MadelineDummyAttacher();
                attacher.SetHoverOffset(hoverOffsetX, hoverOffsetY);
                attacher.SetHoverSpeed(hoverSpeed);
                attacher.SetHoverAmplitude(hoverAmplitude);
                player.Add(attacher);
            }
            else
            {
                // Update existing attacher settings
                existingAttacher.SetHoverOffset(hoverOffsetX, hoverOffsetY);
                existingAttacher.SetHoverSpeed(hoverSpeed);
                existingAttacher.SetHoverAmplitude(hoverAmplitude);
            }

            if (once)
            {
                RemoveSelf();
            }
        }

        public override void OnLeave(Player player)
        {
            if (removeOnExit)
            {
                player.RemoveMadelineDummy();
            }
        }
    }
}