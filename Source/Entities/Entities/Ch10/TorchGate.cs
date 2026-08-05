using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// TorchGate - A movable solid block that activates when a TouchTorchSwitch group is completed.
    /// Links to a torch group via the <c>group</c> parameter. When the group's flag is set,
    /// the gate moves to its target position.
    /// </summary>
    [CustomEntity("DZ/TorchGate")]
    public class TorchGate : Solid
    {
        #region Fields
        private static readonly Color GateColor = Calc.HexToColor("6b5c46");
        private static readonly Color ActiveColor = Calc.HexToColor("ffb14e");

        private readonly string group;
        private readonly string customFlag;
        private readonly Vector2 startPosition;
        private readonly Vector2 targetOffset;
        private readonly float moveSpeed;
        private readonly bool permanent;

        private bool activated;
        private bool moving;
        private Vector2 targetPosition;
        private Color currentColor;
        private float colorLerp;
        #endregion

        #region Constructor
        public TorchGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            group = data.Attr("group", "");
            customFlag = data.Attr("flag", "");
            targetOffset = new Vector2(data.Int("moveX", 0), data.Int("moveY", -64));
            moveSpeed = data.Float("moveSpeed", 60f);
            permanent = data.Bool("permanent", true);

            startPosition = Position;
            targetPosition = startPosition + targetOffset;
            currentColor = GateColor;
            colorLerp = 0f;

            Depth = -9000;
        }
        #endregion

        #region Private Methods
        private string GetGroupFlag()
        {
            if (!string.IsNullOrEmpty(customFlag))
            {
                return customFlag;
            }

            return string.IsNullOrEmpty(group) ? "torch_switch" : "torch_switch_" + group;
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);

            Level level = scene as Level;
            if (level != null && level.Session.GetFlag(GetGroupFlag()))
            {
                if (permanent)
                {
                    // If permanent and flag is already set, spawn at target
                    Position = targetPosition;
                    activated = true;
                    colorLerp = 1f;
                    currentColor = ActiveColor;
                }
                else
                {
                    // If not permanent, start moving
                    activated = true;
                    moving = true;
                }
            }
        }

        public override void Update()
        {
            base.Update();

            Level level = Scene as Level;
            if (level != null && !activated && level.Session.GetFlag(GetGroupFlag()))
            {
                activated = true;
                moving = true;
                Audio.Play(ChapterSfx.RuinsGateOpen, Position);
            }

            if (moving)
            {
                colorLerp = Calc.Approach(colorLerp, 1f, Engine.DeltaTime * 2f);
                currentColor = Color.Lerp(GateColor, ActiveColor, colorLerp);

                Vector2 direction = (targetPosition - Position);
                float distance = direction.Length();

                if (distance <= moveSpeed * Engine.DeltaTime)
                {
                    // Reached target
                    MoveTo(targetPosition);
                    moving = false;
                    Audio.Play(ChapterSfx.RuinsGateFinish, Position);
                }
                else
                {
                    // Move toward target
                    direction.Normalize();
                    MoveH(direction.X * moveSpeed * Engine.DeltaTime);
                    MoveV(direction.Y * moveSpeed * Engine.DeltaTime);
                }
            }
        }

        public override void Render()
        {
            Draw.Rect(X, Y, Width, Height, currentColor);

            // Draw border
            Draw.HollowRect(X, Y, Width, Height, currentColor * 1.5f);

            // Draw decorative lines if active
            if (colorLerp > 0.1f)
            {
                float alpha = colorLerp * 0.6f;
                Draw.Line(X + 2, Y + Height / 2, X + Width - 2, Y + Height / 2, ActiveColor * alpha);
                Draw.Line(X + Width / 2, Y + 2, X + Width / 2, Y + Height - 2, ActiveColor * alpha);
            }

            base.Render();
        }
        #endregion
    }
}
