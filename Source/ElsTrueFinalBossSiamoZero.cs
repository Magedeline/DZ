using MaggyHelper.Helpers;
using MaggyHelper.Entities.Projectiles;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// ElsTrueFinalBoss - Siamo Zero ("We Are Zero") combat phase.
    /// Phase 3: The Fallen Path - Corrupted dark-path Kirby nightmare form.
    /// 
    /// Attack sets derived from the Siamo Zero sprite assets:
    ///   • Aeon Hero moves: crescent_beam_shot, energy_sword, tornado_slash,
    ///     revolution_sword, rising_spine, down_thrust, drill_stab,
    ///     thirty_energy_shower, final_beam_sword, spin_slash, rapid_slash
    ///   • Morpho Knight moves: vortex_strike, double_side_slash, emerge
    ///   • Timeborder: 120-frame reality-distortion overlay
    /// 
    /// 12 unique attacks total, organized into two sub-phases:
    ///   • Aeon Hero Fake (melee/sword) - 8 attacks
    ///   • Morpho Knight Fake (vortex/slash) - 4 attacks
    /// </summary>
    public partial class ElsTrueFinalBoss
    {
        #region Siamo Zero Constants

        // Sprite atlas paths for Siamo Zero sub-forms
        private const string AeonHeroBasePath = "siamo_zero_aeon_hero_fake/";
        private const string MorphoKnightBasePath = "siamo_zero_morpho_knight_fake/";
        private const string TimebordersBasePath = "siamo_zero_timeborders/";
        private const string SiamoZeroContraPath = "siamo_zero_contra/";

        // SFX constants for Siamo Zero
        private const string SFX_SIAMO_SWORD_SWING = "event:/desolozantas/final_content/char/els/Els_Slice";
        private const string SFX_SIAMO_BEAM_CHARGE = "event:/desolozantas/final_content/char/els/Els_Charge";
        private const string SFX_SIAMO_BEAM_FIRE = "event:/desolozantas/final_content/char/els/Els_BeamSlash";
        private const string SFX_SIAMO_TORNADO = "event:/desolozantas/final_content/char/els/Els_Shell_Screamer";
        private const string SFX_SIAMO_DRILL = "event:/desolozantas/final_content/char/els/Els_Build";
        private const string SFX_SIAMO_VORTEX = "event:/desolozantas/final_content/char/els/Els_Time_Manipulator_Start";
        private const string SFX_SIAMO_EMERGE = "event:/desolozantas/final_content/char/els/Els_Darkmatter_Spawn";
        private const string SFX_SIAMO_TRANSFORM = "event:/desolozantas/final_content/char/els/Els_Final_Cry";
        private const string SFX_SIAMO_IMPACT = "event:/desolozantas/final_content/char/els/Els_BigHit";
        private const string SFX_SIAMO_RISING = "event:/desolozantas/final_content/char/els/Els_Rift";

        // Phase colors
        private static readonly Color SiamoAeonGold = new Color(255, 220, 128);
        private static readonly Color SiamoAeonCyan = new Color(100, 255, 255);
        private static readonly Color SiamoMorphoPurple = new Color(180, 60, 220);
        private static readonly Color SiamoMorphoMagenta = new Color(255, 50, 150);
        private static readonly Color SiamoTimeborderRed = new Color(220, 30, 60);

        // Siamo Zero combat properties
        private bool siamoZeroCombatActive = false;
        private SiamoSubPhase currentSiamoSubPhase = SiamoSubPhase.AeonHeroFake;
        private float siamoTimeborderTimer = 0f;
        private float siamoTimeborderAlpha = 0f;
        private bool siamoTimeborderActive = false;
        private int siamoTimeborderFrame = 0;
        private Sprite timeborderSprite;
        private Sprite aeonHeroSprite;
        private Sprite morphoKnightSprite;
        private bool hasTimeborderSprite = false;
        private bool hasAeonHeroSprite = false;
        private bool hasMorphoKnightSprite = false;
        private float siamoTransformProgress = 0f;

        #endregion

        #region Siamo Zero Sub-Phase Enum

        public enum SiamoSubPhase
        {
            AeonHeroFake,
            MorphoKnightFake
        }

        public enum SiamoAttackType
        {
            // Aeon Hero Fake attacks
            CrescentBeamShot,
            EnergySwordCombo,
            TornadoSlash,
            RevolutionSword,
            RisingSpine,
            DownThrust,
            DrillStab,
            EnergyShower,

            // Morpho Knight Fake attacks
            VortexStrike,
            DoubleSideSlash,
            MorphoEmerge,
            TimeborderCollapse
        }

        #endregion

        #region Siamo Zero Setup

        /// <summary>
        /// Initialize Siamo Zero for combat (upgrade from cutscene-only to full combat phase).
        /// Call this when transitioning to the Fallen Path fight.
        /// </summary>
        public void ActivateSiamoZeroCombat()
        {
            if (siamoZeroCombatActive) return;

            siamoZeroCombatActive = true;
            currentElsPhase = ElsPhase.SiamoZero;
            currentSiamoSubPhase = SiamoSubPhase.AeonHeroFake;
            ClearSummonedKnightClones();
            automaticCloneSummonCooldown = 0.35f;

            setupSiamoZero();
            SetupSiamoZeroCombatSprites();

            // Dramatic activation
            Audio.Play(SFX_SIAMO_TRANSFORM, Position);
            var lvl = Scene as Level;
            lvl?.Shake(3f);
            lvl?.Flash(SiamoTimeborderRed, true);
            lvl?.Displacement.AddBurst(Position, 3f, 384f, 768f, 4f);

            phaseWiggler.Start();
        }

        private void SetupSiamoZeroCombatSprites()
        {
            // Aeon Hero animation sprite (overlay for sword attacks)
            if (aeonHeroSprite == null)
            {
                string aeonPath = BossSpriteAtlasRoot + AeonHeroBasePath;
                if (GFX.Game.HasAtlasSubtextures(aeonPath + "idle"))
                {
                    aeonHeroSprite = new Sprite(GFX.Game, aeonPath);
                    aeonHeroSprite.CenterOrigin();
                    aeonHeroSprite.Visible = false;

                    // Aeon Hero animations from sprite assets
                    AddSiamoAnim(aeonHeroSprite, "idle", "idle", 0.1f, true);
                    AddSiamoAnim(aeonHeroSprite, "awaken", "awaken", 0.08f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "move", "move", 0.08f, true);
                    AddSiamoAnim(aeonHeroSprite, "jump", "jump", 0.08f, false);
                    AddSiamoAnim(aeonHeroSprite, "guard", "guard", 0.1f, true);
                    AddSiamoAnim(aeonHeroSprite, "taking_damage", "taking_damage", 0.06f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "defeated", "defeated", 0.1f, false);

                    // Combat animations
                    AddSiamoAnim(aeonHeroSprite, "crescent_beam_shot", "crescent_beam_shot", 0.06f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "energy_sword", "energy_sword", 0.05f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "tornado_attack", "tornado_attack", 0.06f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "tornado_slash", "tornado_slash", 0.05f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "revolution_sword", "revolution_sword", 0.05f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "rising_spine", "rising_spine", 0.06f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "down_thrust", "down_thrust", 0.05f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "drill_stab", "drill_stab", 0.04f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "thirty_energy_shower", "thirty_energy_shower", 0.04f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "final_beam_sword", "final_beam_sword", 0.05f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "spin_slash", "spin_slash", 0.04f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "rapid_slash", "rapid_slash", 0.04f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "slash_with_shockwave", "slash_with_shockwave", 0.05f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "overhead_slash", "overhead_slash", 0.05f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "finish_slash", "finish_slash", 0.05f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "glide_sword", "glide_sword", 0.06f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "fly_start", "fly_start", 0.06f, false, "idle");
                    AddSiamoAnim(aeonHeroSprite, "transform", "transform", 0.06f, false);

                    Add(aeonHeroSprite);
                    hasAeonHeroSprite = true;
                }
            }

            // Morpho Knight animation sprite (overlay for vortex attacks)
            if (morphoKnightSprite == null)
            {
                string morphoPath = BossSpriteAtlasRoot + MorphoKnightBasePath;
                if (GFX.Game.HasAtlasSubtextures(morphoPath + "emerge"))
                {
                    morphoKnightSprite = new Sprite(GFX.Game, morphoPath);
                    morphoKnightSprite.CenterOrigin();
                    morphoKnightSprite.Visible = false;

                    AddSiamoAnim(morphoKnightSprite, "emerge", "emerge", 0.06f, false, "idle");
                    AddSiamoAnim(morphoKnightSprite, "first_slash", "first_slash", 0.05f, false);
                    AddSiamoAnim(morphoKnightSprite, "second_slash", "second_slash", 0.05f, false);
                    AddSiamoAnim(morphoKnightSprite, "double_side_slash", "double_side_slash", 0.04f, false);
                    AddSiamoAnim(morphoKnightSprite, "vortex_summon", "vortex_summon", 0.06f, false);
                    AddSiamoAnim(morphoKnightSprite, "vortex_pull", "vortex_pull", 0.06f, true);
                    AddSiamoAnim(morphoKnightSprite, "vortex_strike", "vortex_strike", 0.04f, false);
                    AddSiamoAnim(morphoKnightSprite, "swords", "swords", 0.08f, true);

                    Add(morphoKnightSprite);
                    hasMorphoKnightSprite = true;
                }
            }

            // Timeborder overlay sprite (120 frames)
            if (timeborderSprite == null)
            {
                string tbPath = BossSpriteAtlasRoot + TimebordersBasePath;
                if (GFX.Game.HasAtlasSubtextures(tbPath + "timeborders"))
                {
                    timeborderSprite = new Sprite(GFX.Game, tbPath);
                    timeborderSprite.CenterOrigin();
                    timeborderSprite.Visible = false;

                    timeborderSprite.AddLoop("loop", "timeborders", 0.06f);
                    Add(timeborderSprite);
                    hasTimeborderSprite = true;
                }
            }
        }

        /// <summary>
        /// Helper: register an animation on a Siamo sprite, skipping if frames don't exist.
        /// </summary>
        private static void AddSiamoAnim(Sprite sprite, string id, string framePath, float delay, bool loop, string gotoAnim = null)
        {
            try
            {
                if (loop)
                    sprite.AddLoop(id, framePath, delay);
                else if (!string.IsNullOrEmpty(gotoAnim))
                    sprite.Add(id, framePath, delay, gotoAnim);
                else
                    sprite.Add(id, framePath, delay);
            }
            catch
            {
                // Frame path doesn't exist — silently skip
            }
        }

        #endregion

        #region Siamo Zero Update

        private void updateSiamoZero()
        {
            if (!siamoZeroCombatActive) return;

            float wingPulse = (float)Math.Sin(Scene.TimeActive * 12f);
            float eyePulse = (float)Math.Sin(Scene.TimeActive * 8f);
            float pupilPulse = (float)Math.Sin(Scene.TimeActive * 9.5f);

            // Faster, more aggressive layer pulsing than Penumbra
            ApplyBossLayerTransform(siamoSprite, Vector2.Zero,
                Vector2.One * (1.04f + phaseWiggler.Value * 0.05f), wingPulse * 0.015f);

            ApplyBossLayerTransform(
                siamoWingSprite, Vector2.Zero,
                new Vector2(1.2f + wingPulse * 0.28f, 0.78f + Math.Abs(wingPulse) * 0.26f),
                wingPulse * 0.2f);

            ApplyBossLayerTransform(
                siamoEyeSprite, Vector2.Zero,
                new Vector2(1f + eyePulse * 0.1f, 0.84f + Math.Abs(eyePulse) * 0.15f),
                eyePulse * 0.06f);

            ApplyBossLayerTransform(
                siamoPupilSprite, Vector2.Zero,
                new Vector2(1f + pupilPulse * 0.07f, 1f + Math.Abs(pupilPulse) * 0.08f),
                pupilPulse * 0.04f);

            // Timeborder pulsing overlay
            if (siamoTimeborderActive && hasTimeborderSprite)
            {
                siamoTimeborderTimer += Engine.DeltaTime;
                siamoTimeborderAlpha = 0.35f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.15f;
                timeborderSprite.Color = Color.White * siamoTimeborderAlpha;
                timeborderSprite.Visible = true;

                if (!timeborderSprite.Animating)
                    timeborderSprite.Play("loop");
            }

            // Sub-phase overlay visibility
            if (hasAeonHeroSprite)
                aeonHeroSprite.Visible = currentSiamoSubPhase == SiamoSubPhase.AeonHeroFake && aeonHeroSprite.Animating;
            if (hasMorphoKnightSprite)
                morphoKnightSprite.Visible = currentSiamoSubPhase == SiamoSubPhase.MorphoKnightFake && morphoKnightSprite.Animating;

            // Core light for Siamo phase — deep red / dark magenta
            if (coreLight != null)
            {
                Color siamoColor = Color.Lerp(SiamoTimeborderRed, SiamoMorphoMagenta, (energyPulse.Value + 1f) * 0.5f);
                coreLight.Color = siamoColor * 1.6f;
                coreLight.Alpha = 0.9f + phaseWiggler.Value * 0.4f;
                coreLight.StartRadius = 400f;
            }
        }

        #endregion

        #region Siamo Zero Attack Dispatch

        /// <summary>
        /// Execute a Siamo Zero attack by enum ID.
        /// </summary>
        public void ExecuteSiamoAttack(SiamoAttackType attack)
        {
            switch (attack)
            {
                case SiamoAttackType.CrescentBeamShot:
                    siamoAttack_CrescentBeamShot();
                    break;
                case SiamoAttackType.EnergySwordCombo:
                    siamoAttack_EnergySwordCombo();
                    break;
                case SiamoAttackType.TornadoSlash:
                    siamoAttack_TornadoSlash();
                    break;
                case SiamoAttackType.RevolutionSword:
                    siamoAttack_RevolutionSword();
                    break;
                case SiamoAttackType.RisingSpine:
                    siamoAttack_RisingSpine();
                    break;
                case SiamoAttackType.DownThrust:
                    siamoAttack_DownThrust();
                    break;
                case SiamoAttackType.DrillStab:
                    siamoAttack_DrillStab();
                    break;
                case SiamoAttackType.EnergyShower:
                    siamoAttack_EnergyShower();
                    break;
                case SiamoAttackType.VortexStrike:
                    siamoAttack_VortexStrike();
                    break;
                case SiamoAttackType.DoubleSideSlash:
                    siamoAttack_DoubleSideSlash();
                    break;
                case SiamoAttackType.MorphoEmerge:
                    siamoAttack_MorphoEmerge();
                    break;
                case SiamoAttackType.TimeborderCollapse:
                    siamoAttack_TimeborderCollapse();
                    break;
            }

            phaseWiggler.Start();
        }

        /// <summary>
        /// Execute a Siamo Zero attack by string name (for custom attack sequences in Loenn).
        /// </summary>
        public bool TryExecuteSiamoAttack(string attackName)
        {
            if (!siamoZeroCombatActive) return false;

            if (Enum.TryParse<SiamoAttackType>(attackName, ignoreCase: true, out var attack))
            {
                ExecuteSiamoAttack(attack);
                return true;
            }
            return false;
        }

        #endregion

        #region Aeon Hero Fake Attacks

        /// <summary>
        /// Crescent Beam Shot — fires 3 crescent projectiles in a fan pattern.
        /// Sprite: crescent_beam_shot
        /// </summary>
        private void siamoAttack_CrescentBeamShot()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "crescent_beam_shot");

            Audio.Play(SFX_SIAMO_BEAM_CHARGE, Position);

            var lvl = Scene as Level;
            lvl?.Displacement.AddBurst(Position, 0.6f, 96f, 192f, 0.8f);

            Alarm.Set(this, 0.4f, () =>
            {
                Audio.Play(SFX_SIAMO_BEAM_FIRE, Position);
                lvl?.Shake(1.5f);

                var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();
                Vector2 baseDir = player != null
                    ? (player.Center - Position).SafeNormalize()
                    : new Vector2(facing, 0f);

                // Fire 3 crescent projectiles in a fan
                for (int i = -1; i <= 1; i++)
                {
                    float spread = i * 0.3f; // ~17° spread
                    Vector2 dir = Calc.AngleToVector(baseDir.Angle() + spread, 1f);
                    lvl?.Add(new SiamoZeroCrescentProjectile(ShotOrigin, dir * 280f, SiamoAeonCyan));
                    lvl?.ParticlesFG.Emit(PShoot, 5, ShotOrigin, dir * 6f);
                }
            });
        }

        /// <summary>
        /// Energy Sword Combo — 6-hit sword slash combo with teleporting.
        /// Sprites: energy_sword (6 sub-anims a-f)
        /// </summary>
        private void siamoAttack_EnergySwordCombo()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "energy_sword");

            Audio.Play(SFX_SIAMO_SWORD_SWING, Position);

            Add(new Coroutine(siamoEnergySwordSequence()));
        }

        private IEnumerator siamoEnergySwordSequence()
        {
            var lvl = Scene as Level;

            for (int hit = 0; hit < 6; hit++)
            {
                // Teleport near player
                var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    Vector2 offset = new Vector2(
                        Calc.Random.Range(-80f, 80f),
                        Calc.Random.Range(-60f, 60f)
                    );
                    Vector2 targetPos = player.Center + offset;

                    Audio.Play(SFX_ELS_TELEPORT, Position);
                    lvl?.Displacement.AddBurst(Position, 0.3f, 48f, 96f, 0.2f);
                    Position = targetPos;
                }

                // Slash effect
                Audio.Play(SFX_SIAMO_SWORD_SWING, Position);
                lvl?.Displacement.AddBurst(Position, 0.5f, 64f, 128f, 0.3f);

                // Spawn blade hitbox
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                Vector2 bladeDir = Calc.AngleToVector(angle, 1f);
                lvl?.Add(new SiamoZeroEnergyBlade(Position, bladeDir * 200f, SiamoAeonGold, 0.8f));
                lvl?.ParticlesFG.Emit(PBurst, 8, Position, Vector2.One * 10f);

                yield return 0.18f;
            }

            // Final shockwave
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Shake(1.5f);
            lvl?.Displacement.AddBurst(Position, 1.5f, 128f, 256f, 1f);

            for (int i = 0; i < 16; i++)
            {
                float a = (i / 16f) * MathHelper.TwoPi;
                Vector2 dir = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
                lvl?.ParticlesFG.Emit(PShoot, 3, Position, dir * 10f);
            }
        }

        /// <summary>
        /// Tornado Slash — spinning tornado with trailing slash projectiles.
        /// Sprites: tornado_attack, tornado_slash
        /// </summary>
        private void siamoAttack_TornadoSlash()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "tornado_attack");

            Audio.Play(SFX_SIAMO_TORNADO, Position);

            Add(new Coroutine(siamoTornadoSlashSequence()));
        }

        private IEnumerator siamoTornadoSlashSequence()
        {
            var lvl = Scene as Level;
            float duration = 2.5f;
            float elapsed = 0f;
            Vector2 startPos = Position;

            var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 target = player?.Center ?? Position;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                // Spiral toward player
                float angle = t * MathHelper.TwoPi * 3f;
                float radius = MathHelper.Lerp(200f, 20f, t);
                Position = Vector2.Lerp(startPos, target, Ease.CubeIn(t))
                    + new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);

                // Emit tornado particles and slash projectiles
                if (Scene.OnInterval(0.15f))
                {
                    lvl?.Add(new SiamoZeroEnergyBlade(Position,
                        Calc.AngleToVector(angle + MathHelper.PiOver2, 180f),
                        SiamoAeonCyan, 0.6f));
                    lvl?.ParticlesFG.Emit(PBurst, 4, Position, Vector2.One * 12f);
                    Audio.Play(SFX_SIAMO_SWORD_SWING, Position);
                }

                lvl?.Displacement.AddBurst(Position, 0.2f, 32f, 64f, 0.1f);
                elapsed += Engine.DeltaTime;
                yield return null;
            }

            // Landing impact
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Shake(2f);
            lvl?.Displacement.AddBurst(Position, 2f, 192f, 384f, 1.5f);

            // Release 8 projectiles on landing
            for (int i = 0; i < 8; i++)
            {
                float a = (i / 8f) * MathHelper.TwoPi;
                Vector2 dir = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
                lvl?.Add(new SiamoZeroCrescentProjectile(Position, dir * 200f, SiamoAeonCyan));
            }
        }

        /// <summary>
        /// Revolution Sword — spinning sword ring that expands outward.
        /// Sprites: revolution_sword (5 sub-anims a-e)
        /// </summary>
        private void siamoAttack_RevolutionSword()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "revolution_sword");

            Audio.Play(SFX_SIAMO_SWORD_SWING, Position);

            Add(new Coroutine(siamoRevolutionSwordSequence()));
        }

        private IEnumerator siamoRevolutionSwordSequence()
        {
            var lvl = Scene as Level;
            int waves = 3;

            for (int w = 0; w < waves; w++)
            {
                float baseAngle = w * (MathHelper.TwoPi / waves);
                int bladeCount = 5 + w * 2;
                float radius = 60f + w * 40f;

                Audio.Play(SFX_SIAMO_SWORD_SWING, Position);
                lvl?.Shake(0.8f);

                for (int i = 0; i < bladeCount; i++)
                {
                    float angle = baseAngle + (i / (float)bladeCount) * MathHelper.TwoPi;
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    Vector2 spawnPos = Position + dir * radius;

                    lvl?.Add(new SiamoZeroEnergyBlade(spawnPos, dir * 220f, SiamoAeonGold, 1f));
                    lvl?.ParticlesFG.Emit(PShoot, 3, spawnPos, dir * 4f);
                }

                lvl?.Displacement.AddBurst(Position, 1f, radius, radius + 128f, 0.8f);
                yield return 0.5f;
            }
        }

        /// <summary>
        /// Rising Spine — vertical chain of spine projectiles rising from the ground.
        /// Sprites: rising_spine (13 sub-anims a-m)
        /// </summary>
        private void siamoAttack_RisingSpine()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "rising_spine");

            Audio.Play(SFX_SIAMO_RISING, Position);

            Add(new Coroutine(siamoRisingSpineSequence()));
        }

        private IEnumerator siamoRisingSpineSequence()
        {
            var lvl = Scene as Level;
            var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();

            float baseX = player?.X ?? Position.X;
            float groundY = player?.Y ?? Position.Y;

            // Spawn 8 spine pillars in a line
            for (int i = 0; i < 8; i++)
            {
                float offsetX = (i - 3.5f) * 40f;
                Vector2 spinePos = new Vector2(baseX + offsetX, groundY);

                Audio.Play(SFX_SIAMO_RISING, spinePos);
                lvl?.Displacement.AddBurst(spinePos, 0.8f, 32f, 64f, 0.5f);

                lvl?.Add(new SiamoZeroSpinePillar(spinePos, SiamoAeonGold));
                lvl?.ParticlesFG.Emit(PBurst, 6, spinePos, Vector2.One * 6f);

                yield return 0.12f;
            }

            // Final burst
            lvl?.Shake(1f);
        }

        /// <summary>
        /// Down Thrust — dives downward with a powerful thrust attack.
        /// Sprites: down_thrust (2 sub-anims a-b)
        /// </summary>
        private void siamoAttack_DownThrust()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "down_thrust");

            Audio.Play(SFX_SIAMO_DRILL, Position);

            Add(new Coroutine(siamoDownThrustSequence()));
        }

        private IEnumerator siamoDownThrustSequence()
        {
            var lvl = Scene as Level;
            var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();

            // Rise up
            Vector2 riseTarget = Position + new Vector2(0, -120f);
            float riseDuration = 0.4f;
            Vector2 startPos = Position;

            for (float t = 0; t < riseDuration; t += Engine.DeltaTime)
            {
                Position = Vector2.Lerp(startPos, riseTarget, Ease.CubeOut(t / riseDuration));
                yield return null;
            }

            yield return 0.2f;

            // Track player position at this moment
            Vector2 thrustTarget = player?.Center ?? (Position + new Vector2(0, 200f));
            thrustTarget.Y += 40f; // Slightly below player

            // Dive down
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            startPos = Position;
            float diveDuration = 0.25f;

            for (float t = 0; t < diveDuration; t += Engine.DeltaTime)
            {
                Position = Vector2.Lerp(startPos, thrustTarget, Ease.CubeIn(t / diveDuration));
                lvl?.ParticlesFG.Emit(PShoot, 2, Position, Vector2.UnitY * -8f);
                yield return null;
            }

            // Impact
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Shake(2.5f);
            lvl?.Displacement.AddBurst(Position, 2f, 128f, 256f, 1.5f);
            lvl?.Flash(SiamoAeonGold * 0.6f, false);

            // Ground shockwave — spawn blades radiating outward
            for (int i = 0; i < 10; i++)
            {
                float angle = (i / 10f) * MathHelper.TwoPi;
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                lvl?.Add(new SiamoZeroEnergyBlade(Position, dir * 250f, SiamoAeonGold, 1.2f));
            }
        }

        /// <summary>
        /// Drill Stab — rapid forward drill attack leaving projectile trail.
        /// Sprites: drill_stab (3 sub-anims a-c)
        /// </summary>
        private void siamoAttack_DrillStab()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "drill_stab");

            Audio.Play(SFX_SIAMO_DRILL, Position);

            Add(new Coroutine(siamoDrillStabSequence()));
        }

        private IEnumerator siamoDrillStabSequence()
        {
            var lvl = Scene as Level;
            var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();

            Vector2 direction = player != null
                ? (player.Center - Position).SafeNormalize()
                : new Vector2(facing, 0f);

            float drillDistance = 300f;
            float drillDuration = 0.35f;
            Vector2 startPos = Position;
            Vector2 endPos = startPos + direction * drillDistance;

            for (float t = 0; t < drillDuration; t += Engine.DeltaTime)
            {
                Position = Vector2.Lerp(startPos, endPos, Ease.CubeIn(t / drillDuration));

                // Leave blade trail behind
                if (Scene.OnInterval(0.05f))
                {
                    Vector2 perpDir = new Vector2(-direction.Y, direction.X);
                    lvl?.Add(new SiamoZeroEnergyBlade(Position, perpDir * 120f, SiamoAeonCyan, 0.5f));
                    lvl?.Add(new SiamoZeroEnergyBlade(Position, -perpDir * 120f, SiamoAeonCyan, 0.5f));
                    lvl?.ParticlesFG.Emit(PBurst, 3, Position, Vector2.One * 6f);
                }

                yield return null;
            }

            // End impact
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Shake(1.5f);
            lvl?.Displacement.AddBurst(Position, 1.5f, 96f, 192f, 1f);
        }

        /// <summary>
        /// Energy Shower — rain of 30 energy projectiles from above.
        /// Sprites: thirty_energy_shower (5 sub-anims a-e)
        /// </summary>
        private void siamoAttack_EnergyShower()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "thirty_energy_shower");

            Audio.Play(SFX_SIAMO_BEAM_CHARGE, Position);

            Add(new Coroutine(siamoEnergyShowerSequence()));
        }

        private IEnumerator siamoEnergyShowerSequence()
        {
            var lvl = Scene as Level;

            // Charge up
            lvl?.Flash(SiamoAeonGold * 0.4f, false);
            lvl?.Displacement.AddBurst(Position, 1f, 128f, 256f, 1f);
            yield return 0.6f;

            // Rain 30 energy projectiles
            Audio.Play(SFX_SIAMO_BEAM_FIRE, Position);
            lvl?.Shake(2f);

            var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();
            float centerX = player?.X ?? Position.X;

            for (int i = 0; i < 30; i++)
            {
                float spawnX = centerX + Calc.Random.Range(-200f, 200f);
                float spawnY = (lvl?.Camera.Top ?? Position.Y - 200f) - 32f;
                Vector2 spawnPos = new Vector2(spawnX, spawnY);

                float angleToCenter = (float)Math.Atan2(Position.Y - spawnY, centerX - spawnX);
                Vector2 vel = Calc.AngleToVector(angleToCenter + Calc.Random.Range(-0.3f, 0.3f), Calc.Random.Range(180f, 320f));

                lvl?.Add(new SiamoZeroCrescentProjectile(spawnPos, vel, SiamoAeonGold));

                if (i % 5 == 0)
                    Audio.Play(SFX_ELS_RIFT_BULLET, spawnPos);

                yield return 0.05f;
            }

            yield return 0.3f;
            lvl?.Shake(1f);
        }

        #endregion

        #region Morpho Knight Fake Attacks

        /// <summary>
        /// Vortex Strike — summons a vortex that pulls player in, then strikes.
        /// Sprites: vortex_summon, vortex_pull, vortex_strike
        /// </summary>
        private void siamoAttack_VortexStrike()
        {
            currentSiamoSubPhase = SiamoSubPhase.MorphoKnightFake;
            PlayBossAnimationSet(ElsPhase.SiamoZero, "void", "boss");
            PlaySiamoOverlay(SiamoSubPhase.MorphoKnightFake, "vortex_summon");

            Audio.Play(SFX_SIAMO_VORTEX, Position);

            Add(new Coroutine(siamoVortexStrikeSequence()));
        }

        private IEnumerator siamoVortexStrikeSequence()
        {
            var lvl = Scene as Level;

            // Summon vortex
            lvl?.Displacement.AddBurst(Position, 1f, 192f, 384f, 2f);
            lvl?.Flash(SiamoMorphoPurple * 0.5f, false);
            yield return 0.6f;

            // Pull phase — displacement pulls inward
            PlaySiamoOverlay(SiamoSubPhase.MorphoKnightFake, "vortex_pull");
            Audio.Play(SFX_SIAMO_VORTEX, Position);

            for (float t = 0; t < 1.5f; t += Engine.DeltaTime)
            {
                lvl?.Displacement.AddBurst(Position, -0.8f, 256f, 512f, 0.2f);

                // Inward particles
                for (int i = 0; i < 4; i++)
                {
                    float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                    float dist = Calc.Random.Range(100f, 250f);
                    Vector2 from = Position + Calc.AngleToVector(angle, dist);
                    lvl?.ParticlesFG.Emit(PBurst, 1, from, Vector2.One * 4f);
                }

                yield return null;
            }

            // Strike!
            PlaySiamoOverlay(SiamoSubPhase.MorphoKnightFake, "vortex_strike");
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Shake(3f);
            lvl?.Flash(SiamoMorphoMagenta, true);
            lvl?.Displacement.AddBurst(Position, 3f, 256f, 512f, 2f);

            // Explosion of blades
            for (int i = 0; i < 12; i++)
            {
                float angle = (i / 12f) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                lvl?.Add(new SiamoZeroEnergyBlade(Position, dir * 300f, SiamoMorphoMagenta, 1.2f));
            }
        }

        /// <summary>
        /// Double Side Slash — two sweeping crescent slashes from left/right.
        /// Sprites: double_side_slash
        /// </summary>
        private void siamoAttack_DoubleSideSlash()
        {
            currentSiamoSubPhase = SiamoSubPhase.MorphoKnightFake;
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.MorphoKnightFake, "double_side_slash");

            Audio.Play(SFX_SIAMO_SWORD_SWING, Position);

            Add(new Coroutine(siamoDoubleSideSlashSequence()));
        }

        private IEnumerator siamoDoubleSideSlashSequence()
        {
            var lvl = Scene as Level;

            // Left slash
            Audio.Play(SFX_SIAMO_SWORD_SWING, Position);
            lvl?.Shake(1f);

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.PiOver2 + (i / 6f) * MathHelper.Pi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                lvl?.Add(new SiamoZeroCrescentProjectile(Position + new Vector2(-80f, 0f), dir * 260f, SiamoMorphoPurple));
            }
            lvl?.Displacement.AddBurst(Position + new Vector2(-80f, 0f), 1f, 96f, 192f, 0.8f);

            yield return 0.35f;

            // Right slash
            Audio.Play(SFX_SIAMO_SWORD_SWING, Position);
            lvl?.Shake(1f);

            for (int i = 0; i < 6; i++)
            {
                float angle = -MathHelper.PiOver2 + (i / 6f) * MathHelper.Pi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                lvl?.Add(new SiamoZeroCrescentProjectile(Position + new Vector2(80f, 0f), dir * 260f, SiamoMorphoMagenta));
            }
            lvl?.Displacement.AddBurst(Position + new Vector2(80f, 0f), 1f, 96f, 192f, 0.8f);
        }

        /// <summary>
        /// Morpho Emerge — disappears then erupts from below with a massive strike.
        /// Sprites: emerge, c_emerge
        /// </summary>
        private void siamoAttack_MorphoEmerge()
        {
            currentSiamoSubPhase = SiamoSubPhase.MorphoKnightFake;
            PlayBossAnimationSet(ElsPhase.SiamoZero, "void", "boss");
            PlaySiamoOverlay(SiamoSubPhase.MorphoKnightFake, "emerge");

            Audio.Play(SFX_SIAMO_EMERGE, Position);

            Add(new Coroutine(siamoMorphoEmergeSequence()));
        }

        private IEnumerator siamoMorphoEmergeSequence()
        {
            var lvl = Scene as Level;
            var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();

            // Vanish
            Collidable = false;
            SetPhaseLayerColor(ElsPhase.SiamoZero, Color.White * 0f);
            if (hasAeonHeroSprite) aeonHeroSprite.Visible = false;
            if (hasMorphoKnightSprite) morphoKnightSprite.Visible = false;

            Audio.Play(SFX_ELS_TELEPORT, Position);
            lvl?.Displacement.AddBurst(Position, 0.5f, 64f, 128f, 0.3f);

            yield return 0.8f;

            // Teleport below player
            Vector2 emergePos = player != null
                ? new Vector2(player.X, player.Y + 80f)
                : Position;

            Position = emergePos;

            // Emerge with massive upward strike
            SetPhaseLayerColor(ElsPhase.SiamoZero, Color.White);
            Collidable = true;
            PlaySiamoOverlay(SiamoSubPhase.MorphoKnightFake, "emerge");

            Audio.Play(SFX_SIAMO_IMPACT, Position);
            Audio.Play(SFX_SIAMO_EMERGE, Position);
            lvl?.Shake(2.5f);
            lvl?.Flash(SiamoMorphoMagenta * 0.8f, true);
            lvl?.Displacement.AddBurst(Position, 2.5f, 192f, 384f, 2f);

            // Upward energy pillar (spawn spine pillars going up)
            for (int i = 0; i < 6; i++)
            {
                Vector2 pillarPos = Position - new Vector2(0, i * 48f);
                lvl?.Add(new SiamoZeroSpinePillar(pillarPos, SiamoMorphoPurple));
                lvl?.ParticlesFG.Emit(PBurst, 4, pillarPos, Vector2.One * 8f);
            }

            // Rise up
            Vector2 startPos = Position;
            Vector2 riseTarget = Position - new Vector2(0, 140f);
            for (float t = 0; t < 0.4f; t += Engine.DeltaTime)
            {
                Position = Vector2.Lerp(startPos, riseTarget, Ease.CubeOut(t / 0.4f));
                yield return null;
            }
        }

        /// <summary>
        /// Timeborder Collapse — activates the 120-frame timeborder overlay and
        /// tears reality with waves of projectiles from all sides.
        /// Sprites: siamo_zero_timeborders/timeborders (120 frames)
        /// </summary>
        private void siamoAttack_TimeborderCollapse()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "void", "boss");

            Audio.Play(SFX_SIAMO_TRANSFORM, Position);
            Audio.Play(SFX_ELS_TIME_MANIPULATOR_START, Position);

            siamoTimeborderActive = true;

            Add(new Coroutine(siamoTimeborderCollapseSequence()));
        }

        private IEnumerator siamoTimeborderCollapseSequence()
        {
            var lvl = Scene as Level;

            // Activate timeborder overlay
            if (hasTimeborderSprite && timeborderSprite != null)
            {
                timeborderSprite.Visible = true;
                timeborderSprite.Play("loop");
            }

            lvl?.Flash(SiamoTimeborderRed, true);
            lvl?.Shake(3f);
            lvl?.Displacement.AddBurst(Position, 3f, 384f, 768f, 4f);

            yield return 0.5f;

            // Wave 1: projectiles from cardinal directions
            for (int wave = 0; wave < 3; wave++)
            {
                Audio.Play(SFX_ELS_SHELLCRACK, Position);
                lvl?.Shake(1.5f);

                float cameraLeft = lvl?.Camera.Left ?? Position.X - 200f;
                float cameraRight = lvl?.Camera.Right ?? Position.X + 200f;
                float cameraTop = lvl?.Camera.Top ?? Position.Y - 200f;
                float cameraBottom = lvl?.Camera.Bottom ?? Position.Y + 200f;

                int projectilesPerSide = 6 + wave * 2;
                for (int i = 0; i < projectilesPerSide; i++)
                {
                    float fraction = (i + 0.5f) / projectilesPerSide;

                    // From left
                    lvl?.Add(new SiamoZeroCrescentProjectile(
                        new Vector2(cameraLeft - 16f, MathHelper.Lerp(cameraTop, cameraBottom, fraction)),
                        new Vector2(Calc.Random.Range(150f, 250f), 0f), SiamoTimeborderRed));

                    // From right
                    lvl?.Add(new SiamoZeroCrescentProjectile(
                        new Vector2(cameraRight + 16f, MathHelper.Lerp(cameraTop, cameraBottom, fraction)),
                        new Vector2(Calc.Random.Range(-250f, -150f), 0f), SiamoTimeborderRed));
                }

                yield return 0.8f;
            }

            // Final massive shockwave
            Audio.Play(SFX_ELS_TIME_MANIPULATOR_END, Position);
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Shake(4f);
            lvl?.Flash(Color.White, true);
            lvl?.Displacement.AddBurst(Position, 4f, 512f, 1024f, 5f);

            // 24 projectiles in all directions
            for (int i = 0; i < 24; i++)
            {
                float angle = (i / 24f) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, Calc.Random.Range(200f, 350f));
                lvl?.Add(new SiamoZeroCrescentProjectile(Position, dir, SiamoTimeborderRed));
            }

            yield return 2f;

            // Fade out timeborder
            siamoTimeborderActive = false;
            if (hasTimeborderSprite && timeborderSprite != null)
                timeborderSprite.Visible = false;
        }

        #endregion

        #region Siamo Zero Helpers

        private void PlaySiamoOverlay(SiamoSubPhase subPhase, string animId)
        {
            if (subPhase == SiamoSubPhase.AeonHeroFake && hasAeonHeroSprite)
            {
                currentSiamoSubPhase = SiamoSubPhase.AeonHeroFake;
                if (aeonHeroSprite.Has(animId))
                {
                    aeonHeroSprite.Play(animId);
                    aeonHeroSprite.Visible = true;
                }
            }
            else if (subPhase == SiamoSubPhase.MorphoKnightFake && hasMorphoKnightSprite)
            {
                currentSiamoSubPhase = SiamoSubPhase.MorphoKnightFake;
                if (morphoKnightSprite.Has(animId))
                {
                    morphoKnightSprite.Play(animId);
                    morphoKnightSprite.Visible = true;
                }
            }
        }

        /// <summary>
        /// Transition from Morpho Knight sub-phase back to Aeon Hero.
        /// </summary>
        public void SiamoReturnToAeonHero()
        {
            currentSiamoSubPhase = SiamoSubPhase.AeonHeroFake;
            if (hasMorphoKnightSprite)
                morphoKnightSprite.Visible = false;

            PlayBossAnimationSet(ElsPhase.SiamoZero, "idle", "boss");
        }

        /// <summary>
        /// Transition from Aeon Hero sub-phase to Morpho Knight.
        /// </summary>
        public void SiamoTransformToMorphoKnight()
        {
            currentSiamoSubPhase = SiamoSubPhase.MorphoKnightFake;
            if (hasAeonHeroSprite)
                aeonHeroSprite.Visible = false;

            PlayBossAnimationSet(ElsPhase.SiamoZero, "void", "boss");
            Audio.Play(SFX_SIAMO_TRANSFORM, Position);

            var lvl = Scene as Level;
            lvl?.Shake(2f);
            lvl?.Displacement.AddBurst(Position, 2f, 256f, 512f, 2f);
            lvl?.Flash(SiamoMorphoPurple * 0.7f, true);
        }

        #endregion
    }
}
