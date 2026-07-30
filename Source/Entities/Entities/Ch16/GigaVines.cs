namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// GigaVines - MyWorld's pursuit hazard. A band of vines that creeps across the room
    /// in <c>direction</c> at <c>growSpeed</c>, killing on contact, so the room becomes a
    /// race. Width/height set the band's cross-section; growth extends it lengthways
    /// until <c>maxExtent</c> (or the room edge when that is zero).
    ///
    /// <c>pauseFlag</c> halts growth and <c>retractFlag</c> pulls the vines back, which is
    /// what lets a switch or a boss phase drive them instead of a plain timer.
    ///
    /// Placeholder art: dark green square mass with wobbling tendril squares at the edge.
    /// </summary>
    [CustomEntity("DZ/GigaVines")]
    [Tracked]
    public class GigaVines : Entity
    {
        #region Enums
        public enum GrowDirection
        {
            Up,
            Down,
            Left,
            Right
        }
        #endregion

        #region Fields
        private static readonly Color VineFill = Calc.HexToColor("1f4023");
        private static readonly Color VineMid = Calc.HexToColor("356b3a");
        private static readonly Color VineEdge = Calc.HexToColor("7ac47f");

        private readonly GrowDirection direction;
        private readonly float growSpeed;
        private readonly float retractSpeed;
        private readonly float startDelay;
        private readonly float maxExtent;
        private readonly bool deadly;
        private readonly int tendrilCount;
        private readonly string flag;
        private readonly bool invertFlag;
        private readonly string pauseFlag;
        private readonly string retractFlag;

        private readonly float bandWidth;
        private readonly float bandHeight;
        private readonly Vector2 origin;

        private float extent;
        private float delayTimer;
        private float wobble;
        private Level level;
        #endregion

        #region Constructor
        public GigaVines(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            direction = data.Enum("direction", GrowDirection.Up);
            growSpeed = Math.Max(0f, data.Float("growSpeed", 26f));
            retractSpeed = Math.Max(1f, data.Float("retractSpeed", 70f));
            startDelay = Math.Max(0f, data.Float("startDelay", 1.5f));
            maxExtent = Math.Max(0f, data.Float("maxExtent", 0f));
            deadly = data.Bool("deadly", true);
            tendrilCount = Math.Max(0, data.Int("tendrilCount", 8));
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);
            pauseFlag = data.Attr("pauseFlag", "");
            retractFlag = data.Attr("retractFlag", "");

            bandWidth = Math.Max(8, data.Width);
            bandHeight = Math.Max(8, data.Height);
            origin = Position;

            delayTimer = startDelay;
            Collider = new Hitbox(bandWidth, bandHeight);
            Collidable = false;
            Depth = -8500;
        }
        #endregion

        #region Public API
        /// <summary>How far the vines have grown, in pixels.</summary>
        public float Extent => extent;
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

        private bool FlagOn(string name)
        {
            return !string.IsNullOrEmpty(name) && level != null && level.Session.GetFlag(name);
        }

        /// <summary>Growth ceiling: the configured extent, or the distance to the room edge.</summary>
        private float MaxReach()
        {
            if (maxExtent > 0f)
            {
                return maxExtent;
            }

            if (level == null)
            {
                return 320f;
            }

            switch (direction)
            {
                case GrowDirection.Up:
                    return Math.Max(0f, origin.Y + bandHeight - level.Bounds.Top);
                case GrowDirection.Down:
                    return Math.Max(0f, level.Bounds.Bottom - origin.Y);
                case GrowDirection.Left:
                    return Math.Max(0f, origin.X + bandWidth - level.Bounds.Left);
                default:
                    return Math.Max(0f, level.Bounds.Right - origin.X);
            }
        }

        /// <summary>Rectangle the vine mass currently occupies.</summary>
        private Rectangle GrownBounds()
        {
            switch (direction)
            {
                case GrowDirection.Up:
                    return new Rectangle(
                        (int)origin.X,
                        (int)(origin.Y - extent),
                        (int)bandWidth,
                        (int)(bandHeight + extent));

                case GrowDirection.Down:
                    return new Rectangle(
                        (int)origin.X,
                        (int)origin.Y,
                        (int)bandWidth,
                        (int)(bandHeight + extent));

                case GrowDirection.Left:
                    return new Rectangle(
                        (int)(origin.X - extent),
                        (int)origin.Y,
                        (int)(bandWidth + extent),
                        (int)bandHeight);

                default:
                    return new Rectangle(
                        (int)origin.X,
                        (int)origin.Y,
                        (int)(bandWidth + extent),
                        (int)bandHeight);
            }
        }

        private void SyncCollider()
        {
            Rectangle bounds = GrownBounds();

            Position = new Vector2(bounds.X, bounds.Y);
            Collider.Position = Vector2.Zero;
            Collider.Width = Math.Max(1, bounds.Width);
            Collider.Height = Math.Max(1, bounds.Height);
        }

        /// <summary>Unit vector pointing the way the leading edge advances.</summary>
        private Vector2 GrowthAxis()
        {
            switch (direction)
            {
                case GrowDirection.Up:
                    return -Vector2.UnitY;
                case GrowDirection.Down:
                    return Vector2.UnitY;
                case GrowDirection.Left:
                    return -Vector2.UnitX;
                default:
                    return Vector2.UnitX;
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            SyncCollider();
        }

        public override void Update()
        {
            base.Update();

            wobble += Engine.DeltaTime * 2.2f;

            if (!FlagAllows())
            {
                Collidable = false;
                return;
            }

            Collidable = true;

            if (FlagOn(retractFlag))
            {
                extent = Calc.Approach(extent, 0f, retractSpeed * Engine.DeltaTime);
            }
            else if (delayTimer > 0f)
            {
                delayTimer -= Engine.DeltaTime;
            }
            else if (!FlagOn(pauseFlag))
            {
                extent = Calc.Approach(extent, MaxReach(), growSpeed * Engine.DeltaTime);
            }

            SyncCollider();

            if (deadly)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && !player.Dead && CollideCheck(player))
                {
                    player.Die(-GrowthAxis());
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

            Rectangle bounds = GrownBounds();
            Placeholder.Square(bounds.X, bounds.Y, bounds.Width, bounds.Height, VineFill, VineMid);

            bool vertical = direction == GrowDirection.Up || direction == GrowDirection.Down;

            // Ropey strands running the length of the mass.
            if (vertical)
            {
                for (float x = bounds.Left + 3f; x < bounds.Right - 2f; x += 7f)
                {
                    float sway = (float)Math.Sin(wobble + x * 0.3f) * 1.5f;
                    Draw.Rect(x + sway, bounds.Top, 2f, bounds.Height, VineMid * 0.8f);
                }
            }
            else
            {
                for (float y = bounds.Top + 3f; y < bounds.Bottom - 2f; y += 7f)
                {
                    float sway = (float)Math.Sin(wobble + y * 0.3f) * 1.5f;
                    Draw.Rect(bounds.Left, y + sway, bounds.Width, 2f, VineMid * 0.8f);
                }
            }

            RenderTendrils(bounds, vertical);
        }

        private void RenderTendrils(Rectangle bounds, bool vertical)
        {
            if (tendrilCount <= 0)
            {
                return;
            }

            Vector2 axis = GrowthAxis();

            for (int i = 0; i < tendrilCount; i++)
            {
                float t = (i + 0.5f) / tendrilCount;
                float reach = 3f + (float)Math.Sin(wobble * 1.6f + i * 1.37f) * 3f;

                Vector2 root = vertical
                    ? new Vector2(
                        MathHelper.Lerp(bounds.Left, bounds.Right, t),
                        axis.Y < 0f ? bounds.Top : bounds.Bottom)
                    : new Vector2(
                        axis.X < 0f ? bounds.Left : bounds.Right,
                        MathHelper.Lerp(bounds.Top, bounds.Bottom, t));

                Vector2 tip = root + axis * reach;

                Draw.Line(root, tip, VineEdge * 0.8f);
                Placeholder.FillCircle(tip, 1.5f, VineEdge);
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            Vector2 axis = GrowthAxis();
            Draw.Line(origin, origin + axis * MaxReach(), Color.LimeGreen * 0.5f);
            Draw.HollowRect(origin.X, origin.Y, bandWidth, bandHeight, Color.LimeGreen * 0.6f);
        }
        #endregion
    }
}
