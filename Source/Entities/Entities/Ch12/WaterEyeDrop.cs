namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// WaterEyeDrop - Wateredgefall ceiling eye. The pupil tracks the player, the eye
    /// swells as a tell, then it releases a droplet that falls straight down and kills
    /// on contact. Set <c>onlyWhenPlayerBelow</c> so it only fires when the player
    /// walks into its column.
    ///
    /// Placeholder art: white circle sclera, dark circle pupil, small blue circle drop.
    /// </summary>
    [CustomEntity("DZ/WaterEyeDrop")]
    [Tracked]
    public class WaterEyeDrop : Entity
    {
        #region Enums
        private enum EyeState
        {
            Idle,
            Warning,
            Cooldown
        }
        #endregion

        #region Fields
        private static readonly Color Sclera = Calc.HexToColor("eaf6ff");
        private static readonly Color ScleraOutline = Calc.HexToColor("3c6b85");
        private static readonly Color Pupil = Calc.HexToColor("1b3d52");
        private static readonly Color DropColor = Calc.HexToColor("59b7e0");

        private readonly float dropInterval;
        private readonly float warningTime;
        private readonly float dropSpeed;
        private readonly float dropRadius;
        private readonly float maxFallDistance;
        private readonly float eyeRadius;
        private readonly float detectWidth;
        private readonly bool onlyWhenPlayerBelow;
        private readonly bool deadly;
        private readonly string flag;
        private readonly bool invertFlag;

        private EyeState state = EyeState.Idle;
        private float stateTimer;
        private Vector2 pupilOffset;
        private Level level;
        #endregion

        #region Constructor
        public WaterEyeDrop(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            dropInterval = Math.Max(0.1f, data.Float("dropInterval", 2.2f));
            warningTime = Math.Max(0f, data.Float("warningTime", 0.55f));
            dropSpeed = Math.Max(10f, data.Float("dropSpeed", 190f));
            dropRadius = Math.Max(1f, data.Float("dropRadius", 4f));
            maxFallDistance = Math.Max(16f, data.Float("maxFallDistance", 220f));
            eyeRadius = Math.Max(3f, data.Float("eyeRadius", 7f));
            detectWidth = Math.Max(4f, data.Float("detectWidth", 24f));
            onlyWhenPlayerBelow = data.Bool("onlyWhenPlayerBelow", true);
            deadly = data.Bool("deadly", true);
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            Collider = new Circle(eyeRadius);
            Depth = -100;

            Add(new VertexLight(Color.LightSkyBlue, 0.4f, 12, 32));
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

        private bool PlayerInColumn(Player player)
        {
            if (!onlyWhenPlayerBelow)
            {
                return true;
            }

            if (player == null)
            {
                return false;
            }

            return Math.Abs(player.Center.X - X) <= detectWidth / 2f
                && player.Center.Y > Y
                && player.Center.Y - Y <= maxFallDistance;
        }

        private void ReleaseDrop()
        {
            Audio.Play("event:/env/local/waterfall_small_main", Position);
            Scene.Add(new WaterDroplet(Position + new Vector2(0f, eyeRadius), dropSpeed, dropRadius, maxFallDistance, deadly));
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            stateTimer = dropInterval;
        }

        public override void Update()
        {
            base.Update();

            if (!FlagAllows())
            {
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();

            // Pupil follows the player even while idle.
            if (player != null)
            {
                Vector2 toPlayer = player.Center - Position;
                pupilOffset = Calc.Approach(pupilOffset, toPlayer.SafeNormalize() * (eyeRadius * 0.4f), 40f * Engine.DeltaTime);
            }
            else
            {
                pupilOffset = Calc.Approach(pupilOffset, Vector2.Zero, 30f * Engine.DeltaTime);
            }

            stateTimer -= Engine.DeltaTime;

            switch (state)
            {
                case EyeState.Idle:
                    if (stateTimer <= 0f && PlayerInColumn(player))
                    {
                        state = EyeState.Warning;
                        stateTimer = warningTime;
                    }
                    break;

                case EyeState.Warning:
                    if (stateTimer <= 0f)
                    {
                        ReleaseDrop();
                        state = EyeState.Cooldown;
                        stateTimer = dropInterval;
                    }
                    break;

                case EyeState.Cooldown:
                    if (stateTimer <= 0f)
                    {
                        state = EyeState.Idle;
                        stateTimer = 0f;
                    }
                    break;
            }
        }

        public override void Render()
        {
            base.Render();

            float swell = state == EyeState.Warning && warningTime > 0f
                ? 1f - MathHelper.Clamp(stateTimer / warningTime, 0f, 1f)
                : 0f;

            Placeholder.Circle(Position, eyeRadius + swell * 1.5f, Sclera, ScleraOutline);
            Placeholder.FillCircle(Position + pupilOffset, eyeRadius * 0.45f, Pupil);

            // Bulging droplet during the tell.
            if (swell > 0f)
            {
                Placeholder.Circle(
                    Position + new Vector2(0f, eyeRadius * 0.8f + swell * 3f),
                    dropRadius * swell,
                    DropColor,
                    ScleraOutline);
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            if (onlyWhenPlayerBelow)
            {
                Draw.HollowRect(X - detectWidth / 2f, Y, detectWidth, maxFallDistance, Color.SkyBlue * 0.4f);
            }
        }
        #endregion

        /// <summary>Falling droplet released by a <see cref="WaterEyeDrop"/>.</summary>
        [Tracked]
        public class WaterDroplet : Entity
        {
            private readonly float fallSpeed;
            private readonly float radius;
            private readonly float maxDistance;
            private readonly bool deadly;
            private readonly float startY;
            private float squash;

            public WaterDroplet(Vector2 position, float fallSpeed, float radius, float maxDistance, bool deadly)
                : base(position)
            {
                this.fallSpeed = fallSpeed;
                this.radius = radius;
                this.maxDistance = maxDistance;
                this.deadly = deadly;
                startY = position.Y;

                Collider = new Circle(radius);
                Depth = -8500;
            }

            public override void Update()
            {
                base.Update();

                Position.Y += fallSpeed * Engine.DeltaTime;
                squash = Calc.Approach(squash, 1f, Engine.DeltaTime * 3f);

                if (deadly)
                {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    if (player != null && CollideCheck(player))
                    {
                        player.Die(Vector2.UnitY);
                        Splash();
                        return;
                    }
                }

                if (CollideCheck<Solid>() || Position.Y - startY >= maxDistance)
                {
                    Splash();
                }
            }

            private void Splash()
            {
                Audio.Play(ChapterSfx.WaterSplash, Position);
                RemoveSelf();
            }

            public override void Render()
            {
                base.Render();

                // Stretch as it accelerates so the fall reads clearly.
                float tail = radius * (1f + squash * 1.6f);
                Draw.Rect(Position.X - radius * 0.5f, Position.Y - tail, radius, tail, DropColor * 0.75f);
                Placeholder.Circle(Position, radius, DropColor, Sclera);
            }
        }
    }
}
