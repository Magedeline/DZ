namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// TreadmillFloor - Hotcliff conveyor plate. Standing on it drags the player along
    /// at <c>speed</c>; running against the belt costs ground. Set
    /// <c>reverseInterval</c> above zero to make the belt flip direction on a timer.
    ///
    /// The belt uses MoveH rather than touching Speed, so the player keeps full control
    /// of their own acceleration and the drag simply adds on top.
    ///
    /// Placeholder art: hot orange square with scrolling chevrons.
    /// </summary>
    [CustomEntity("DZ/TreadmillFloor")]
    [Tracked]
    public class TreadmillFloor : Solid
    {
        #region Fields
        private static readonly Color BeltFill = Calc.HexToColor("8c3b1e");
        private static readonly Color BeltOutline = Calc.HexToColor("3d1408");
        private static readonly Color ChevronColor = Calc.HexToColor("ff9e4a");

        private readonly float speed;
        private readonly float reverseInterval;
        private readonly bool affectsEnemies;
        private readonly string flag;
        private readonly bool invertFlag;

        private int direction;
        private float reverseTimer;
        private float scroll;
        private Level level;
        private SoundSource beltLoop;
        #endregion

        #region Constructor
        public TreadmillFloor(EntityData data, Vector2 offset)
            : base(data.Position + offset, Math.Max(8, data.Width), Math.Max(4, data.Height), safe: true)
        {
            speed = Math.Max(0f, data.Float("speed", 70f));
            reverseInterval = Math.Max(0f, data.Float("reverseInterval", 0f));
            affectsEnemies = data.Bool("affectsEnemies", false);
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            direction = data.Bool("moveLeft", false) ? -1 : 1;
            reverseTimer = reverseInterval;

            SurfaceSoundIndex = SurfaceIndex.Girder;
            Depth = -9000;

            // Belt loop, held for as long as the treadmill is running.
            Add(beltLoop = new SoundSource());
        }
        #endregion

        #region Public API
        /// <summary>Current belt direction: -1 left, 1 right.</summary>
        public int Direction => direction;

        public void Reverse()
        {
            direction = -direction;
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

            bool running = FlagAllows() && speed > 0f;

            if (running != beltLoop.Playing)
            {
                if (running)
                {
                    beltLoop.Play(ChapterSfx.HotTreadmill);
                }
                else
                {
                    beltLoop.Stop();
                }
            }

            if (!running)
            {
                return;
            }

            if (reverseInterval > 0f)
            {
                reverseTimer -= Engine.DeltaTime;
                if (reverseTimer <= 0f)
                {
                    reverseTimer = reverseInterval;
                    Reverse();
                }
            }

            float drag = speed * direction * Engine.DeltaTime;

            Player player = GetPlayerOnTop();
            if (player != null)
            {
                player.MoveH(drag);
            }

            if (affectsEnemies)
            {
                foreach (Entity entity in Scene.Tracker.GetEntities<Enemy>())
                {
                    Enemy enemy = (Enemy)entity;
                    if (enemy.CollideCheck(this, enemy.Position + Vector2.UnitY))
                    {
                        enemy.MoveH(drag);
                    }
                }
            }

            scroll += speed * direction * Engine.DeltaTime * 0.5f;
            scroll %= 8f;
        }

        public override void Render()
        {
            Placeholder.Square(X, Y, Width, Height, BeltFill, BeltOutline);

            if (!FlagAllows())
            {
                return;
            }

            // Scrolling chevrons showing which way the belt pulls.
            float offset = scroll < 0f ? scroll + 8f : scroll;
            for (float x = X - 8f + offset; x < Right; x += 8f)
            {
                float cx = x + (direction > 0 ? 0f : 4f);
                if (cx < X || cx + 4f > Right)
                {
                    continue;
                }

                Draw.Rect(cx, Y + 1f, 4f, 1f, ChevronColor);
                Draw.Rect(cx + (direction > 0 ? 3f : 0f), Y + 1f, 1f, Math.Max(1f, Height - 2f), ChevronColor * 0.8f);
            }
        }
        #endregion
    }
}
