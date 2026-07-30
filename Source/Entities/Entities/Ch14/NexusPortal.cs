namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// NexusPortal - point A to point B, or point C. The entity itself is exit A; node 1
    /// is exit B and the optional node 2 is exit C. <c>exitMode</c> decides which one a
    /// given entry uses:
    ///   Primary   - always B.
    ///   Alternate - B, C, B, C...
    ///   Random    - picks between B and C.
    ///   Flag      - C while <c>flag</c> is set, otherwise B.
    ///
    /// Momentum carries through by default, so a portal can be dashed through as part of
    /// a movement line rather than acting as a full stop.
    ///
    /// Placeholder art: cyan ring with a filled core.
    /// </summary>
    [CustomEntity("DZ/NexusPortal")]
    [Tracked]
    public class NexusPortal : Entity
    {
        #region Enums
        public enum ExitMode
        {
            Primary,
            Alternate,
            Random,
            Flag
        }
        #endregion

        #region Fields
        private static readonly Color RingColor = Calc.HexToColor("2fe0c4");
        private static readonly Color CoreColor = Calc.HexToColor("0e3f4d");
        private static readonly Color SparkColor = Calc.HexToColor("bff7ee");

        private readonly ExitMode exitMode;
        private readonly string flag;
        private readonly float cooldown;
        private readonly bool preserveMomentum;
        private readonly float exitBoost;
        private readonly float radius;
        private readonly bool requiresDash;

        private readonly Vector2 exitB;
        private readonly Vector2 exitC;
        private readonly bool hasExitC;

        private bool useC;
        private float cooldownTimer;
        private float spin;
        private Level level;
        #endregion

        #region Constructor
        public NexusPortal(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            exitMode = data.Enum("exitMode", ExitMode.Primary);
            flag = data.Attr("flag", "");
            cooldown = Math.Max(0.05f, data.Float("cooldown", 0.6f));
            preserveMomentum = data.Bool("preserveMomentum", true);
            exitBoost = Math.Max(0f, data.Float("exitBoost", 1f));
            radius = Math.Max(4f, data.Float("radius", 12f));
            requiresDash = data.Bool("requiresDash", false);

            Vector2[] nodes = data.Nodes ?? new Vector2[0];
            exitB = nodes.Length > 0 ? nodes[0] + offset : Position;
            hasExitC = nodes.Length > 1;
            exitC = hasExitC ? nodes[1] + offset : exitB;

            Collider = new Circle(radius);
            Depth = -100;

            Add(new VertexLight(Color.Aquamarine, 0.6f, 20, 48));
        }
        #endregion

        #region Public API
        /// <summary>The exit this portal would use right now.</summary>
        public Vector2 CurrentExit()
        {
            switch (exitMode)
            {
                case ExitMode.Alternate:
                    return hasExitC && useC ? exitC : exitB;

                case ExitMode.Random:
                    return hasExitC && Calc.Random.Chance(0.5f) ? exitC : exitB;

                case ExitMode.Flag:
                    bool flagSet = !string.IsNullOrEmpty(flag) && level != null && level.Session.GetFlag(flag);
                    return hasExitC && flagSet ? exitC : exitB;

                default:
                    return exitB;
            }
        }

        public void Teleport(Player player)
        {
            if (player == null || player.Dead)
            {
                return;
            }

            Vector2 target = CurrentExit();
            Vector2 from = player.Position;
            Vector2 speed = player.Speed;

            player.Position = target;
            player.Speed = preserveMomentum ? speed * exitBoost : Vector2.Zero;

            // Keep the camera from snapping if the hop stays inside the room.
            if (level != null)
            {
                level.Camera.Position += (target - from) * 0.5f;
                level.Flash(RingColor * 0.15f, drawPlayerOver: true);
            }

            Audio.Play(ChapterSfx.NexusEnter, target);

            cooldownTimer = cooldown;
            useC = !useC;

            // Stop the destination portal from bouncing the player straight back.
            foreach (Entity entity in Scene.Tracker.GetEntities<NexusPortal>())
            {
                NexusPortal other = (NexusPortal)entity;
                if (other != this && Vector2.Distance(other.Position, target) <= other.radius + 12f)
                {
                    other.cooldownTimer = cooldown;
                }
            }
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

            spin += Engine.DeltaTime * 2.4f;

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Engine.DeltaTime;
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player) && (!requiresDash || player.DashAttacking))
            {
                Teleport(player);
            }
        }

        public override void Render()
        {
            base.Render();

            float ready = cooldownTimer > 0f ? 0.35f : 1f;
            float breathe = 1f + (float)Math.Sin(spin * 1.6f) * 0.08f;

            Placeholder.FillCircle(Position, radius * 0.75f * breathe, CoreColor * ready);
            Draw.Circle(Position, radius * breathe, RingColor * ready, 14);
            Draw.Circle(Position, radius * 0.55f * breathe, RingColor * (ready * 0.7f), 10);

            // Orbiting sparks, three for a portal with a C exit and two otherwise.
            int sparks = hasExitC ? 3 : 2;
            for (int i = 0; i < sparks; i++)
            {
                float angle = spin + i * (MathHelper.TwoPi / sparks);
                Placeholder.FillCircle(Position + Calc.AngleToVector(angle, radius * breathe), 1.5f, SparkColor * ready);
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            Draw.Line(Position, exitB, Color.Aquamarine * 0.6f);
            Draw.HollowRect(exitB.X - 4f, exitB.Y - 4f, 8f, 8f, Color.Aquamarine * 0.6f);

            if (hasExitC)
            {
                Draw.Line(Position, exitC, Color.Violet * 0.6f);
                Draw.HollowRect(exitC.X - 4f, exitC.Y - 4f, 8f, 8f, Color.Violet * 0.6f);
            }
        }
        #endregion
    }
}
