namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// AntiGravityField - Citadel zone that cancels the player's fall and pushes them
    /// upward while they are inside. Deliberately implemented as an upward force rather
    /// than a full gravity flip, so it needs no GravityHelper dependency and the player's
    /// controls stay the way they are everywhere else.
    ///
    /// Shipped alongside <see cref="AntiGravityPowerUp"/>: the same effect as a timed
    /// pickup, so anti-gravity can be a room feature or a carried ability.
    ///
    /// Placeholder art: hollow purple square with rising chevrons.
    /// </summary>
    [CustomEntity("DZ/AntiGravityField")]
    [Tracked]
    public class AntiGravityField : Entity
    {
        #region Fields
        private static readonly Color FieldFill = Calc.HexToColor("6b3fa0");
        private static readonly Color FieldEdge = Calc.HexToColor("d9b8ff");

        private readonly float riseAcceleration;
        private readonly float maxRiseSpeed;
        private readonly bool showBorder;
        private readonly bool affectsEnemies;
        private readonly string flag;
        private readonly bool invertFlag;

        private float scroll;
        private Level level;
        #endregion

        #region Constructor
        public AntiGravityField(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            riseAcceleration = Math.Max(1f, data.Float("riseAcceleration", 340f));
            maxRiseSpeed = Math.Max(1f, data.Float("maxRiseSpeed", 110f));
            showBorder = data.Bool("showBorder", true);
            affectsEnemies = data.Bool("affectsEnemies", false);
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            Collider = new Hitbox(Math.Max(8, data.Width), Math.Max(8, data.Height));
            Depth = 1000;
        }
        #endregion

        #region Public API
        /// <summary>Applies one frame of lift. Shared with the power-up so both feel identical.</summary>
        public static void ApplyLift(Player player, float riseAcceleration, float maxRiseSpeed)
        {
            if (player == null || player.Dead)
            {
                return;
            }

            player.Speed.Y = Calc.Approach(player.Speed.Y, -maxRiseSpeed, riseAcceleration * Engine.DeltaTime);
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

            scroll -= 22f * Engine.DeltaTime;
            if (scroll < -12f)
            {
                scroll += 12f;
            }

            if (!FlagAllows())
            {
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                ApplyLift(player, riseAcceleration, maxRiseSpeed);
            }

            if (affectsEnemies)
            {
                foreach (Entity entity in Scene.Tracker.GetEntities<Enemy>())
                {
                    Enemy enemy = (Enemy)entity;
                    if (enemy.IsAlive && CollideCheck(enemy))
                    {
                        enemy.Position.Y -= maxRiseSpeed * 0.5f * Engine.DeltaTime;
                    }
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

            Rectangle bounds = Collider.Bounds;

            Draw.Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height, FieldFill * 0.18f);

            if (showBorder)
            {
                Draw.HollowRect(bounds.X, bounds.Y, bounds.Width, bounds.Height, FieldEdge * 0.6f);
            }

            // Rising chevrons showing the push direction.
            for (float y = bounds.Bottom + scroll; y > bounds.Top; y -= 12f)
            {
                for (float x = bounds.X + 4f; x < bounds.Right - 4f; x += 12f)
                {
                    if (y - 3f < bounds.Top)
                    {
                        continue;
                    }

                    Draw.Rect(x, y - 1f, 5f, 1f, FieldEdge * 0.45f);
                    Draw.Rect(x + 2f, y - 3f, 1f, 3f, FieldEdge * 0.45f);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// AntiGravityPowerUp - pickup that grants anti-gravity for <c>duration</c> seconds
    /// anywhere in the level, using the same lift maths as the field.
    /// </summary>
    [CustomEntity("DZ/AntiGravityPowerUp")]
    [Tracked]
    public class AntiGravityPowerUp : Entity
    {
        #region Fields
        private static readonly Color PickupFill = Calc.HexToColor("6b3fa0");
        private static readonly Color PickupEdge = Calc.HexToColor("d9b8ff");

        private readonly float duration;
        private readonly float riseAcceleration;
        private readonly float maxRiseSpeed;
        private readonly float respawnDelay;
        private readonly bool oneUse;
        private readonly float radius;
        private readonly string flag;

        private bool collected;
        private float respawnTimer;
        private float bob;
        private Level level;
        #endregion

        #region Constructor
        public AntiGravityPowerUp(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            duration = Math.Max(0.2f, data.Float("duration", 4f));
            riseAcceleration = Math.Max(1f, data.Float("riseAcceleration", 340f));
            maxRiseSpeed = Math.Max(1f, data.Float("maxRiseSpeed", 110f));
            respawnDelay = Math.Max(0f, data.Float("respawnDelay", 3f));
            oneUse = data.Bool("oneUse", false);
            radius = Math.Max(4f, data.Float("radius", 8f));
            flag = data.Attr("flag", "");

            Collider = new Circle(radius);
            Depth = -100;

            Add(new VertexLight(Color.MediumPurple, 0.6f, 16, 40));
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
            Scene.Add(new AntiGravityBuff(duration, riseAcceleration, maxRiseSpeed));

            if (!string.IsNullOrEmpty(flag))
            {
                level?.Session.SetFlag(flag, true);
            }

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

            // Up arrow inside.
            Draw.Rect(center.X - 0.5f, center.Y - radius * 0.5f, 1f, radius, PickupEdge);
            Draw.Rect(center.X - 2.5f, center.Y - radius * 0.5f, 5f, 1f, PickupEdge);
        }
        #endregion
    }

    /// <summary>Timed anti-gravity granted by <see cref="AntiGravityPowerUp"/>.</summary>
    [Tracked]
    public class AntiGravityBuff : Entity
    {
        private readonly float riseAcceleration;
        private readonly float maxRiseSpeed;
        private float remaining;

        public AntiGravityBuff(float duration, float riseAcceleration, float maxRiseSpeed)
        {
            remaining = duration;
            this.riseAcceleration = riseAcceleration;
            this.maxRiseSpeed = maxRiseSpeed;

            Tag = Tags.Global;
            Depth = -10000;
        }

        public override void Update()
        {
            base.Update();

            remaining -= Engine.DeltaTime;
            if (remaining <= 0f)
            {
                RemoveSelf();
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && player.StateMachine.State == Player.StNormal)
            {
                AntiGravityField.ApplyLift(player, riseAcceleration, maxRiseSpeed);
            }
        }
    }
}
