using Celeste;
using Celeste.Entities;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that attaches a floating Badeline dummy to the player.
    /// Place this trigger in a level to automatically attach the Badeline dummy when the player enters.
    /// </summary>
    [CustomEntity("DZ/BadelineDummyAttacherTrigger")]
    [Tracked]
    public class BadelineDummyAttacherTrigger : Trigger
    {
        private float hoverOffsetX = -20f;
        private float hoverOffsetY = -10f;
        private float hoverSpeed = 2f;
        private float hoverAmplitude = 4f;
        private bool once = true;
        private bool removeOnExit = false;
        private bool triggered = false;
        private bool pendingRemove;

        public BadelineDummyAttacherTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            hoverOffsetX = data.Float("hoverOffsetX", -20f);
            hoverOffsetY = data.Float("hoverOffsetY", -10f);
            hoverSpeed = data.Float("hoverSpeed", 2f);
            hoverAmplitude = data.Float("hoverAmplitude", 4f);
            once = data.Bool("once", true);
            removeOnExit = data.Bool("removeOnExit", false);
        }

        public override void OnEnter(Player player)
        {
            if (once && triggered)
                return;

            triggered = true;

            // Check if player already has the attacher
            var existingAttacher = player.Get<BadelineDummyAttacher>();
            if (existingAttacher == null)
            {
                // Create and add the attacher
                var attacher = new BadelineDummyAttacher();
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
                pendingRemove = true;
            }
        }

        public override void Update()
        {
            base.Update();
            if (pendingRemove) RemoveSelf();
        }

        public override void OnLeave(Player player)
        {
            if (removeOnExit)
            {
                player.RemoveBadelineDummy();
            }
        }
    }
}