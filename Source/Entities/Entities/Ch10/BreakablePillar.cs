namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// BreakablePillar - Ruins stone column that blocks a path until the player
    /// dashes through it. Solid while intact, shatters into debris on a dash hit,
    /// and optionally rebuilds itself after a delay so a room can be retried.
    ///
    /// Placeholder art: stone-grey square with a cracked outline (see Placeholder).
    /// </summary>
    [CustomEntity("DZ/BreakablePillar")]
    [Tracked]
    public class BreakablePillar : Solid
    {
        #region Fields
        private static readonly Color StoneFill = Calc.HexToColor("6f6656");
        private static readonly Color StoneOutline = Calc.HexToColor("3d382e");
        private static readonly Color CrackColor = Calc.HexToColor("2a251e");

        public bool IsBroken { get; private set; }

        private readonly bool requiresDash;
        private readonly bool reboundOnBreak;
        private readonly float respawnDelay;
        private readonly int debrisCount;
        private readonly string flag;

        private float respawnTimer;
        private float crackSeed;
        private Level level;
        #endregion

        #region Constructor
        public BreakablePillar(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height,
                data.Bool("requiresDash", true),
                data.Bool("reboundOnBreak", false),
                data.Float("respawnDelay", 0f),
                data.Int("debrisCount", 10),
                data.Attr("flag", ""))
        {
        }

        public BreakablePillar(Vector2 position, int width, int height, bool requiresDash = true,
            bool reboundOnBreak = false, float respawnDelay = 0f, int debrisCount = 10, string flag = "")
            : base(position, Math.Max(8, width), Math.Max(8, height), safe: true)
        {
            this.requiresDash = requiresDash;
            this.reboundOnBreak = reboundOnBreak;
            this.respawnDelay = respawnDelay;
            this.debrisCount = debrisCount;
            this.flag = flag;

            crackSeed = Calc.Random.NextFloat();
            OnDashCollide = OnDashed;
            SurfaceSoundIndex = SurfaceIndex.Brick;
            Depth = -9000;
        }
        #endregion

        #region Public API
        /// <summary>Shatters the pillar. <paramref name="direction"/> only biases the debris spray.</summary>
        public void Break(Vector2 direction)
        {
            if (IsBroken)
            {
                return;
            }

            IsBroken = true;
            Collidable = false;
            respawnTimer = respawnDelay;

            Audio.Play(ChapterSfx.RuinsPillarBreak, Center);
            level?.Shake(0.2f);

            for (int i = 0; i < debrisCount; i++)
            {
                Vector2 spawn = new Vector2(
                    Calc.Random.Range(Left, Right),
                    Calc.Random.Range(Top, Bottom));

                Vector2 velocity = direction * Calc.Random.Range(40f, 110f)
                    + new Vector2(Calc.Random.Range(-45f, 45f), Calc.Random.Range(-130f, -40f));

                Scene.Add(new PillarDebris(spawn, velocity));
            }

            if (!string.IsNullOrEmpty(flag))
            {
                level?.Session.SetFlag(flag, true);
            }
        }

        /// <summary>Puts the pillar back, refusing while the player would be inside it.</summary>
        public bool Restore()
        {
            if (!IsBroken)
            {
                return true;
            }

            if (CollideCheck<Player>())
            {
                return false;
            }

            IsBroken = false;
            Collidable = true;
            crackSeed = Calc.Random.NextFloat();
            Audio.Play("event:/game/general/fallblock_impact", Center);
            return true;
        }
        #endregion

        #region Private
        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            Break(direction);
            return reboundOnBreak ? DashCollisionResults.Rebound : DashCollisionResults.Ignore;
        }

        private bool TouchedByPlayer()
        {
            return CollideCheck<Player>(Position - Vector2.UnitY)
                || CollideCheck<Player>(Position + Vector2.UnitY)
                || CollideCheck<Player>(Position - Vector2.UnitX)
                || CollideCheck<Player>(Position + Vector2.UnitX);
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

            if (IsBroken)
            {
                if (respawnDelay <= 0f)
                {
                    return;
                }

                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f && !Restore())
                {
                    // Player is standing in the gap - try again shortly.
                    respawnTimer = 0.25f;
                }

                return;
            }

            // Dash handling goes through OnDashCollide; this is the "any contact" mode.
            if (!requiresDash && TouchedByPlayer())
            {
                Break(Vector2.UnitY * -1f);
            }
        }

        public override void Render()
        {
            if (IsBroken)
            {
                if (respawnDelay > 0f)
                {
                    // Ghost outline so the player can see it coming back.
                    Draw.HollowRect(X, Y, Width, Height, StoneOutline * 0.35f);
                }

                return;
            }

            Placeholder.Square(X, Y, Width, Height, StoneFill, StoneOutline);

            // Cracks derived from crackSeed rather than Calc.Random, so they stay put
            // frame to frame and only change when the pillar is rebuilt.
            int bands = Math.Max(1, (int)(Height / 12f));
            float crackWidth = Math.Max(2f, Width * 0.35f);
            float slack = Math.Max(0f, Width - 4f - crackWidth);

            for (int i = 1; i <= bands; i++)
            {
                float y = (float)Math.Floor(Y + Height * (i / (float)(bands + 1)));
                float offset = ((crackSeed + i * 0.37f) % 1f) * slack;
                Draw.Rect(X + 2f + offset, y, crackWidth, 1f, CrackColor);
            }
        }
        #endregion

        /// <summary>Chunk of masonry thrown out when a pillar breaks.</summary>
        private class PillarDebris : Entity
        {
            private Vector2 velocity;
            private readonly float size;
            private readonly float maxLifetime;
            private float lifetime;

            public PillarDebris(Vector2 position, Vector2 velocity)
                : base(position)
            {
                this.velocity = velocity;
                size = Calc.Random.Range(2f, 5f);
                maxLifetime = Calc.Random.Range(0.5f, 1.1f);
                lifetime = maxLifetime;
                Depth = -9001;
            }

            public override void Update()
            {
                base.Update();

                Position += velocity * Engine.DeltaTime;
                velocity.X = Calc.Approach(velocity.X, 0f, 120f * Engine.DeltaTime);
                velocity.Y += 480f * Engine.DeltaTime;

                lifetime -= Engine.DeltaTime;
                if (lifetime <= 0f)
                {
                    RemoveSelf();
                }
            }

            public override void Render()
            {
                float alpha = MathHelper.Clamp(lifetime / maxLifetime, 0f, 1f);
                Draw.Rect(Position.X, Position.Y, size, size, StoneFill * alpha);
            }
        }
    }
}
