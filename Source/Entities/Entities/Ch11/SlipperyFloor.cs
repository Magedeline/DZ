namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// SlipperyFloor - Snowding City ice. A normal solid to stand on, except the
    /// player keeps their horizontal momentum instead of stopping: releasing the
    /// stick slides, and turning around costs distance.
    ///
    /// <c>slipperiness</c> is how much of last frame's speed is restored each frame
    /// (0 = normal ground, 1 = frictionless). <c>slideSpeed</c> adds a constant pull
    /// in <c>slideDirection</c> for downhill ice.
    ///
    /// Placeholder art: pale blue square with a highlight band.
    /// </summary>
    [CustomEntity("DZ/SlipperyFloor")]
    [Tracked]
    public class SlipperyFloor : Solid
    {
        #region Enums
        public enum SlideDirection
        {
            None,
            Left,
            Right
        }
        #endregion

        #region Fields
        private static readonly Color IceFill = Calc.HexToColor("9fd8e8");
        private static readonly Color IceOutline = Calc.HexToColor("5c93ad");
        private static readonly Color IceShine = Calc.HexToColor("e4f7ff");

        private readonly float slipperiness;
        private readonly float slideSpeed;
        private readonly SlideDirection slideDirection;
        private readonly bool affectsEnemies;

        private float retainedSpeedX;
        private float shineOffset;
        private SoundSource iceLoop;
        #endregion

        #region Constructor
        public SlipperyFloor(EntityData data, Vector2 offset)
            : base(data.Position + offset, Math.Max(8, data.Width), Math.Max(8, data.Height),
                data.Bool("safeGround", true))
        {
            slipperiness = MathHelper.Clamp(data.Float("slipperiness", 0.9f), 0f, 1f);
            slideSpeed = data.Float("slideSpeed", 0f);
            slideDirection = data.Enum("slideDirection", SlideDirection.None);
            affectsEnemies = data.Bool("affectsEnemies", false);

            SurfaceSoundIndex = SurfaceIndex.Snow;
            shineOffset = Calc.Random.NextFloat() * 32f;

            // Sliding loop, started and stopped as the player gets on and off.
            Add(iceLoop = new SoundSource());
        }
        #endregion

        #region Private
        private void ApplyIce(Player player)
        {
            float current = player.Speed.X;

            // Only fight deceleration, never the player's own acceleration or a turn,
            // so ice feels slippery rather than uncontrollable.
            bool decelerating = Math.Abs(current) < Math.Abs(retainedSpeedX);
            bool sameDirection = current == 0f || Math.Sign(current) == Math.Sign(retainedSpeedX);

            if (decelerating && sameDirection)
            {
                player.Speed.X = MathHelper.Lerp(current, retainedSpeedX, slipperiness);
            }

            if (slideDirection != SlideDirection.None && slideSpeed > 0f)
            {
                int sign = slideDirection == SlideDirection.Left ? -1 : 1;
                player.Speed.X = Calc.Approach(player.Speed.X, sign * slideSpeed, slideSpeed * 2f * Engine.DeltaTime);
            }

            retainedSpeedX = player.Speed.X;
        }
        #endregion

        #region Entity Overrides
        public override void Update()
        {
            base.Update();

            Player player = GetPlayerOnTop();
            if (player != null)
            {
                ApplyIce(player);

                if (!iceLoop.Playing)
                {
                    iceLoop.Play(ChapterSfx.SnowIceIdle);
                }
            }
            else
            {
                retainedSpeedX = 0f;

                if (iceLoop.Playing)
                {
                    iceLoop.Stop();
                }
            }

            if (affectsEnemies && slideDirection != SlideDirection.None && slideSpeed > 0f)
            {
                int sign = slideDirection == SlideDirection.Left ? -1 : 1;
                foreach (Entity entity in Scene.Tracker.GetEntities<Enemy>())
                {
                    Enemy enemy = (Enemy)entity;
                    if (enemy.CollideCheck(this, enemy.Position + Vector2.UnitY))
                    {
                        enemy.Position.X += sign * slideSpeed * 0.5f * Engine.DeltaTime;
                    }
                }
            }

            shineOffset += 6f * Engine.DeltaTime;
            if (shineOffset > Width + 32f)
            {
                shineOffset = -32f;
            }
        }

        public override void Render()
        {
            Placeholder.Square(X, Y, Width, Height, IceFill, IceOutline);

            // Top highlight so ice reads differently from ordinary tiles.
            Draw.Rect(X + 1f, Y + 1f, Width - 2f, 1f, IceShine);

            // Drifting glint.
            float glintX = X + shineOffset;
            if (glintX > X && glintX < Right - 6f)
            {
                Draw.Rect(glintX, Y + 2f, 5f, 1f, IceShine * 0.7f);
            }

            if (slideDirection != SlideDirection.None && slideSpeed > 0f)
            {
                int sign = slideDirection == SlideDirection.Left ? -1 : 1;
                for (float x = X + 4f; x < Right - 4f; x += 10f)
                {
                    Draw.Rect(x, CenterY, 4f, 1f, IceOutline * 0.6f);
                    Draw.Rect(x + (sign > 0 ? 3f : 0f), CenterY - 1f, 1f, 3f, IceOutline * 0.6f);
                }
            }
        }
        #endregion
    }
}
