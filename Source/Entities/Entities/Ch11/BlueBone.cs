namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// BlueBone - Undertale blue attack. The bone sweeps between its spawn point and
    /// its node; it only kills the player if the player is <em>moving</em> when it
    /// passes through them. Stand still and it is harmless, so the room becomes a
    /// stop-and-go rhythm puzzle.
    ///
    /// "Moving" means speed above <c>moveThreshold</c>, or dashing at any speed - you
    /// cannot cheese it by dashing into a standstill.
    ///
    /// Placeholder art: blue capsule (square shaft, circle knobs at each end).
    /// </summary>
    [CustomEntity("DZ/BlueBone")]
    [Tracked]
    public class BlueBone : Entity
    {
        #region Fields
        private static readonly Color BoneFill = Calc.HexToColor("3a7fd5");
        private static readonly Color BoneOutline = Calc.HexToColor("bfe2ff");
        private static readonly Color DangerOutline = Calc.HexToColor("ff5a5a");

        private readonly float boneLength;
        private readonly float boneThickness;
        private readonly bool horizontal;
        private readonly float speed;
        private readonly float moveThreshold;
        private readonly float pauseTime;
        private readonly bool loop;
        private readonly string flag;
        private readonly bool invertFlag;

        private readonly Vector2 startPosition;
        private readonly Vector2 endPosition;
        private bool towardEnd = true;
        private float pauseTimer;
        private float pulse;
        private Level level;
        #endregion

        #region Constructor
        public BlueBone(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            boneLength = Math.Max(8f, data.Float("boneLength", 48f));
            boneThickness = Math.Max(4f, data.Float("boneThickness", 8f));
            horizontal = data.Bool("horizontal", false);
            speed = Math.Max(0f, data.Float("speed", 90f));
            moveThreshold = Math.Max(0f, data.Float("moveThreshold", 12f));
            pauseTime = Math.Max(0f, data.Float("pauseTime", 0f));
            loop = data.Bool("loop", true);
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            startPosition = Position;
            endPosition = data.Nodes != null && data.Nodes.Length > 0
                ? data.Nodes[0] + offset
                : Position;

            Collider = horizontal
                ? new Hitbox(boneLength, boneThickness, -boneLength / 2f, -boneThickness / 2f)
                : new Hitbox(boneThickness, boneLength, -boneThickness / 2f, -boneLength / 2f);

            Depth = -8500;
        }
        #endregion

        #region Public API
        /// <summary>True when the player is moving fast enough for this bone to be lethal.</summary>
        public bool IsLethalTo(Player player)
        {
            if (player == null || player.Dead)
            {
                return false;
            }

            return player.DashAttacking || player.Speed.Length() > moveThreshold;
        }
        #endregion

        #region Private
        /// <summary>Whether this entity's enable flag currently permits it to run.</summary>
        private bool FlagAllows()
        {
            if (string.IsNullOrEmpty(flag) || level == null)
            {
                return true;
            }

            return level.Session.GetFlag(flag) != invertFlag;
        }

        private void Travel()
        {
            if (speed <= 0f || startPosition == endPosition)
            {
                return;
            }

            if (pauseTimer > 0f)
            {
                pauseTimer -= Engine.DeltaTime;
                return;
            }

            Vector2 target = towardEnd ? endPosition : startPosition;
            Vector2 toTarget = target - Position;
            float step = speed * Engine.DeltaTime;

            if (toTarget.Length() <= step)
            {
                Position = target;

                if (!loop && towardEnd)
                {
                    Audio.Play(ChapterSfx.SnowBoneBreak, Center);
                    RemoveSelf();
                    return;
                }

                towardEnd = !towardEnd;
                pauseTimer = pauseTime;
                return;
            }

            Position += toTarget.SafeNormalize() * step;
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            if (!FlagAllows())
            {
                pulse = 0f;
                return;
            }

            Travel();

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                if (IsLethalTo(player))
                {
                    Audio.Play(ChapterSfx.SnowHit, Center);
                    player.Die((player.Center - Center).SafeNormalize());
                }
                else
                {
                    // Passing safely - flash so the player learns the rule.
                    pulse = 1f;
                }
            }
            else if (player != null && Vector2.Distance(player.Center, Center) < boneLength && IsLethalTo(player))
            {
                pulse = Math.Max(pulse, 0.6f);
            }

            pulse = Calc.Approach(pulse, 0f, Engine.DeltaTime * 3f);
        }

        public override void Render()
        {
            base.Render();

            if (!FlagAllows())
            {
                return;
            }

            Rectangle bounds = Collider.Bounds;
            Color outline = Color.Lerp(BoneOutline, DangerOutline, pulse);

            Placeholder.Square(bounds.X, bounds.Y, bounds.Width, bounds.Height, BoneFill, outline);

            // Bone knobs at both ends.
            float knob = boneThickness * 0.75f;
            if (horizontal)
            {
                Placeholder.Circle(new Vector2(bounds.Left, Center.Y), knob, BoneFill, outline);
                Placeholder.Circle(new Vector2(bounds.Right, Center.Y), knob, BoneFill, outline);
            }
            else
            {
                Placeholder.Circle(new Vector2(Center.X, bounds.Top), knob, BoneFill, outline);
                Placeholder.Circle(new Vector2(Center.X, bounds.Bottom), knob, BoneFill, outline);
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(startPosition, endPosition, Color.Aqua * 0.5f);
        }
        #endregion
    }
}
