using System;
using MaggyHelper.Entities.Bosses;
using MaggyHelper.Entities.Projectiles;
using MaggyHelper.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities
{
    public abstract class ElsKnightCloneBoss : BossActor, IKirbyCopySource
    {
        private readonly ElsKnightCloneCombatProfile combatProfile;
        private readonly ElsKnightCloneVisualProfile visualProfile;
        private readonly ElsKnightCloneAttackProfile[] attackPattern;
        private readonly ElsKnightCloneVariantAttribute metadata;
        private readonly bool playMusicOnStart;
        private readonly string musicEvent;
        private VertexLight auraLight;
        private SineWave auraPulse;
        private Wiggler hitWiggler;
        private Level level;
        private global::Celeste.Player player;
        private float stateTimer;
        private float cooldownTimer;
        private float invulnerabilityTimer;
        private Vector2 dashVelocity;
        private float hoverAngle;
        private int attackIndex;
        private bool fightStarted;
        private bool changedMusic;
        private string previousMusic;
        private bool hasAnimatedSprite;

        protected ElsKnightCloneState CloneState = ElsKnightCloneState.Dormant;
        protected ElsKnightCloneAttackProfile CurrentAttack;

        protected abstract CopyAbilityType CloneAbility { get; }

        protected virtual string SpriteBankEntryName => null;

        public ElsKnightCloneKind Kind => metadata.Kind;

        protected ElsKnightCloneBoss(
            EntityData data,
            Vector2 offset,
            ElsKnightCloneCombatProfile defaultCombatProfile,
            ElsKnightCloneVisualProfile defaultVisualProfile,
            ElsKnightCloneAttackProfile[] attackPattern)
            : this(
                data.Position + offset,
                new ElsKnightCloneCombatProfile(
                    data.Int("health", defaultCombatProfile.MaxHealth),
                    data.Float("moveSpeed", defaultCombatProfile.MoveSpeed),
                    data.Float("attackCooldown", defaultCombatProfile.AttackCooldown),
                    data.Float("orbitRadius", defaultCombatProfile.OrbitRadius),
                    data.Float("dashSpeed", defaultCombatProfile.DashSpeed)),
                new ElsKnightCloneVisualProfile(
                    defaultVisualProfile.PrimaryColor,
                    defaultVisualProfile.SecondaryColor,
                    defaultVisualProfile.AccentColor,
                    data.Attr("spritePath", defaultVisualProfile.SpritePath),
                    data.Attr("idleAnimationPath", defaultVisualProfile.IdleAnimationPath),
                    data.Attr("moveAnimationPath", defaultVisualProfile.MoveAnimationPath),
                    data.Attr("chargeAnimationPath", defaultVisualProfile.ChargeAnimationPath),
                    data.Attr("slashAnimationPath", defaultVisualProfile.SlashAnimationPath),
                    data.Attr("warpAnimationPath", defaultVisualProfile.WarpAnimationPath)),
                attackPattern,
                data.Bool("playMusicOnStart", false),
                data.Attr("bossMusic", string.Empty))
        {
        }

        protected ElsKnightCloneBoss(
            Vector2 position,
            ElsKnightCloneCombatProfile combatProfile,
            ElsKnightCloneVisualProfile visualProfile,
            ElsKnightCloneAttackProfile[] attackPattern,
            bool playMusicOnStart = false,
            string bossMusic = null)
            : base(position, string.Empty, Vector2.One, 0f, true, false, 0f, new Hitbox(28f, 36f, -14f, -36f))
        {
            this.combatProfile = combatProfile;
            this.visualProfile = visualProfile;
            this.attackPattern = attackPattern ?? Array.Empty<ElsKnightCloneAttackProfile>();
            this.playMusicOnStart = playMusicOnStart;
            metadata = ResolveMetadata(GetType());
            musicEvent = !string.IsNullOrWhiteSpace(bossMusic) ? bossMusic : metadata.DefaultMusicEvent;

            MaxHealth = combatProfile.MaxHealth;
            Health = MaxHealth;
            Depth = -8600;

            Add(new PlayerCollider(OnPlayer));
            Add(auraLight = new VertexLight(visualProfile.PrimaryColor, 1f, 24, 72));
            Add(auraPulse = new SineWave(0.65f, 0f));
            auraPulse.Randomize();
            Add(hitWiggler = Wiggler.Create(0.35f, 2f));

            CreateSpriteIfAvailable(visualProfile.SpritePath);
        }

        public CopyAbilityType GetCopyAbility()
        {
            return CloneAbility;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Removed(Scene scene)
        {
            RestoreMusic();
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();

            level = level ?? SceneAs<Level>();
            player = Scene?.Tracker.GetEntity<global::Celeste.Player>();

            stateTimer += Engine.DeltaTime;

            if (cooldownTimer > 0f)
                cooldownTimer -= Engine.DeltaTime;

            if (invulnerabilityTimer > 0f)
                invulnerabilityTimer -= Engine.DeltaTime;

            UpdateVisuals();

            switch (CloneState)
            {
                case ElsKnightCloneState.Dormant:
                    if (ShouldStartFight())
                        StartFight();
                    break;

                case ElsKnightCloneState.Intro:
                    UpdateIntro();
                    break;

                case ElsKnightCloneState.Hovering:
                    UpdateHovering();
                    break;

                case ElsKnightCloneState.Telegraphing:
                    UpdateTelegraphing();
                    break;

                case ElsKnightCloneState.ExecutingAttack:
                    UpdateExecutingAttack();
                    break;

                case ElsKnightCloneState.Recovering:
                    UpdateRecovering();
                    break;

                case ElsKnightCloneState.Defeated:
                    UpdateDefeated();
                    break;
            }
        }

        public override void Render()
        {
            base.Render();

            if (!hasAnimatedSprite)
                RenderFallbackSilhouette();

            if (fightStarted && CloneState != ElsKnightCloneState.Defeated)
                RenderHealthBar();
        }

        public override void TakeDamage(int amount)
        {
            if (amount <= 0 || CloneState == ElsKnightCloneState.Defeated || invulnerabilityTimer > 0f)
                return;

            Health = Math.Max(0, Health - amount);
            invulnerabilityTimer = 0.35f;
            hitWiggler.Start();

            level?.Displacement.AddBurst(Position, 0.35f, 48f, 96f, 0.25f);
            level?.Flash(visualProfile.SecondaryColor * 0.2f, false);
            Audio.Play("event:/game/general/thing_booped", Position);

            if (Health <= 0)
            {
                IsDefeated = true;
                Collidable = false;
                dashVelocity = Vector2.Zero;
                SetCloneState(ElsKnightCloneState.Defeated);
                RestoreMusic();
                return;
            }

            dashVelocity = Vector2.Zero;
            cooldownTimer = Math.Max(cooldownTimer, combatProfile.AttackCooldown * 0.5f);
            SetCloneState(ElsKnightCloneState.Recovering);
        }

        private static ElsKnightCloneVariantAttribute ResolveMetadata(Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(ElsKnightCloneVariantAttribute)) as ElsKnightCloneVariantAttribute
                ?? new ElsKnightCloneVariantAttribute(ElsKnightCloneKind.Galacta, type.Name, string.Empty);
        }

        private void StartFight()
        {
            fightStarted = true;
            cooldownTimer = 0.4f;

            if (playMusicOnStart && !string.IsNullOrWhiteSpace(musicEvent))
            {
                previousMusic = Audio.CurrentMusic;
                Audio.SetMusic(musicEvent);
                changedMusic = true;
            }

            Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
            level?.Displacement.AddBurst(Position, 0.55f, 72f, 144f, 0.45f);
            SetCloneState(ElsKnightCloneState.Intro);
        }

        private bool ShouldStartFight()
        {
            return player != null && !player.Dead && Vector2.Distance(Center, player.Center) <= combatProfile.OrbitRadius + 120f;
        }

        private void SetCloneState(ElsKnightCloneState newState)
        {
            CloneState = newState;
            stateTimer = 0f;

            switch (newState)
            {
                case ElsKnightCloneState.Intro:
                case ElsKnightCloneState.Telegraphing:
                    TryPlay("charge", "idle");
                    break;

                case ElsKnightCloneState.Hovering:
                case ElsKnightCloneState.Recovering:
                    TryPlay("move", "idle");
                    break;

                case ElsKnightCloneState.ExecutingAttack:
                    TryPlay(CurrentAttack.Attack == ElsKnightCloneAttack.WarpStrike ? "warp" : "slash", "move");
                    break;

                case ElsKnightCloneState.Defeated:
                    TryPlay("idle", "move");
                    break;
            }
        }

        private void UpdateIntro()
        {
            DoHoverMovement(0.65f);

            if (stateTimer >= 0.45f)
                SetCloneState(ElsKnightCloneState.Hovering);
        }

        private void UpdateHovering()
        {
            DoHoverMovement(1f);

            if (player != null && cooldownTimer <= 0f && attackPattern.Length > 0)
                BeginAttack();
        }

        private void UpdateTelegraphing()
        {
            DoHoverMovement(0.4f);

            if (stateTimer >= CurrentAttack.Windup)
                ExecuteCurrentAttack();
        }

        private void UpdateExecutingAttack()
        {
            Position += dashVelocity * Engine.DeltaTime;
            dashVelocity *= 0.96f;

            if (Scene.OnInterval(0.04f))
                EmitDashTrail();

            if (player != null && !player.Dead && Vector2.Distance(Center, player.Center) < 18f)
                player.Die((player.Center - Center).SafeNormalize());

            if (stateTimer >= CurrentAttack.ActiveTime)
            {
                dashVelocity = Vector2.Zero;
                BeginRecovery(CurrentAttack.Cooldown);
            }
        }

        private void UpdateRecovering()
        {
            DoHoverMovement(0.7f);

            if (stateTimer >= 0.35f)
                SetCloneState(ElsKnightCloneState.Hovering);
        }

        private void UpdateDefeated()
        {
            dashVelocity = Vector2.Zero;

            if (Sprite != null)
                Sprite.Color *= 0.92f;

            auraLight.Alpha = Math.Max(0f, auraLight.Alpha - Engine.DeltaTime * 2f);

            if (stateTimer >= 0.75f)
                RemoveSelf();
        }

        private void BeginAttack()
        {
            CurrentAttack = attackPattern[attackIndex % attackPattern.Length];
            attackIndex++;
            level?.Displacement.AddBurst(Position, 0.25f, 48f, 96f, 0.2f);
            SetCloneState(ElsKnightCloneState.Telegraphing);
        }

        private void ExecuteCurrentAttack()
        {
            switch (CurrentAttack.Attack)
            {
                case ElsKnightCloneAttack.DashSlash:
                    Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                    dashVelocity = AimAtPlayer(combatProfile.DashSpeed);
                    SetCloneState(ElsKnightCloneState.ExecutingAttack);
                    break;

                case ElsKnightCloneAttack.CrescentVolley:
                    Audio.Play("event:/game/general/thing_booped", Position);
                    FireCrescentVolley();
                    BeginRecovery(CurrentAttack.Cooldown);
                    break;

                case ElsKnightCloneAttack.RadialBurst:
                    Audio.Play("event:/game/general/thing_booped", Position);
                    FireRadialBurst();
                    BeginRecovery(CurrentAttack.Cooldown);
                    break;

                case ElsKnightCloneAttack.WarpStrike:
                    Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                    PerformWarpStrike();
                    SetCloneState(ElsKnightCloneState.ExecutingAttack);
                    break;
            }
        }

        private void BeginRecovery(float cooldown)
        {
            cooldownTimer = Math.Max(cooldown, combatProfile.AttackCooldown);
            dashVelocity = Vector2.Zero;
            SetCloneState(ElsKnightCloneState.Recovering);
        }

        private void DoHoverMovement(float speedMultiplier)
        {
            if (player == null)
                return;

            hoverAngle += Engine.DeltaTime * 1.8f;
            Vector2 target = player.Center + new Vector2(
                (float)Math.Cos(hoverAngle) * combatProfile.OrbitRadius,
                -28f + (float)Math.Sin(hoverAngle * 2f) * 18f);

            Position = Calc.Approach(Position, target, combatProfile.MoveSpeed * speedMultiplier * Engine.DeltaTime);

            if (Sprite != null)
            {
                float sign = player.X < X ? -1f : 1f;
                Sprite.Scale = new Vector2(sign, 1f + hitWiggler.Value * 0.04f);
            }
        }

        private void UpdateVisuals()
        {
            float colorLerp = (auraPulse.Value + 1f) * 0.5f;
            auraLight.Color = Color.Lerp(visualProfile.PrimaryColor, visualProfile.AccentColor, colorLerp);
            auraLight.Alpha = 0.65f + colorLerp * 0.2f + hitWiggler.Value * 0.25f;
            auraLight.StartRadius = 24f + hitWiggler.Value * 12f;
            auraLight.EndRadius = 72f + hitWiggler.Value * 24f;

            if (Sprite != null)
            {
                Sprite.Color = invulnerabilityTimer > 0f && Scene.OnInterval(0.06f)
                    ? visualProfile.SecondaryColor
                    : Color.White;
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (CloneState == ElsKnightCloneState.Defeated || player == null || player.Dead)
                return;

            if (player.StateMachine.State == global::Celeste.Player.StDash)
            {
                TakeDamage(1);
                return;
            }

            if (!fightStarted)
                StartFight();

            player.Die((player.Center - Center).SafeNormalize());
        }

        private void FireCrescentVolley()
        {
            if (Scene == null || player == null)
                return;

            int projectileCount = Math.Max(CurrentAttack.ProjectileCount, Kind == ElsKnightCloneKind.Galacta ? 3 : 5);
            float projectileSpeed = CurrentAttack.ProjectileSpeed > 0f
                ? CurrentAttack.ProjectileSpeed
                : (Kind == ElsKnightCloneKind.Galacta ? 260f : 230f);
            float spread = MathHelper.ToRadians(Kind == ElsKnightCloneKind.Galacta ? 18f : 28f);
            Vector2 baseDirection = AimAtPlayer(1f);
            Color projectileColor = Kind == ElsKnightCloneKind.Galacta
                ? Color.Lerp(visualProfile.PrimaryColor, visualProfile.AccentColor, 0.35f)
                : Color.Lerp(visualProfile.AccentColor, visualProfile.SecondaryColor, 0.3f);

            for (int i = 0; i < projectileCount; i++)
            {
                float offset = projectileCount == 1
                    ? 0f
                    : MathHelper.Lerp(-spread, spread, i / (float)(projectileCount - 1));
                Vector2 velocity = Rotate(baseDirection, offset) * projectileSpeed;
                Scene.Add(new SiamoZeroCrescentProjectile(Center, velocity, projectileColor));
            }

            level?.Displacement.AddBurst(Position, 0.45f, 64f, 128f, 0.4f);
        }

        private void FireRadialBurst()
        {
            if (Scene == null)
                return;

            int projectileCount = Math.Max(CurrentAttack.ProjectileCount, Kind == ElsKnightCloneKind.Galacta ? 8 : 10);
            float projectileSpeed = CurrentAttack.ProjectileSpeed > 0f
                ? CurrentAttack.ProjectileSpeed
                : (Kind == ElsKnightCloneKind.Galacta ? 240f : 210f);

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = (i / (float)projectileCount) * MathHelper.TwoPi;
                Vector2 direction = Calc.AngleToVector(angle, projectileSpeed);

                if (Kind == ElsKnightCloneKind.Galacta)
                    Scene.Add(new SwordBeam(Center, direction));
                else
                    Scene.Add(new SiamoZeroEnergyBlade(Center, direction, visualProfile.AccentColor, 0.9f));
            }

            level?.Shake(0.3f);
            level?.Displacement.AddBurst(Position, 0.65f, 96f, 192f, 0.6f);
        }

        private void PerformWarpStrike()
        {
            if (player != null)
            {
                Vector2 flankOffset = new Vector2(player.Facing == Facings.Left ? 52f : -52f, -16f);
                Position = player.Center + flankOffset;
            }

            dashVelocity = AimAtPlayer(combatProfile.DashSpeed * 0.85f);
            level?.Displacement.AddBurst(Position, 0.5f, 64f, 128f, 0.45f);
        }

        private void EmitDashTrail()
        {
            if (level == null || dashVelocity == Vector2.Zero)
                return;

            ParticleType particle = Kind == ElsKnightCloneKind.Galacta
                ? global::Celeste.Player.P_DashB
                : global::Celeste.Player.P_DashA;

            level.ParticlesFG.Emit(particle, 2, Center, Vector2.One * 3f, Calc.Angle(dashVelocity));
        }

        private Vector2 AimAtPlayer(float speed)
        {
            if (player == null)
                return Vector2.UnitX * speed;

            Vector2 direction = (player.Center - Center).SafeNormalize();
            return direction == Vector2.Zero ? Vector2.UnitX * speed : direction * speed;
        }

        private void RestoreMusic()
        {
            if (!changedMusic)
                return;

            changedMusic = false;

            if (!string.IsNullOrWhiteSpace(previousMusic))
                Audio.SetMusic(previousMusic);
        }

        private void RenderHealthBar()
        {
            Vector2 barPosition = Position + new Vector2(-18f, -48f);
            Draw.Rect(barPosition, 36f, 4f, Color.Black * 0.7f);
            Draw.Rect(barPosition, 36f * ((float)Health / Math.Max(1, MaxHealth)), 4f, visualProfile.AccentColor * 0.9f);
        }

        private void RenderFallbackSilhouette()
        {
            float fade = CloneState == ElsKnightCloneState.Defeated ? Math.Max(0f, 1f - stateTimer * 1.4f) : 1f;
            Vector2 anchor = Position + new Vector2(0f, -18f);
            Color mainColor = Color.Lerp(visualProfile.PrimaryColor, visualProfile.SecondaryColor, (auraPulse.Value + 1f) * 0.5f) * fade;
            Color accentColor = visualProfile.AccentColor * (0.7f * fade);

            Vector2 top = anchor + new Vector2(0f, -18f);
            Vector2 left = anchor + new Vector2(-12f, 0f);
            Vector2 right = anchor + new Vector2(12f, 0f);
            Vector2 bottom = anchor + new Vector2(0f, 18f);

            Draw.Line(top, left, mainColor);
            Draw.Line(left, bottom, mainColor);
            Draw.Line(bottom, right, mainColor);
            Draw.Line(right, top, mainColor);

            Draw.Line(anchor + new Vector2(-18f, -6f), anchor + new Vector2(18f, 6f), accentColor);
            Draw.Line(anchor + new Vector2(-6f, -18f), anchor + new Vector2(6f, 18f), Color.White * (0.45f * fade));

            if (Kind == ElsKnightCloneKind.Morpho)
            {
                Draw.Line(anchor + new Vector2(-22f, -8f), anchor + new Vector2(-8f, -22f), accentColor);
                Draw.Line(anchor + new Vector2(22f, -8f), anchor + new Vector2(8f, -22f), accentColor);
            }
            else
            {
                Draw.Line(anchor + new Vector2(-24f, 0f), anchor + new Vector2(24f, 0f), accentColor);
                Draw.Line(anchor + new Vector2(0f, -24f), anchor + new Vector2(0f, 24f), accentColor);
            }
        }

        private void CreateSpriteIfAvailable(string spritePath)
        {
            if (TryCreateSpriteFromSpriteBank())
                return;

            if (string.IsNullOrWhiteSpace(spritePath))
                return;

            string normalizedRoot = spritePath.EndsWith("/", StringComparison.Ordinal) ? spritePath : spritePath + "/";
            if (!GFX.Game.HasAtlasSubtextures(normalizedRoot + "idle")
                && !GFX.Game.HasAtlasSubtextures(normalizedRoot + "move")
                && !GFX.Game.HasAtlasSubtextures(normalizedRoot + "charge"))
            {
                return;
            }

            Sprite sprite = new Sprite(GFX.Game, normalizedRoot);
            bool addedAny = false;
            addedAny |= TryAddLoop(sprite, normalizedRoot, "idle", visualProfile.IdleAnimationPath, 0.1f);
            addedAny |= TryAddLoop(sprite, normalizedRoot, "move", visualProfile.MoveAnimationPath, 0.08f);
            addedAny |= TryAddLoop(sprite, normalizedRoot, "charge", visualProfile.ChargeAnimationPath, 0.08f);
            addedAny |= TryAddLoop(sprite, normalizedRoot, "slash", visualProfile.SlashAnimationPath, 0.05f);
            addedAny |= TryAddLoop(sprite, normalizedRoot, "warp", visualProfile.WarpAnimationPath, 0.06f);

            if (!addedAny)
                return;

            Sprite = sprite;
            Sprite.CenterOrigin();
            if (Sprite.Has("idle"))
                Sprite.Play("idle");
            else if (Sprite.Has("move"))
                Sprite.Play("move");
            else if (Sprite.Has("charge"))
                Sprite.Play("charge");

            Add(Sprite);
            hasAnimatedSprite = true;
        }

        private bool TryCreateSpriteFromSpriteBank()
        {
            if (string.IsNullOrWhiteSpace(SpriteBankEntryName))
                return false;

            if (GFX.SpriteBank == null || !GFX.SpriteBank.Has(SpriteBankEntryName))
                return false;

            Sprite = GFX.SpriteBank.Create(SpriteBankEntryName);
            if (Sprite == null)
                return false;

            Sprite.CenterOrigin();
            if (Sprite.Has("idle"))
                Sprite.Play("idle");
            else if (Sprite.Has("move"))
                Sprite.Play("move");
            else if (Sprite.Has("charge"))
                Sprite.Play("charge");

            Add(Sprite);
            hasAnimatedSprite = true;
            return true;
        }

        private static bool TryAddLoop(Sprite sprite, string spriteRoot, string animationId, string path, float delay)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (!GFX.Game.HasAtlasSubtextures(spriteRoot + path))
                return false;

            sprite.AddLoop(animationId, path, delay);
            return true;
        }

        private void TryPlay(string animationId, string fallbackAnimation = null)
        {
            if (Sprite == null)
                return;

            if (Sprite.Has(animationId))
            {
                if (Sprite.CurrentAnimationID != animationId)
                    Sprite.Play(animationId);
                return;
            }

            if (!string.IsNullOrEmpty(fallbackAnimation) && Sprite.Has(fallbackAnimation) && Sprite.CurrentAnimationID != fallbackAnimation)
                Sprite.Play(fallbackAnimation);
        }

        private static Vector2 Rotate(Vector2 value, float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            return new Vector2(value.X * cos - value.Y * sin, value.X * sin + value.Y * cos);
        }
    }
}