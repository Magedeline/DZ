namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// ThrowableSpear - Undyne's spear as a carryable power-up. Pick it up like Theo,
    /// throw it, and it flies flat and fast instead of arcing: it damages enemies on
    /// the way through and sticks into the first wall it hits. A stuck spear can
    /// optionally become a small ledge (<c>platformWhenStuck</c>), which makes it a
    /// traversal tool and not just a weapon.
    ///
    /// <c>respawnDelay</c> above zero returns the spear to its spawn point, so one
    /// placement can serve a whole room as a reusable ability.
    ///
    /// Placeholder art: long thin square shaft plus a square tip, oriented along flight.
    /// </summary>
    [CustomEntity("DZ/ThrowableSpear")]
    [Tracked]
    public class ThrowableSpear : Actor
    {
        #region Constants
        private const float Gravity = 800f;
        private const float MaxFall = 200f;
        private const float AirFriction = 350f;
        #endregion

        #region Enums
        public enum SpearState
        {
            Loose,
            Held,
            Flying,
            Stuck
        }
        #endregion

        #region Fields
        private static readonly Color ShaftFill = Calc.HexToColor("2f8fd6");
        private static readonly Color ShaftOutline = Calc.HexToColor("d8f2ff");
        private static readonly Color TipFill = Calc.HexToColor("eaf6ff");

        public Vector2 Speed;
        public SpearState State { get; private set; } = SpearState.Loose;

        private readonly float throwSpeed;
        private readonly float stickDuration;
        private readonly float respawnDelay;
        private readonly int damage;
        private readonly bool killsEnemies;
        private readonly bool sticksToSolids;
        private readonly bool platformWhenStuck;
        private readonly float spearLength;
        private readonly float spearThickness;

        private readonly Vector2 spawnPosition;
        private Holdable hold;
        private Collision onCollideH;
        private Collision onCollideV;
        private float noGravityTimer;
        private float stateTimer;
        private Vector2 flightDirection = Vector2.UnitX;
        private SpearLedge ledge;
        private Level level;
        #endregion

        #region Constructor
        public ThrowableSpear(EntityData data, Vector2 offset)
            : this(data.Position + offset,
                data.Float("throwSpeed", 300f),
                data.Float("stickDuration", 4f),
                data.Float("respawnDelay", 2.5f),
                data.Int("damage", 2),
                data.Bool("killsEnemies", true),
                data.Bool("sticksToSolids", true),
                data.Bool("platformWhenStuck", false),
                data.Float("spearLength", 20f),
                data.Float("spearThickness", 4f))
        {
        }

        public ThrowableSpear(Vector2 position, float throwSpeed = 300f, float stickDuration = 4f,
            float respawnDelay = 2.5f, int damage = 2, bool killsEnemies = true, bool sticksToSolids = true,
            bool platformWhenStuck = false, float spearLength = 20f, float spearThickness = 4f)
            : base(position)
        {
            this.throwSpeed = Math.Max(40f, throwSpeed);
            this.stickDuration = Math.Max(0.1f, stickDuration);
            this.respawnDelay = Math.Max(0f, respawnDelay);
            this.damage = Math.Max(1, damage);
            this.killsEnemies = killsEnemies;
            this.sticksToSolids = sticksToSolids;
            this.platformWhenStuck = platformWhenStuck;
            this.spearLength = Math.Max(8f, spearLength);
            this.spearThickness = Math.Max(2f, spearThickness);

            spawnPosition = position;
            Depth = 100;
            Collider = new Hitbox(8f, 8f, -4f, -8f);

            Add(hold = new Holdable());
            hold.PickupCollider = new Hitbox(16f, 20f, -8f, -16f);
            hold.SlowFall = false;
            hold.SlowRun = false;
            hold.OnPickup = OnPickup;
            hold.OnRelease = OnRelease;
            hold.SpeedGetter = () => Speed;
            hold.OnHitSpring = OnHitSpring;
            hold.DangerousCheck = _ => State == SpearState.Flying;

            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;

            Add(new VertexLight(Color.LightSkyBlue, 0.5f, 12, 28));
        }
        #endregion

        #region Public API
        /// <summary>Sends the spear flying in <paramref name="direction"/> at full throw speed.</summary>
        public void Launch(Vector2 direction)
        {
            flightDirection = direction.SafeNormalize(Vector2.UnitX);
            Speed = flightDirection * throwSpeed;
            State = SpearState.Flying;
            stateTimer = 0f;
            RemoveLedge();
            Audio.Play(ChapterSfx.WaterSpearThrow, Position);
        }

        /// <summary>Returns the spear to its placed position, ready to be picked up again.</summary>
        public void Respawn()
        {
            RemoveLedge();
            Position = spawnPosition;
            Speed = Vector2.Zero;
            State = SpearState.Loose;
            stateTimer = 0f;
            Collidable = true;
            Visible = true;
            Audio.Play(ChapterSfx.WaterSpearAppear, Position);
        }
        #endregion

        #region Private - holdable plumbing
        private void OnPickup()
        {
            Speed = Vector2.Zero;
            State = SpearState.Held;
            RemoveLedge();
            AddTag(Tags.Persistent);
            RemoveTag(Tags.Persistent);
        }

        private void OnRelease(Vector2 force)
        {
            RemoveTag(Tags.Persistent);

            if (force == Vector2.Zero)
            {
                // Gentle drop - behave like a normal held object.
                State = SpearState.Loose;
                Speed = Vector2.Zero;
                return;
            }

            // A real throw: flatten the arc and go fast.
            Vector2 direction = force;
            if (Math.Abs(direction.Y) < 0.4f)
            {
                direction.Y = 0f;
            }

            Launch(direction);
        }

        private bool OnHitSpring(Spring spring)
        {
            if (hold.IsHeld)
            {
                return false;
            }

            if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
            {
                Speed.X *= 0.5f;
                Speed.Y = -160f;
                noGravityTimer = 0.15f;
                State = SpearState.Loose;
                return true;
            }

            if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
            {
                Launch(Vector2.UnitX);
                return true;
            }

            if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
            {
                Launch(-Vector2.UnitX);
                return true;
            }

            return false;
        }

        private void OnCollideH(CollisionData data)
        {
            if (State == SpearState.Flying)
            {
                Stick();
                return;
            }

            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            Speed.X *= -0.4f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (State == SpearState.Flying)
            {
                Stick();
                return;
            }

            if (Speed.Y > 0f)
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position);
            }

            Speed.Y = Math.Abs(Speed.Y) > 80f ? Speed.Y * -0.4f : 0f;
        }
        #endregion

        #region Private - states
        private void Stick()
        {
            if (!sticksToSolids)
            {
                Dissipate();
                return;
            }

            State = SpearState.Stuck;
            stateTimer = stickDuration;
            Speed = Vector2.Zero;
            Audio.Play("event:/game/general/wall_break_stone", Position);

            if (platformWhenStuck && Math.Abs(flightDirection.X) > Math.Abs(flightDirection.Y))
            {
                // Horizontal spear buried in a wall doubles as a ledge.
                ledge = new SpearLedge(
                    new Vector2(Center.X - spearLength / 2f, Center.Y - spearThickness / 2f),
                    spearLength,
                    Math.Max(3f, spearThickness));
                Scene.Add(ledge);
            }
        }

        private void Dissipate()
        {
            RemoveLedge();

            if (respawnDelay > 0f)
            {
                State = SpearState.Stuck;
                stateTimer = 0f;
                Visible = false;
                Collidable = false;
                ScheduleRespawn();
                return;
            }

            RemoveSelf();
        }

        private void ScheduleRespawn()
        {
            Add(new Coroutine(RespawnRoutine()));
        }

        private IEnumerator RespawnRoutine()
        {
            yield return respawnDelay;
            Respawn();
        }

        private void RemoveLedge()
        {
            if (ledge != null)
            {
                ledge.RemoveSelf();
                ledge = null;
            }
        }

        private void DamageEnemies()
        {
            if (!killsEnemies)
            {
                return;
            }

            foreach (Entity entity in Scene.Tracker.GetEntities<Enemy>())
            {
                Enemy enemy = (Enemy)entity;
                if (enemy.IsAlive && CollideCheck(enemy))
                {
                    enemy.TakeDamage(damage);
                    Audio.Play("event:/game/general/thing_booped", Position);
                }
            }
        }

        private void LooseMovement()
        {
            if (OnGround())
            {
                Speed.X = Calc.Approach(Speed.X, 0f, AirFriction * Engine.DeltaTime);
            }
            else if (hold.ShouldHaveGravity)
            {
                float gravity = Math.Abs(Speed.Y) <= 30f ? Gravity * 0.5f : Gravity;
                Speed.X = Calc.Approach(Speed.X, 0f, AirFriction * Engine.DeltaTime);

                if (noGravityTimer > 0f)
                {
                    noGravityTimer -= Engine.DeltaTime;
                }
                else
                {
                    Speed.Y = Calc.Approach(Speed.Y, MaxFall, gravity * Engine.DeltaTime);
                }
            }

            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
        }

        private void FlyingMovement()
        {
            stateTimer += Engine.DeltaTime;

            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            if (State != SpearState.Flying)
            {
                return;
            }

            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            if (State != SpearState.Flying)
            {
                return;
            }

            DamageEnemies();

            if (level != null && !level.Bounds.Contains((int)X, (int)Y))
            {
                Dissipate();
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Removed(Scene scene)
        {
            RemoveLedge();
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();

            if (hold.IsHeld)
            {
                State = SpearState.Held;
                Speed = Vector2.Zero;
                hold.CheckAgainstColliders();
                return;
            }

            switch (State)
            {
                case SpearState.Held:
                    State = SpearState.Loose;
                    break;

                case SpearState.Loose:
                    LooseMovement();
                    break;

                case SpearState.Flying:
                    FlyingMovement();
                    break;

                case SpearState.Stuck:
                    if (stateTimer > 0f)
                    {
                        stateTimer -= Engine.DeltaTime;
                        if (stateTimer <= 0f)
                        {
                            Dissipate();
                        }
                    }
                    break;
            }

            hold.CheckAgainstColliders();
        }

        public override void Render()
        {
            base.Render();

            if (!Visible)
            {
                return;
            }

            Vector2 axis = State == SpearState.Loose || State == SpearState.Held
                ? Vector2.UnitX
                : flightDirection;

            bool horizontal = Math.Abs(axis.X) >= Math.Abs(axis.Y);
            Vector2 center = Collider.Center + Position;

            float length = spearLength;
            float thickness = spearThickness;

            if (horizontal)
            {
                Placeholder.Square(center.X - length / 2f, center.Y - thickness / 2f, length, thickness, ShaftFill, ShaftOutline);

                float tipX = axis.X >= 0f ? center.X + length / 2f - thickness : center.X;
                Placeholder.Square(tipX, center.Y - thickness, thickness * 1.5f, thickness * 2f, TipFill, ShaftOutline);
            }
            else
            {
                Placeholder.Square(center.X - thickness / 2f, center.Y - length / 2f, thickness, length, ShaftFill, ShaftOutline);

                float tipY = axis.Y >= 0f ? center.Y + length / 2f - thickness : center.Y;
                Placeholder.Square(center.X - thickness, tipY, thickness * 2f, thickness * 1.5f, TipFill, ShaftOutline);
            }

            if (State == SpearState.Stuck)
            {
                Draw.Circle(center, thickness * 2f, ShaftOutline * 0.4f, 8);
            }
        }
        #endregion

        /// <summary>Small solid ledge left behind by a spear stuck in a wall.</summary>
        [Tracked]
        public class SpearLedge : Solid
        {
            public SpearLedge(Vector2 position, float width, float height)
                : base(position, width, height, safe: true)
            {
                SurfaceSoundIndex = SurfaceIndex.Girder;
                Depth = 99;
            }

            public override void Render()
            {
                // The spear itself draws the visual; the ledge only needs an edge hint.
                Draw.HollowRect(X, Y, Width, Height, ShaftOutline * 0.5f);
            }
        }
    }
}
