namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// NukeDrop - MyWorld bombardment. The spawner drops a bomb (optionally right above
    /// the player), the bomb falls until it hits ground, blinks through its fuse, then
    /// erupts into an expanding shockwave that kills on contact.
    ///
    /// <see cref="NukeDropPowerUp"/> is the same warhead pointed the other way: a pickup
    /// that lets the player call one down, harmless to them but lethal to enemies.
    ///
    /// Placeholder art: dark circle bomb with a blinking red circle, hollow circle rings
    /// for the blast.
    /// </summary>
    [CustomEntity("DZ/NukeDrop")]
    [Tracked]
    public class NukeDrop : Entity
    {
        #region Enums
        public enum DropMode
        {
            Once,
            Interval,
            Flag
        }
        #endregion

        #region Fields
        private static readonly Color MarkerColor = Calc.HexToColor("ff5a5a");

        private readonly DropMode mode;
        private readonly float interval;
        private readonly float startDelay;
        private readonly float fallSpeed;
        private readonly float fuseTime;
        private readonly float blastRadius;
        private readonly float spawnHeight;
        private readonly float shakeTime;
        private readonly bool targetsPlayer;
        private readonly bool deadly;
        private readonly int damage;
        private readonly bool bigBlast;
        private readonly string flag;

        private float timer;
        private bool firedOnce;
        private bool lastFlagState;
        private Level level;
        #endregion

        #region Constructor
        public NukeDrop(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            mode = data.Enum("mode", DropMode.Interval);
            interval = Math.Max(0.5f, data.Float("interval", 5f));
            startDelay = Math.Max(0f, data.Float("startDelay", 1.5f));
            fallSpeed = Math.Max(20f, data.Float("fallSpeed", 220f));
            fuseTime = Math.Max(0.1f, data.Float("fuseTime", 0.9f));
            blastRadius = Math.Max(8f, data.Float("blastRadius", 84f));
            spawnHeight = Math.Max(0f, data.Float("spawnHeight", 64f));
            shakeTime = Math.Max(0f, data.Float("shakeTime", 0.6f));
            targetsPlayer = data.Bool("targetsPlayer", true);
            deadly = data.Bool("deadly", true);
            damage = Math.Max(1, data.Int("damage", 3));
            bigBlast = data.Bool("bigBlast", false);
            flag = data.Attr("flag", "");

            timer = startDelay;
            Depth = -10000;
        }
        #endregion

        #region Public API
        /// <summary>Drops one warhead now.</summary>
        public void Drop()
        {
            Vector2 spawn = Position - new Vector2(0f, spawnHeight);

            if (targetsPlayer)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    spawn.X = player.Center.X;
                }
            }

            // NukeBomb plays its own drop cue as it spawns.
            Scene.Add(new NukeBomb(spawn, fallSpeed, fuseTime, blastRadius, shakeTime, deadly, damage,
                friendly: false, bigBlast: bigBlast));
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            lastFlagState = FlagSet();
        }

        private bool FlagSet()
        {
            return !string.IsNullOrEmpty(flag) && level != null && level.Session.GetFlag(flag);
        }

        public override void Update()
        {
            base.Update();

            if (mode == DropMode.Flag)
            {
                bool now = FlagSet();
                if (now && !lastFlagState)
                {
                    Drop();
                }

                lastFlagState = now;
                return;
            }

            if (mode == DropMode.Once && firedOnce)
            {
                return;
            }

            timer -= Engine.DeltaTime;
            if (timer > 0f)
            {
                return;
            }

            Drop();
            firedOnce = true;
            timer = interval;
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            Draw.HollowRect(X - 6f, Y - 6f, 12f, 12f, MarkerColor * 0.7f);
            Draw.Circle(Position, blastRadius, MarkerColor * 0.3f, 20);
            Draw.Line(Position, Position - new Vector2(0f, spawnHeight), MarkerColor * 0.4f);
        }
        #endregion
    }

    /// <summary>Falling warhead: drops, fuses, then spawns a <see cref="NukeShockwave"/>.</summary>
    [Tracked]
    public class NukeBomb : Entity
    {
        #region Fields
        private static readonly Color CasingFill = Calc.HexToColor("2b2b33");
        private static readonly Color CasingEdge = Calc.HexToColor("8c8c99");
        private static readonly Color BlinkColor = Calc.HexToColor("ff3b3b");

        private readonly float fallSpeed;
        private readonly float fuseTime;
        private readonly float blastRadius;
        private readonly float shakeTime;
        private readonly bool deadly;
        private readonly int damage;
        private readonly bool friendly;
        private readonly bool bigBlast;

        private bool landed;
        private float fuse;
        private float blink;
        private Level level;
        #endregion

        #region Constructor
        public NukeBomb(Vector2 position, float fallSpeed, float fuseTime, float blastRadius,
            float shakeTime, bool deadly, int damage, bool friendly, bool bigBlast = false)
            : base(position)
        {
            this.fallSpeed = fallSpeed;
            this.fuseTime = fuseTime;
            this.blastRadius = blastRadius;
            this.shakeTime = shakeTime;
            this.deadly = deadly;
            this.damage = damage;
            this.friendly = friendly;
            this.bigBlast = bigBlast;

            fuse = fuseTime;
            Collider = new Circle(5f);
            Depth = -8600;

            Add(new VertexLight(Color.OrangeRed, 0.5f, 12, 32));
        }
        #endregion

        #region Private
        private void Detonate()
        {
            Audio.Play(bigBlast ? ChapterSfx.MyWorldBigNukeExplosion : ChapterSfx.MyWorldNukeExplosion, Position);
            level?.Shake(shakeTime);
            level?.Flash(Color.White * 0.4f);

            Scene.Add(new NukeShockwave(Position, blastRadius, deadly && !friendly, damage));
            RemoveSelf();
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            Audio.Play(bigBlast ? ChapterSfx.MyWorldBigNukeDrop : ChapterSfx.MyWorldNukeDrop, Position);
        }

        public override void Update()
        {
            base.Update();

            blink += Engine.DeltaTime;

            if (!landed)
            {
                Position.Y += fallSpeed * Engine.DeltaTime;

                if (CollideCheck<Solid>() || (level != null && Y > level.Bounds.Bottom))
                {
                    landed = true;
                    Audio.Play("event:/game/general/fallblock_impact", Position);
                }

                return;
            }

            fuse -= Engine.DeltaTime;
            if (fuse <= 0f)
            {
                Detonate();
            }
        }

        public override void Render()
        {
            base.Render();

            Placeholder.Circle(Position, 5f, CasingFill, CasingEdge);

            // Blink faster as the fuse runs down.
            float rate = landed ? MathHelper.Lerp(4f, 18f, 1f - MathHelper.Clamp(fuse / fuseTime, 0f, 1f)) : 3f;
            if ((float)Math.Sin(blink * rate) > 0f)
            {
                Placeholder.FillCircle(Position + new Vector2(0f, -2f), 2f, BlinkColor);
            }

            if (!landed)
            {
                // Motion streak.
                Draw.Rect(Position.X - 1f, Position.Y - 10f, 2f, 8f, CasingEdge * 0.4f);
            }
            else
            {
                Draw.Circle(Position, blastRadius, BlinkColor * 0.15f, 20);
            }
        }
        #endregion
    }

    /// <summary>Expanding blast ring left by a detonated <see cref="NukeBomb"/>.</summary>
    [Tracked]
    public class NukeShockwave : Entity
    {
        #region Fields
        private static readonly Color RingInner = Calc.HexToColor("fff2c2");
        private static readonly Color RingOuter = Calc.HexToColor("ff6a2b");

        private readonly float maxRadius;
        private readonly bool deadly;
        private readonly int damage;
        private readonly List<Entity> alreadyHit = new List<Entity>();

        private float radius;
        private float life = 1f;
        #endregion

        #region Constructor
        public NukeShockwave(Vector2 position, float maxRadius, bool deadly, int damage)
            : base(position)
        {
            this.maxRadius = maxRadius;
            this.deadly = deadly;
            this.damage = damage;

            Collider = new Circle(1f);
            Depth = -8700;

            Add(new VertexLight(Color.Orange, 1f, (int)maxRadius / 2, (int)maxRadius));
        }
        #endregion

        #region Entity Overrides
        public override void Update()
        {
            base.Update();

            radius = Calc.Approach(radius, maxRadius, maxRadius * 2.5f * Engine.DeltaTime);
            ((Circle)Collider).Radius = Math.Max(1f, radius);

            life -= Engine.DeltaTime * 1.6f;

            foreach (Entity entity in Scene.Tracker.GetEntities<Enemy>())
            {
                Enemy enemy = (Enemy)entity;
                if (enemy.IsAlive && !alreadyHit.Contains(enemy) && CollideCheck(enemy))
                {
                    alreadyHit.Add(enemy);
                    enemy.TakeDamage(damage);
                }
            }

            if (deadly)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && !player.Dead && CollideCheck(player))
                {
                    player.Die((player.Center - Position).SafeNormalize());
                }
            }

            if (life <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();

            float alpha = MathHelper.Clamp(life, 0f, 1f);

            Placeholder.FillCircle(Position, radius * 0.35f, RingInner * (alpha * 0.6f));
            Draw.Circle(Position, radius, RingOuter * alpha, 24);
            Draw.Circle(Position, radius * 0.7f, RingInner * (alpha * 0.8f), 20);
            Draw.Circle(Position, radius * 1.1f, RingOuter * (alpha * 0.4f), 24);
        }
        #endregion
    }

    /// <summary>
    /// NukeDropPowerUp - pickup that arms the player with call-down nukes. The nuke it
    /// drops is <c>friendly</c>: it clears enemies without killing the player, so this is
    /// an offensive ability rather than a hazard.
    /// </summary>
    [CustomEntity("DZ/NukeDropPowerUp")]
    [Tracked]
    public class NukeDropPowerUp : Entity
    {
        #region Fields
        private static readonly Color PickupFill = Calc.HexToColor("d64f4f");
        private static readonly Color PickupEdge = Calc.HexToColor("ffe0e0");

        private readonly int charges;
        private readonly float respawnDelay;
        private readonly bool oneUse;
        private readonly float radius;
        private readonly float dropHeight;
        private readonly float fallSpeed;
        private readonly float fuseTime;
        private readonly float blastRadius;
        private readonly int damage;
        private readonly bool bigBlast;
        private readonly AbilityButton button;

        private bool collected;
        private float respawnTimer;
        private float bob;
        #endregion

        #region Constructor
        public NukeDropPowerUp(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            charges = Math.Max(1, data.Int("charges", 2));
            respawnDelay = Math.Max(0f, data.Float("respawnDelay", 4f));
            oneUse = data.Bool("oneUse", false);
            radius = Math.Max(4f, data.Float("radius", 8f));
            dropHeight = Math.Max(8f, data.Float("dropHeight", 72f));
            fallSpeed = Math.Max(20f, data.Float("fallSpeed", 260f));
            fuseTime = Math.Max(0.05f, data.Float("fuseTime", 0.4f));
            blastRadius = Math.Max(8f, data.Float("blastRadius", 72f));
            damage = Math.Max(1, data.Int("damage", 4));
            bigBlast = data.Bool("bigBlast", false);
            button = data.Enum("useButton", AbilityButton.Grab);

            Collider = new Circle(radius);
            Depth = -100;

            Add(new VertexLight(Color.OrangeRed, 0.6f, 16, 40));
        }
        #endregion

        #region Entity Overrides
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
            NukeStrikeController.Ensure(Scene, dropHeight, fallSpeed, fuseTime, blastRadius, damage, bigBlast, button)
                ?.AddCharges(charges);

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

            // Down arrow inside.
            Draw.Rect(center.X - 0.5f, center.Y - radius * 0.5f, 1f, radius, PickupEdge);
            Draw.Rect(center.X - 2.5f, center.Y + radius * 0.4f, 5f, 1f, PickupEdge);
        }
        #endregion
    }

    /// <summary>Holds the player's nuke charges and calls them down on the configured button.</summary>
    [Tracked]
    public class NukeStrikeController : Entity
    {
        #region Fields
        private float dropHeight;
        private float fallSpeed;
        private float fuseTime;
        private float blastRadius;
        private int damage;
        private bool bigBlast;
        private AbilityButton button;
        private int charges;
        private float cooldown;
        #endregion

        #region Constructor
        public NukeStrikeController(float dropHeight, float fallSpeed, float fuseTime, float blastRadius,
            int damage, bool bigBlast, AbilityButton button)
        {
            this.dropHeight = dropHeight;
            this.fallSpeed = fallSpeed;
            this.fuseTime = fuseTime;
            this.blastRadius = blastRadius;
            this.damage = damage;
            this.bigBlast = bigBlast;
            this.button = button;

            Tag = Tags.Global;
            Depth = -10000;
        }
        #endregion

        #region Public API
        public static NukeStrikeController Ensure(Scene scene, float dropHeight, float fallSpeed,
            float fuseTime, float blastRadius, int damage, bool bigBlast, AbilityButton button)
        {
            if (scene == null)
            {
                return null;
            }

            NukeStrikeController controller = scene.Tracker.GetEntity<NukeStrikeController>();
            if (controller != null)
            {
                controller.dropHeight = dropHeight;
                controller.fallSpeed = fallSpeed;
                controller.fuseTime = fuseTime;
                controller.blastRadius = blastRadius;
                controller.damage = damage;
                controller.bigBlast = bigBlast;
                controller.button = button;
                return controller;
            }

            controller = new NukeStrikeController(dropHeight, fallSpeed, fuseTime, blastRadius, damage, bigBlast, button);
            scene.Add(controller);
            return controller;
        }

        public int Charges => charges;

        public void AddCharges(int count)
        {
            charges += Math.Max(0, count);
        }

        public void CallStrike(Player player)
        {
            if (player == null || charges <= 0)
            {
                return;
            }

            charges--;
            cooldown = 0.4f;

            Vector2 spawn = new Vector2(player.Center.X, player.Center.Y - dropHeight);
            Scene.Add(new NukeBomb(spawn, fallSpeed, fuseTime, blastRadius, 0.4f, deadly: false, damage,
                friendly: true, bigBlast: bigBlast));
        }
        #endregion

        #region Private
        private bool ButtonPressed(Player player)
        {
            switch (button)
            {
                case AbilityButton.Dash:
                    return Input.Dash.Pressed;
                case AbilityButton.Jump:
                    return Input.Jump.Pressed;
                case AbilityButton.Talk:
                    return Input.Talk.Pressed;
                default:
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

            if (charges <= 0)
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
                CallStrike(player);
            }
        }
        #endregion
    }
}
