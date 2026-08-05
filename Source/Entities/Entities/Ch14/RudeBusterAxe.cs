namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// RudeBusterAxe - Susie's axe, as a player power-up rather than a hazard.
    ///
    /// Three pieces:
    ///   <see cref="RudeBusterAxe"/>          the placeable pickup that grants charges.
    ///   <see cref="RudeBusterAxeController"/> global controller: holds the charge count
    ///                                        and watches for the throw button.
    ///   <see cref="RudeBusterAxeProjectile"/> the spinning axe itself.
    ///
    /// Charges live on the session counter <c>dz_rude_buster_axes</c>, so the ability
    /// survives room transitions and can be handed out by a cutscene as easily as by a
    /// pickup - that is what makes this an entity *and* a power-up.
    ///
    /// Placeholder art: purple circle pickup, spinning square outline projectile.
    /// </summary>
    [CustomEntity("DZ/RudeBusterAxe")]
    [Tracked]
    public class RudeBusterAxe : Entity
    {
        #region Constants
        public const string ChargeCounter = "dz_rude_buster_axes";
        #endregion

        #region Fields
        private static readonly Color PickupFill = Calc.HexToColor("8f4fd6");
        private static readonly Color PickupOutline = Calc.HexToColor("f0d9ff");
        private static readonly Color BladeColor = Calc.HexToColor("ffe066");

        private readonly int charges;
        private readonly float respawnDelay;
        private readonly bool oneUse;
        private readonly string flag;
        private readonly float radius;

        private readonly RudeBusterAxeSettings settings;

        private bool collected;
        private float respawnTimer;
        private float bob;
        private Level level;
        #endregion

        #region Constructor
        public RudeBusterAxe(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            charges = Math.Max(1, data.Int("charges", 3));
            respawnDelay = Math.Max(0f, data.Float("respawnDelay", 3f));
            oneUse = data.Bool("oneUse", false);
            flag = data.Attr("flag", "");
            radius = Math.Max(4f, data.Float("radius", 8f));

            settings = new RudeBusterAxeSettings
            {
                ThrowSpeed = Math.Max(40f, data.Float("throwSpeed", 280f)),
                Range = Math.Max(16f, data.Float("range", 120f)),
                Damage = Math.Max(1, data.Int("damage", 2)),
                Boomerang = data.Bool("boomerang", true),
                RefundOnCatch = data.Bool("refundOnCatch", false),
                SpinSpeed = data.Float("spinSpeed", 14f),
                Size = Math.Max(4f, data.Float("axeSize", 10f)),
                Button = data.Enum("useButton", AbilityButton.Grab)
            };

            Collider = new Circle(radius);
            Depth = -100;

            Add(new VertexLight(Color.MediumPurple, 0.6f, 16, 40));
        }
        #endregion

        #region Public API
        /// <summary>Grants axe charges to the player without needing a pickup in the room.</summary>
        public static void Grant(Scene scene, int count, RudeBusterAxeSettings settings)
        {
            RudeBusterAxeController controller = RudeBusterAxeController.Ensure(scene, settings);
            controller?.AddCharges(count);
        }
        #endregion

        #region Private
        private void Collect(Player player)
        {
            collected = true;
            respawnTimer = respawnDelay;

            Audio.Play(ChapterSfx.NexusSwing, Position);
            level?.Flash(PickupFill * 0.2f);

            Grant(Scene, charges, settings);

            if (!string.IsNullOrEmpty(flag))
            {
                level?.Session.SetFlag(flag, true);
            }

            if (oneUse || respawnDelay <= 0f)
            {
                RemoveSelf();
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

            bob += Engine.DeltaTime * 3f;

            if (collected)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    collected = false;
                    Audio.Play(ChapterSfx.NexusSwing, Position);
                }

                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                Collect(player);
            }
        }

        public override void Render()
        {
            base.Render();

            if (collected)
            {
                Draw.Circle(Position, radius, PickupOutline * 0.2f, 10);
                return;
            }

            Vector2 center = Position + new Vector2(0f, (float)Math.Sin(bob) * 2f);

            Draw.Circle(center, radius + 3f, PickupFill * 0.3f, 12);
            Placeholder.Circle(center, radius, PickupFill, PickupOutline);

            // Axe head hint inside the pickup.
            Placeholder.RotatedSquare(center, radius * 0.9f, bob * 0.6f, BladeColor);
        }
        #endregion
    }

    /// <summary>Tuning shared between the pickup, the controller and the projectile.</summary>
    public class RudeBusterAxeSettings
    {
        public float ThrowSpeed = 280f;
        public float Range = 120f;
        public int Damage = 2;
        public bool Boomerang = true;
        public bool RefundOnCatch;
        public float SpinSpeed = 14f;
        public float Size = 10f;
        public AbilityButton Button = AbilityButton.Grab;
    }

    /// <summary>
    /// Holds the player's axe charges and turns button presses into thrown axes.
    /// Tagged global so the ability follows the player between rooms.
    /// </summary>
    [Tracked]
    public class RudeBusterAxeController : Entity
    {
        #region Fields
        private RudeBusterAxeSettings settings;
        private float cooldown;
        #endregion

        #region Constructor
        public RudeBusterAxeController(RudeBusterAxeSettings settings)
        {
            this.settings = settings ?? new RudeBusterAxeSettings();
            Tag = Tags.Global;
            Depth = -10000;
        }
        #endregion

        #region Public API
        /// <summary>Finds the live controller, creating one if the scene has none yet.</summary>
        public static RudeBusterAxeController Ensure(Scene scene, RudeBusterAxeSettings settings)
        {
            if (scene == null)
            {
                return null;
            }

            RudeBusterAxeController controller = scene.Tracker.GetEntity<RudeBusterAxeController>();
            if (controller != null)
            {
                // Latest pickup wins, so a room can retune the ability.
                controller.settings = settings ?? controller.settings;
                return controller;
            }

            controller = new RudeBusterAxeController(settings);
            scene.Add(controller);
            return controller;
        }

        public int Charges
        {
            get
            {
                Level level = Scene as Level;
                return level?.Session.GetCounter(RudeBusterAxe.ChargeCounter) ?? 0;
            }
            private set
            {
                Level level = Scene as Level;
                level?.Session.SetCounter(RudeBusterAxe.ChargeCounter, Math.Max(0, value));
            }
        }

        public void AddCharges(int count)
        {
            Charges += Math.Max(0, count);
        }

        public void Throw(Player player)
        {
            if (player == null || Charges <= 0)
            {
                return;
            }

            Charges -= 1;
            cooldown = 0.2f;

            Vector2 direction = new Vector2(player.Facing == Facings.Left ? -1f : 1f, 0f);

            // Aiming up or down overrides the facing, so the axe covers all four ways.
            if (Input.MoveY.Value != 0 && Input.MoveX.Value == 0)
            {
                direction = new Vector2(0f, Input.MoveY.Value);
            }

            Scene.Add(new RudeBusterAxeProjectile(player.Center, direction, settings, this));
            Audio.Play(ChapterSfx.NexusSwing, player.Center);
        }

        /// <summary>Called by a returning axe that the player caught.</summary>
        public void Refund()
        {
            if (settings.RefundOnCatch)
            {
                Charges += 1;
                Audio.Play(ChapterSfx.NexusHit);
            }
        }
        #endregion

        #region Private
        private bool ButtonPressed(Player player)
        {
            switch (settings.Button)
            {
                case AbilityButton.Dash:
                    return Input.Dash.Pressed;
                case AbilityButton.Jump:
                    return Input.Jump.Pressed;
                case AbilityButton.Talk:
                    return Input.Talk.Pressed;
                default:
                    // Grab is also climb, so ignore presses while the player is on a wall.
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
                return;
            }

            if (Charges <= 0)
            {
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead || player.StateMachine.State != Player.StNormal)
            {
                return;
            }

            if (ButtonPressed(player))
            {
                Throw(player);
            }
        }
        #endregion
    }

    /// <summary>Spinning axe thrown by the Rude Buster power-up.</summary>
    [Tracked]
    public class RudeBusterAxeProjectile : Entity
    {
        #region Fields
        private static readonly Color BladeColor = Calc.HexToColor("ffe066");
        private static readonly Color TrailColor = Calc.HexToColor("8f4fd6");

        private readonly RudeBusterAxeSettings settings;
        private readonly RudeBusterAxeController owner;
        private readonly Vector2 origin;
        private readonly List<Entity> alreadyHit = new List<Entity>();

        private Vector2 direction;
        private bool returning;
        private float spin;
        private Level level;
        #endregion

        #region Constructor
        public RudeBusterAxeProjectile(Vector2 position, Vector2 direction, RudeBusterAxeSettings settings, RudeBusterAxeController owner)
            : base(position)
        {
            this.settings = settings ?? new RudeBusterAxeSettings();
            this.owner = owner;
            this.direction = direction.SafeNormalize(Vector2.UnitX);

            origin = position;
            Collider = new Circle(this.settings.Size * 0.6f);
            Depth = -8600;

            Add(new VertexLight(Color.Gold, 0.6f, 12, 28));
        }
        #endregion

        #region Private
        private void HitEnemies()
        {
            foreach (Entity entity in Scene.Tracker.GetEntities<Enemy>())
            {
                Enemy enemy = (Enemy)entity;
                if (!enemy.IsAlive || alreadyHit.Contains(enemy) || !CollideCheck(enemy))
                {
                    continue;
                }

                alreadyHit.Add(enemy);
                enemy.TakeDamage(settings.Damage);
                Audio.Play(ChapterSfx.NexusHit, Position);
            }
        }

        private bool HitBreakablePillar(Vector2 hitDirection)
        {
            foreach (Entity entity in Scene.Tracker.GetEntities<global::Celeste.Entities.Chapters.Ch10.BreakablePillar>())
            {
                global::Celeste.Entities.Chapters.Ch10.BreakablePillar pillar = (global::Celeste.Entities.Chapters.Ch10.BreakablePillar)entity;
                if (pillar.IsBroken || !CollideCheck(pillar))
                {
                    continue;
                }

                pillar.Break(hitDirection);
                Audio.Play(ChapterSfx.NexusHit, Position);
                return true;
            }

            return false;
        }

        private void TurnBack()
        {
            if (!settings.Boomerang)
            {
                Dissipate();
                return;
            }

            returning = true;
            alreadyHit.Clear();
        }

        private void Dissipate()
        {
            Audio.Play(ChapterSfx.NexusHit, Position);
            RemoveSelf();
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

            spin += settings.SpinSpeed * Engine.DeltaTime;

            Player player = Scene.Tracker.GetEntity<Player>();

            if (returning)
            {
                if (player == null || player.Dead)
                {
                    Dissipate();
                    return;
                }

                Vector2 toPlayer = (player.Center - Position).SafeNormalize();
                Position += toPlayer * settings.ThrowSpeed * Engine.DeltaTime;

                if (Vector2.Distance(Position, player.Center) < 10f)
                {
                    owner?.Refund();
                    RemoveSelf();
                    return;
                }
            }
            else
            {
                Position += direction * settings.ThrowSpeed * Engine.DeltaTime;

                if (Vector2.Distance(Position, origin) >= settings.Range)
                {
                    TurnBack();
                }
                else if (HitBreakablePillar(direction))
                {
                    TurnBack();
                }
                else if (CollideCheck<Solid>())
                {
                    TurnBack();
                }
                else if (level != null && !level.Bounds.Contains((int)X, (int)Y))
                {
                    Dissipate();
                    return;
                }
            }

            HitEnemies();
        }

        public override void Render()
        {
            base.Render();

            Placeholder.FillCircle(Position, settings.Size * 0.25f, TrailColor);
            Placeholder.RotatedSquare(Position, settings.Size, spin, BladeColor);
            Placeholder.RotatedSquare(Position, settings.Size * 0.6f, -spin * 0.7f, BladeColor * 0.6f);
        }
        #endregion
    }
}
