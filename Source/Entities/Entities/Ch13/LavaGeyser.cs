namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// LavaGeyser - Hotcliff eruption on a fixed cycle: dormant, a rumbling warning at
    /// the base, then a lethal column that shoots <c>eruptHeight</c> pixels out before
    /// cooling down. Set <c>downward</c> for ceiling drips.
    ///
    /// The column is only collidable while erupting, so a dormant geyser never blocks
    /// movement. <c>startDelay</c> lets several geysers be placed out of phase.
    ///
    /// Placeholder art: orange square column with a brighter core and a base plate.
    /// </summary>
    [CustomEntity("DZ/LavaGeyser")]
    [Tracked]
    public class LavaGeyser : Entity
    {
        #region Enums
        public enum GeyserState
        {
            Dormant,
            Warning,
            Erupting,
            Cooling
        }
        #endregion

        #region Fields
        private static readonly Color LavaFill = Calc.HexToColor("e8641c");
        private static readonly Color LavaCore = Calc.HexToColor("ffd166");
        private static readonly Color LavaOutline = Calc.HexToColor("7a2408");
        private static readonly Color VentFill = Calc.HexToColor("4a2517");

        public GeyserState State { get; private set; } = GeyserState.Dormant;

        private readonly float columnWidth;
        private readonly float eruptHeight;
        private readonly float warningTime;
        private readonly float eruptDuration;
        private readonly float cooldown;
        private readonly float startDelay;
        private readonly bool downward;
        private readonly bool deadly;
        private readonly string flag;
        private readonly bool invertFlag;

        private float stateTimer;
        private float currentHeight;
        private float rumble;
        private Level level;
        private VertexLight glow;
        #endregion

        #region Constructor
        public LavaGeyser(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            // Loenn resizes this horizontally, so width is the column's only size source.
            columnWidth = Math.Max(4f, data.Width > 0 ? data.Width : 16f);
            eruptHeight = Math.Max(8f, data.Float("eruptHeight", 88f));
            warningTime = Math.Max(0f, data.Float("warningTime", 0.85f));
            eruptDuration = Math.Max(0.1f, data.Float("eruptDuration", 1.1f));
            cooldown = Math.Max(0.1f, data.Float("cooldown", 2.4f));
            startDelay = Math.Max(0f, data.Float("startDelay", 0f));
            downward = data.Bool("downward", false);
            deadly = data.Bool("deadly", true);
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            stateTimer = startDelay > 0f ? startDelay : cooldown;

            Collider = new Hitbox(columnWidth, 1f, -columnWidth / 2f, 0f);
            Collidable = false;
            Depth = -8500;

            Add(glow = new VertexLight(Color.OrangeRed, 0f, 24, 64));
        }
        #endregion

        #region Public API
        /// <summary>Forces an eruption immediately, skipping the remaining cooldown.</summary>
        public void Erupt()
        {
            State = GeyserState.Erupting;
            stateTimer = eruptDuration;
            Collidable = true;
            Audio.Play("event:/game/09_core/rising_threat_appear", Position);
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

        private void UpdateCollider()
        {
            float height = Math.Max(1f, currentHeight);
            Collider.Width = columnWidth;
            Collider.Height = height;
            Collider.Position = downward
                ? new Vector2(-columnWidth / 2f, 0f)
                : new Vector2(-columnWidth / 2f, -height);
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
                Collidable = false;
                currentHeight = 0f;
                glow.Alpha = 0f;
                return;
            }

            stateTimer -= Engine.DeltaTime;

            switch (State)
            {
                case GeyserState.Dormant:
                    currentHeight = Calc.Approach(currentHeight, 0f, eruptHeight * 3f * Engine.DeltaTime);
                    Collidable = false;

                    if (stateTimer <= 0f)
                    {
                        State = GeyserState.Warning;
                        stateTimer = warningTime;
                        Audio.Play("event:/game/09_core/bubble_loop", Position);
                    }
                    break;

                case GeyserState.Warning:
                    rumble += Engine.DeltaTime * 22f;
                    currentHeight = Calc.Approach(currentHeight, 6f, 30f * Engine.DeltaTime);

                    if (stateTimer <= 0f)
                    {
                        Erupt();
                    }
                    break;

                case GeyserState.Erupting:
                    currentHeight = Calc.Approach(currentHeight, eruptHeight, eruptHeight * 5f * Engine.DeltaTime);
                    level?.Shake(0.08f);

                    if (stateTimer <= 0f)
                    {
                        State = GeyserState.Cooling;
                        stateTimer = 0.35f;
                        Collidable = false;
                    }
                    break;

                case GeyserState.Cooling:
                    currentHeight = Calc.Approach(currentHeight, 0f, eruptHeight * 2.5f * Engine.DeltaTime);

                    if (stateTimer <= 0f)
                    {
                        State = GeyserState.Dormant;
                        stateTimer = cooldown;
                    }
                    break;
            }

            UpdateCollider();

            glow.Alpha = MathHelper.Clamp(currentHeight / Math.Max(1f, eruptHeight), 0f, 1f);
            glow.Position = downward
                ? new Vector2(0f, currentHeight / 2f)
                : new Vector2(0f, -currentHeight / 2f);

            if (deadly && Collidable && State == GeyserState.Erupting)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && CollideCheck(player))
                {
                    player.Die(downward ? Vector2.UnitY : -Vector2.UnitY);
                }
            }
        }

        public override void Render()
        {
            base.Render();

            if (!FlagAllows())
            {
                return;
            }

            // Vent plate at the base.
            Placeholder.Square(X - columnWidth / 2f - 2f, downward ? Y - 3f : Y, columnWidth + 4f, 3f, VentFill, LavaOutline);

            if (currentHeight <= 0.5f)
            {
                return;
            }

            float shake = State == GeyserState.Warning ? (float)Math.Sin(rumble) * 1f : 0f;
            float top = downward ? Y : Y - currentHeight;

            Placeholder.Square(
                X - columnWidth / 2f + shake,
                top,
                columnWidth,
                currentHeight,
                LavaFill,
                LavaOutline);

            // Brighter core so the column reads as molten.
            float coreWidth = Math.Max(2f, columnWidth * 0.4f);
            Draw.Rect(X - coreWidth / 2f + shake, top + 1f, coreWidth, Math.Max(1f, currentHeight - 2f), LavaCore * 0.8f);

            // Rounded tip.
            float tipY = downward ? Y + currentHeight : Y - currentHeight;
            Placeholder.Circle(new Vector2(X + shake, tipY), columnWidth * 0.45f, LavaCore, LavaOutline);
        }
        #endregion
    }
}
