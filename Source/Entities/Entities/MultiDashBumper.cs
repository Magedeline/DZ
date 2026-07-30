namespace Celeste.Entities
{
    /// <summary>
    /// MultiDashBumper - a pinball bumper that also refills the player's dashes.
    ///
    /// One entity covers the whole family: <c>dashes</c> sets how many the player walks
    /// away with. 1-5 each get their own colour; anything from 6 up (the rainbow bumper
    /// ships as 10) cycles through the spectrum instead. Loenn exposes each count as its
    /// own placement so a mapper picks "3 Dash Bumper" rather than typing a number.
    ///
    /// By default the bumper only ever raises the dash count - bouncing a player who
    /// already has 5 dashes off a 1-dash bumper does not rob them. Set
    /// <c>setExactly</c> when a room needs the bumper to clamp downward as well.
    ///
    /// Placeholder art: circle with a ring of pips, one pip per dash granted.
    /// </summary>
    [CustomEntity("DZ/MultiDashBumper")]
    [Tracked]
    public class MultiDashBumper : Entity
    {
        #region Constants
        /// <summary>Dash counts at or above this render as the rainbow bumper.</summary>
        public const int RainbowThreshold = 6;

        /// <summary>Fixed colour per dash count for 1-5; index 0 is unused.</summary>
        private static readonly Color[] DashColors =
        {
            Color.White,
            Calc.HexToColor("ff3355"),
            Calc.HexToColor("ff77dd"),
            Calc.HexToColor("7d5fff"),
            Calc.HexToColor("2fd8ff"),
            Calc.HexToColor("ffd53d")
        };

        private static readonly Color RingColor = Calc.HexToColor("ffffff");
        private static readonly Color SpentColor = Calc.HexToColor("4a4a55");
        #endregion

        #region Fields
        public int Dashes { get; }
        public bool IsRainbow => Dashes >= RainbowThreshold;

        private readonly float radius;
        private readonly float respawnTime;
        private readonly float launchMultiplier;
        private readonly bool setExactly;
        private readonly bool refillStamina;
        private readonly bool oneUse;
        private readonly float moveTime;
        private readonly bool sineWave;
        private readonly string flag;
        private readonly bool invertFlag;

        private Vector2 anchor;
        private float respawnTimer;
        private float hueCycle;
        private float hitFlash;
        private SineWave sine;
        private VertexLight light;
        private Level level;
        #endregion

        #region Constructor
        public MultiDashBumper(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            // 10 is the documented rainbow value; allow a little headroom above it.
            Dashes = Math.Clamp(data.Int("dashes", 1), 1, 20);

            radius = Math.Max(4f, data.Float("radius", 12f));
            respawnTime = Math.Max(0.05f, data.Float("respawnTime", 0.6f));
            launchMultiplier = Math.Max(0.1f, data.Float("launchMultiplier", 1f));
            setExactly = data.Bool("setExactly", false);
            refillStamina = data.Bool("refillStamina", true);
            oneUse = data.Bool("oneUse", false);
            moveTime = Math.Max(0.1f, data.Float("moveTime", 1.81f));
            sineWave = data.Bool("sineWave", true);
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            anchor = Position;
            hueCycle = Calc.Random.NextFloat();

            Collider = new Circle(radius);
            Depth = -100;

            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(CurrentColor(), 1f, 16, 48));

            if (sineWave)
            {
                Add(sine = new SineWave(0.44f, 0f));
                sine.Randomize();
            }

            // A node turns the bumper into a back-and-forth mover, like the vanilla one.
            if (data.Nodes != null && data.Nodes.Length > 0)
            {
                Vector2 start = Position;
                Vector2 end = data.Nodes[0] + offset;

                Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.CubeInOut, moveTime, start: true);
                tween.OnUpdate = t => anchor = Vector2.Lerp(start, end, t.Eased);
                Add(tween);
            }
        }
        #endregion

        #region Public API
        /// <summary>Colour for this bumper right now; rainbow bumpers cycle over time.</summary>
        public Color CurrentColor()
        {
            if (IsRainbow)
            {
                return ColorUtils.FromHsv(hueCycle * 360f, 0.85f, 1f);
            }

            return DashColors[Math.Clamp(Dashes, 1, DashColors.Length - 1)];
        }
        #endregion

        #region Private
        private bool FlagAllows()
        {
            if (string.IsNullOrEmpty(flag) || level == null)
            {
                return true;
            }

            return level.Session.GetFlag(flag) != invertFlag;
        }

        private void GrantDashes(Player player)
        {
            player.Dashes = setExactly ? Dashes : Math.Max(player.Dashes, Dashes);

            if (refillStamina)
            {
                player.RefillStamina();
            }
        }

        private void OnPlayer(Player player)
        {
            if (respawnTimer > 0f || !FlagAllows())
            {
                return;
            }

            respawnTimer = respawnTime;
            hitFlash = 1f;

            GrantDashes(player);

            Vector2 launch = player.ExplodeLaunch(Position, false, false);
            if (launchMultiplier != 1f)
            {
                player.Speed = launch * launchMultiplier;
            }

            Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);

            if (level != null)
            {
                level.DirectionalShake(launch, 0.15f);
                level.Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
            }

            if (oneUse)
            {
                RemoveSelf();
            }
        }

        private void UpdatePosition()
        {
            Position = sine != null
                ? anchor + new Vector2(sine.Value * 3f, sine.ValueOverTwo * 2f)
                : anchor;
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            UpdatePosition();
        }

        public override void Update()
        {
            base.Update();

            if (IsRainbow)
            {
                hueCycle = (hueCycle + Engine.DeltaTime * 0.45f) % 1f;
            }

            hitFlash = Calc.Approach(hitFlash, 0f, Engine.DeltaTime * 3f);

            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
            }

            Collidable = respawnTimer <= 0f && FlagAllows();

            UpdatePosition();

            light.Color = CurrentColor();
            light.Alpha = Collidable ? 1f : 0.3f;
        }

        public override void Render()
        {
            base.Render();

            if (!FlagAllows())
            {
                return;
            }

            bool ready = respawnTimer <= 0f;
            Color color = ready ? CurrentColor() : SpentColor;

            // Outer glow, punchier for a moment after a hit.
            float glow = 0.25f + hitFlash * 0.5f;
            Draw.Circle(Position, radius + 3f + hitFlash * 3f, color * glow, 16);

            Chapters.Placeholder.Circle(Position, radius, color * (ready ? 0.85f : 0.4f), RingColor * (ready ? 1f : 0.3f));

            if (!ready)
            {
                return;
            }

            RenderDashPips(color);
        }

        /// <summary>One pip per dash, arranged in a ring so the count is readable at a glance.</summary>
        private void RenderDashPips(Color color)
        {
            int pips = Math.Clamp(Dashes, 1, 20);

            if (pips == 1)
            {
                Chapters.Placeholder.SquareCentered(Position, 4f, RingColor, color);
                return;
            }

            float pipRadius = radius * 0.5f;
            float pipSize = pips > 8 ? 2.5f : 3.5f;

            for (int i = 0; i < pips; i++)
            {
                float angle = -MathHelper.PiOver2 + i * (MathHelper.TwoPi / pips);

                // Rainbow bumpers tint each pip separately so the ring reads as a spectrum.
                Color pipColor = IsRainbow
                    ? ColorUtils.FromHsv((hueCycle + i / (float)pips) % 1f * 360f, 0.85f, 1f)
                    : RingColor;

                Chapters.Placeholder.SquareCentered(
                    Position + Calc.AngleToVector(angle, pipRadius),
                    pipSize,
                    pipColor,
                    color);
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Circle(anchor, 2f, Color.Yellow * 0.6f, 6);
        }
        #endregion
    }
}
