namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// Replayback - MyWorld's chase-your-own-past hazard. It records the player's
    /// position every frame and, after <c>startDelay</c>, spawns a ghost that walks that
    /// exact recording <c>startDelay</c> seconds behind. Touching the ghost kills you
    /// when <c>deadly</c> is set, so backtracking into your own path is fatal.
    ///
    /// Shipped alongside <see cref="ReplaybackPowerUp"/>, which reuses the same recording
    /// idea as an ability: rewind yourself to where you were a moment ago.
    ///
    /// Placeholder art: translucent square ghost, small marker square for the recorder.
    /// </summary>
    [CustomEntity("DZ/Replayback")]
    [Tracked]
    public class Replayback : Entity
    {
        #region Fields
        private static readonly Color GhostEdge = Calc.HexToColor("f0d9ff");

        private readonly float recordDuration;
        private readonly float startDelay;
        private readonly bool deadly;
        private readonly bool loop;
        private readonly float ghostAlpha;
        private readonly float ghostSize;
        private readonly string flag;
        private readonly bool invertFlag;

        private readonly Queue<Sample> samples = new Queue<Sample>();
        private float elapsed;
        private ReplaybackGhost ghost;
        private Level level;
        #endregion

        #region Constructor
        public Replayback(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            recordDuration = Math.Max(0.5f, data.Float("recordDuration", 4f));
            startDelay = Math.Max(0.1f, data.Float("startDelay", 2f));
            deadly = data.Bool("deadly", true);
            loop = data.Bool("loop", true);
            ghostAlpha = MathHelper.Clamp(data.Float("ghostAlpha", 0.65f), 0.05f, 1f);
            ghostSize = Math.Max(4f, data.Float("ghostSize", 8f));
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            Depth = -10000;
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

        private void Record(Player player)
        {
            samples.Enqueue(new Sample(elapsed, player.Position, player.Facing));

            // Keep only what the ghost still needs to play back.
            float keepFrom = elapsed - (startDelay + recordDuration);
            while (samples.Count > 0 && samples.Peek().Time < keepFrom)
            {
                samples.Dequeue();
            }
        }

        /// <summary>Position the ghost should be at right now, or null while still buffering.</summary>
        private Sample? SampleAt(float time)
        {
            Sample? best = null;
            foreach (Sample sample in samples)
            {
                if (sample.Time <= time)
                {
                    best = sample;
                }
                else
                {
                    break;
                }
            }

            return best;
        }

        private void Reset()
        {
            samples.Clear();
            elapsed = 0f;

            if (ghost != null)
            {
                ghost.RemoveSelf();
                ghost = null;
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

            if (!FlagAllows())
            {
                Reset();
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
            {
                Reset();
                return;
            }

            elapsed += Engine.DeltaTime;
            Record(player);

            float ghostTime = elapsed - startDelay;
            if (ghostTime <= 0f)
            {
                return;
            }

            Sample? sample = SampleAt(ghostTime);
            if (sample == null)
            {
                return;
            }

            if (ghost == null)
            {
                ghost = new ReplaybackGhost(sample.Value.Position, ghostSize, ghostAlpha, deadly);
                Scene.Add(ghost);
                Audio.Play("event:/char/badeline/maddy_split", sample.Value.Position);
            }

            ghost.MoveTo(sample.Value.Position, sample.Value.Facing);

            if (!loop && ghostTime > recordDuration)
            {
                ghost.RemoveSelf();
                ghost = null;
                RemoveSelf();
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(X - 6f, Y - 6f, 12f, 12f, GhostEdge * 0.7f);
        }
        #endregion

        /// <summary>One recorded frame of player state.</summary>
        private readonly struct Sample
        {
            public readonly float Time;
            public readonly Vector2 Position;
            public readonly Facings Facing;

            public Sample(float time, Vector2 position, Facings facing)
            {
                Time = time;
                Position = position;
                Facing = facing;
            }
        }
    }

    /// <summary>The replayed silhouette spawned by <see cref="Replayback"/>.</summary>
    [Tracked]
    public class ReplaybackGhost : Entity
    {
        #region Fields
        private static readonly Color GhostFill = Calc.HexToColor("b06fd6");
        private static readonly Color GhostEdge = Calc.HexToColor("f0d9ff");

        private readonly float size;
        private readonly float alpha;
        private readonly bool deadly;

        private Facings facing = Facings.Right;
        private float shimmer;
        #endregion

        #region Constructor
        public ReplaybackGhost(Vector2 position, float size, float alpha, bool deadly)
            : base(position)
        {
            this.size = size;
            this.alpha = alpha;
            this.deadly = deadly;

            Collider = new Hitbox(size, size * 1.4f, -size / 2f, -size * 1.4f);
            Depth = -8000;

            Add(new VertexLight(Color.MediumPurple, alpha * 0.5f, 12, 32));
        }
        #endregion

        #region Public API
        public void MoveTo(Vector2 position, Facings facing)
        {
            Position = position;
            this.facing = facing;
        }
        #endregion

        #region Entity Overrides
        public override void Update()
        {
            base.Update();

            shimmer += Engine.DeltaTime * 6f;

            if (!deadly)
            {
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && !player.Dead && CollideCheck(player))
            {
                player.Die((player.Center - Center).SafeNormalize());
            }
        }

        public override void Render()
        {
            base.Render();

            float wobble = 1f + (float)Math.Sin(shimmer) * 0.05f;
            Rectangle bounds = Collider.Bounds;

            Placeholder.Square(
                bounds.X,
                bounds.Y,
                bounds.Width * wobble,
                bounds.Height,
                GhostFill * alpha,
                GhostEdge * alpha);

            // Facing pip, so the ghost's direction is readable.
            float pipX = facing == Facings.Right ? bounds.Right - 3f : bounds.Left + 1f;
            Draw.Rect(pipX, bounds.Y + 2f, 2f, 2f, GhostEdge * alpha);
        }
        #endregion
    }

    /// <summary>
    /// ReplaybackPowerUp - the rewind ability. Once collected it keeps its own rolling
    /// recording of the player; pressing the configured button snaps the player back to
    /// where they were <c>rewindTime</c> seconds ago, restoring that moment's speed.
    /// </summary>
    [CustomEntity("DZ/ReplaybackPowerUp")]
    [Tracked]
    public class ReplaybackPowerUp : Entity
    {
        #region Fields
        private static readonly Color PickupFill = Calc.HexToColor("4f7fd6");
        private static readonly Color PickupEdge = Calc.HexToColor("d9e9ff");

        private readonly float rewindTime;
        private readonly int charges;
        private readonly float respawnDelay;
        private readonly bool oneUse;
        private readonly float radius;
        private readonly AbilityButton button;

        private bool collected;
        private float respawnTimer;
        private float bob;
        private Level level;
        #endregion

        #region Constructor
        public ReplaybackPowerUp(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            rewindTime = Math.Max(0.2f, data.Float("rewindTime", 1.5f));
            charges = Math.Max(1, data.Int("charges", 1));
            respawnDelay = Math.Max(0f, data.Float("respawnDelay", 4f));
            oneUse = data.Bool("oneUse", false);
            radius = Math.Max(4f, data.Float("radius", 8f));
            button = data.Enum("useButton", AbilityButton.Grab);

            Collider = new Circle(radius);
            Depth = -100;

            Add(new VertexLight(Color.CornflowerBlue, 0.6f, 16, 40));
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

            bob += Engine.DeltaTime * 3f;

            if (collected)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    collected = false;
                }

                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || !CollideCheck(player))
            {
                return;
            }

            collected = true;
            respawnTimer = respawnDelay;

            Audio.Play("event:/game/general/crystalheart_blue_get", Position);
            RewindController.Ensure(Scene, rewindTime, button)?.AddCharges(charges);

            if (oneUse || respawnDelay <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();

            if (collected)
            {
                Draw.Circle(Position, radius, PickupEdge * 0.2f, 10);
                return;
            }

            Vector2 center = Position + new Vector2(0f, (float)Math.Sin(bob) * 2f);

            Draw.Circle(center, radius + 3f, PickupFill * 0.35f, 12);
            Placeholder.Circle(center, radius, PickupFill, PickupEdge);

            // Rewind arrow: a backwards chevron.
            Draw.Rect(center.X - 3f, center.Y - 0.5f, 6f, 1f, PickupEdge);
            Draw.Rect(center.X - 3f, center.Y - 2.5f, 1f, 5f, PickupEdge);
        }
        #endregion
    }

    /// <summary>Rolling player recorder that performs the rewind for <see cref="ReplaybackPowerUp"/>.</summary>
    [Tracked]
    public class RewindController : Entity
    {
        #region Fields
        private readonly Queue<Frame> frames = new Queue<Frame>();
        private float rewindTime;
        private AbilityButton button;
        private int charges;
        private float elapsed;
        private float cooldown;
        #endregion

        #region Constructor
        public RewindController(float rewindTime, AbilityButton button)
        {
            this.rewindTime = rewindTime;
            this.button = button;

            Tag = Tags.Global;
            Depth = -10000;
        }
        #endregion

        #region Public API
        public static RewindController Ensure(Scene scene, float rewindTime, AbilityButton button)
        {
            if (scene == null)
            {
                return null;
            }

            RewindController controller = scene.Tracker.GetEntity<RewindController>();
            if (controller != null)
            {
                controller.rewindTime = rewindTime;
                controller.button = button;
                return controller;
            }

            controller = new RewindController(rewindTime, button);
            scene.Add(controller);
            return controller;
        }

        public int Charges => charges;

        public void AddCharges(int count)
        {
            charges += Math.Max(0, count);
        }

        public void Rewind(Player player)
        {
            if (player == null || charges <= 0 || frames.Count == 0)
            {
                return;
            }

            float target = elapsed - rewindTime;
            Frame? chosen = null;

            foreach (Frame frame in frames)
            {
                if (frame.Time <= target)
                {
                    chosen = frame;
                }
                else
                {
                    break;
                }
            }

            // Not enough history yet - fall back to the oldest frame we have.
            chosen ??= frames.Peek();

            charges--;
            cooldown = 0.35f;

            player.Position = chosen.Value.Position;
            player.Speed = chosen.Value.Speed;

            Audio.Play("event:/char/badeline/disappear", player.Position);
            (Scene as Level)?.Flash(Color.CornflowerBlue * 0.25f, drawPlayerOver: true);

            frames.Clear();
            elapsed = 0f;
        }
        #endregion

        #region Private
        private bool ButtonPressed(Player player)
        {
            switch (button)
            {
                case AbilityButton.Dash:
                    return Input.Dash.Pressed;
                case AbilityButton.Jump:
                    return Input.Jump.Pressed;
                case AbilityButton.Talk:
                    return Input.Talk.Pressed;
                default:
                    return Input.Grab.Pressed && player.StateMachine.State != Player.StClimb;
            }
        }
        #endregion

        #region Entity Overrides
        public override void Update()
        {
            base.Update();

            if (cooldown > 0f)
            {
                cooldown -= Engine.DeltaTime;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
            {
                frames.Clear();
                elapsed = 0f;
                return;
            }

            elapsed += Engine.DeltaTime;
            frames.Enqueue(new Frame(elapsed, player.Position, player.Speed));

            // Keep a little more than one rewind's worth of history.
            while (frames.Count > 0 && frames.Peek().Time < elapsed - (rewindTime + 0.5f))
            {
                frames.Dequeue();
            }

            if (cooldown <= 0f && charges > 0 && player.StateMachine.State == Player.StNormal && ButtonPressed(player))
            {
                Rewind(player);
            }
        }
        #endregion

        private readonly struct Frame
        {
            public readonly float Time;
            public readonly Vector2 Position;
            public readonly Vector2 Speed;

            public Frame(float time, Vector2 position, Vector2 speed)
            {
                Time = time;
                Position = position;
                Speed = speed;
            }
        }
    }
}
