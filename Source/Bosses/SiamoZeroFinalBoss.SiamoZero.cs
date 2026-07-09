using Celeste.Helpers;
using Celeste.Entities.Projectiles;

namespace Celeste.Entities
{
    /// <summary>
    /// SiamoZeroFinalBoss - Siamo Zero ("We Are Zero") combat phase.
    /// Phase 3: The Fallen Path - Corrupted dark-path Kirby nightmare form.
    /// 
    /// Attack sets derived from the Siamo Zero sprite assets:
    ///   â€¢ Aeon Hero moves: crescent_beam_shot, energy_sword, tornado_slash,
    ///     revolution_sword, rising_spine, down_thrust, drill_stab,
    ///     thirty_energy_shower, final_beam_sword, spin_slash, rapid_slash
    ///   â€¢ Morpho Knight moves: vortex_strike, double_side_slash, emerge
    ///   â€¢ Timeborder: 120-frame reality-distortion overlay
    /// 
    /// 12 unique attacks total, organized into two sub-phases:
    ///   â€¢ Aeon Hero Fake (melee/sword) - 8 attacks
    ///   â€¢ Morpho Knight Fake (vortex/slash) - 4 attacks
    /// </summary>
    public partial class SiamoZeroFinalBoss
    {
        #region Siamo Zero Constants

        // Sprite atlas paths for Siamo Zero sub-forms
        private const string AeonHeroBasePath = "siamo_zero_aeon_hero_fake/";
        private const string MorphoKnightBasePath = "siamo_zero_morpho_knight_fake/";
        private const string TimebordersBasePath = "siamo_zero_timeborders/";
        private const string SiamoZeroContraPath = "siamo_zero_contra/";

        // SFX constants for Siamo Zero
        private const string SFX_SIAMO_SWORD_SWING = "event:/DZ/new_content/char/els/Els_Slice";
        private const string SFX_SIAMO_BEAMDZ_CHARGE = "event:/DZ/new_content/char/els/ElsDZ_CHarge";
        private const string SFX_SIAMO_BEAM_FIRE = "event:/DZ/new_content/char/els/Els_BeamSlash";
        private const string SFX_SIAMO_TORNADO = "event:/DZ/new_content/char/els/Els_Shell_Screamer";
        private const string SFX_SIAMO_DRILL = "event:/DZ/new_content/char/els/Els_Build";
        private const string SFX_SIAMO_VORTEX = "event:/DZ/new_content/char/els/Els_Time_Manipulator_Start";
        private const string SFX_SIAMO_EMERGE = "event:/DZ/new_content/char/els/dark_matter_spawn";
        private const string SFX_SIAMO_TRANSFORM = "event:/DZ/new_content/char/els/Els_Final_Cry";
        private const string SFX_SIAMO_IMPACT = "event:/DZ/new_content/char/els/big_hit";
        private const string SFX_SIAMO_RISING = "event:/DZ/new_content/char/els/Els_Rift";

        // Phase colors
        private static readonly Color SiamoAeonGold = new Color(255, 220, 128);
        private static readonly Color SiamoAeonCyan = new Color(100, 255, 255);
        private static readonly Color SiamoMorphoPurple = new Color(180, 60, 220);
        private static readonly Color SiamoMorphoMagenta = new Color(255, 50, 150);
        private static readonly Color SiamoTimeborderRed = new Color(220, 30, 60);
        private static readonly Color SiamoPinkRose = new Color(255, 120, 196);
        private static readonly Color SiamoPinkBlush = new Color(255, 216, 238);
        private static readonly Color SiamoSoulBlackBody = new Color(204, 196, 214);
        private static readonly Color SiamoSoulBlackWing = new Color(98, 76, 116);
        private static readonly Color SiamoSoulBlackCrimson = new Color(160, 24, 48);
        private static readonly Color SiamoStellarrussWhite = new Color(250, 250, 255);
        private static readonly Color[] SiamoStellarrussSpectrum =
        {
            Calc.HexToColor("ffffff"),
            Calc.HexToColor("ff6cc8"),
            Calc.HexToColor("ff9a52"),
            Calc.HexToColor("ffe56f"),
            Calc.HexToColor("68ffd4"),
            Calc.HexToColor("6fc7ff"),
            Calc.HexToColor("b08cff")
        };

        // Siamo Zero combat properties
        private bool siamoZeroCombatActive = false;
        private SiamoSubPhase currentSiamoSubPhase = SiamoSubPhase.AeonHeroFake;
        private float siamoTimeborderTimer = 0f;
        private float siamoTimeborderAlpha = 0f;
        private bool siamoTimeborderActive = false;
        private Sprite timeborderSprite;
        private Sprite aeonHeroSprite;
        private Sprite morphoKnightSprite;
        private bool hasTimeborderSprite = false;
        private bool hasAeonHeroSprite = false;
        private bool hasMorphoKnightSprite = false;

        private int GetSiamoCount(int pink, int soulBlack, int stellarruss)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => pink,
                SiamoZeroTier.Stellarruss => stellarruss,
                _ => soulBlack
            };
        }

        private float GetSiamoValue(float pink, float soulBlack, float stellarruss)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => pink,
                SiamoZeroTier.Stellarruss => stellarruss,
                _ => soulBlack
            };
        }

        private Color SampleStellarrussColor(float offset)
        {
            float palettePosition = ((Scene?.TimeActive ?? 0f) * 0.55f) + offset * SiamoStellarrussSpectrum.Length;
            int count = SiamoStellarrussSpectrum.Length;
            float wrapped = palettePosition % count;
            if (wrapped < 0f)
                wrapped += count;

            int left = (int)Math.Floor(wrapped) % count;
            int right = (left + 1) % count;
            float blend = wrapped - (float)Math.Floor(wrapped);
            return Color.Lerp(SiamoStellarrussSpectrum[left], SiamoStellarrussSpectrum[right], blend);
        }

        private Color GetSiamoAeonColor(float offset = 0f)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => Color.Lerp(SiamoPinkRose, SiamoPinkBlush, 0.35f + (float)Math.Sin((Scene?.TimeActive ?? 0f) * 3f + offset) * 0.15f),
                SiamoZeroTier.Stellarruss => SampleStellarrussColor(offset + 0.08f),
                _ => Color.Lerp(SiamoSoulBlackCrimson, SiamoMorphoMagenta, 0.28f)
            };
        }

        private Color GetSiamoMorphoColor(float offset = 0f)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => Color.Lerp(SiamoPinkRose, SiamoMorphoMagenta, 0.6f),
                SiamoZeroTier.Stellarruss => SampleStellarrussColor(offset + 0.24f),
                _ => Color.Lerp(SiamoSoulBlackCrimson, SiamoMorphoPurple, 0.65f)
            };
        }

        private Color GetSiamoTimeborderColor(float offset = 0f)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => Color.Lerp(SiamoPinkRose, SiamoPinkBlush, 0.45f),
                SiamoZeroTier.Stellarruss => Color.Lerp(SiamoStellarrussWhite, SampleStellarrussColor(offset + 0.36f), 0.5f),
                _ => Color.Lerp(SiamoTimeborderRed, SiamoMorphoMagenta, 0.25f)
            };
        }

        private Color GetSiamoBodyTint(float offset = 0f)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => Color.Lerp(Color.White, SiamoPinkBlush, 0.4f),
                SiamoZeroTier.Stellarruss => Color.Lerp(SiamoStellarrussWhite, SampleStellarrussColor(offset + 0.1f), 0.12f),
                _ => SiamoSoulBlackBody
            };
        }

        private Color GetSiamoWingTint(float offset = 0f)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => Color.Lerp(SiamoPinkRose, SiamoPinkBlush, 0.25f),
                SiamoZeroTier.Stellarruss => Color.Lerp(SiamoStellarrussWhite, SampleStellarrussColor(offset + 0.18f), 0.72f),
                _ => SiamoSoulBlackWing
            };
        }

        private Color GetSiamoEyeTint(float offset = 0f)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => Color.Lerp(Color.White, SiamoPinkBlush, 0.22f),
                SiamoZeroTier.Stellarruss => Color.Lerp(SiamoStellarrussWhite, SampleStellarrussColor(offset + 0.28f), 0.18f),
                _ => new Color(242, 234, 242)
            };
        }

        private Color GetSiamoPupilTint(float offset = 0f)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => Color.Lerp(SiamoPinkRose, SiamoMorphoMagenta, 0.55f),
                SiamoZeroTier.Stellarruss => SampleStellarrussColor(offset + 0.34f),
                _ => Color.Lerp(SiamoSoulBlackCrimson, SiamoMorphoMagenta, 0.35f)
            };
        }

        private Color GetSiamoAuraColor(float offset = 0f)
        {
            return currentSiamoSubPhase == SiamoSubPhase.AeonHeroFake
                ? GetSiamoAeonColor(offset)
                : GetSiamoMorphoColor(offset + 0.12f);
        }

        private Color GetSiamoAuraAccentColor(float offset = 0f)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => SiamoPinkBlush,
                SiamoZeroTier.Stellarruss => Color.Lerp(SiamoStellarrussWhite, SampleStellarrussColor(offset + 0.48f), 0.35f),
                _ => Color.Lerp(SiamoSoulBlackBody, GetSiamoMorphoColor(offset), 0.25f)
            };
        }

        private Color GetSiamoCoreColor(float pulse)
        {
            return siamoZeroTier switch
            {
                SiamoZeroTier.Pink => Color.Lerp(SiamoPinkRose, SiamoPinkBlush, pulse),
                SiamoZeroTier.Stellarruss => Color.Lerp(SiamoStellarrussWhite, SampleStellarrussColor(pulse + 0.2f), 0.7f),
                _ => Color.Lerp(SiamoTimeborderRed, SiamoMorphoMagenta, pulse)
            };
        }

        private void ApplySiamoThemeToSprites()
        {
            float offset = (Scene?.TimeActive ?? 0f) * 0.12f;

            if (siamoSprite != null)
                siamoSprite.Color = Color.Lerp(Color.White, GetSiamoBodyTint(offset), GetSiamoValue(0.3f, 0.56f, 0.22f));
            if (siamoWingSprite != null)
                siamoWingSprite.Color = Color.Lerp(Color.White, GetSiamoWingTint(offset), GetSiamoValue(0.38f, 0.7f, 0.4f));
            if (siamoEyeSprite != null)
                siamoEyeSprite.Color = Color.Lerp(Color.White, GetSiamoEyeTint(offset), GetSiamoValue(0.18f, 0.36f, 0.18f));
            if (siamoPupilSprite != null)
                siamoPupilSprite.Color = Color.Lerp(Color.White, GetSiamoPupilTint(offset), GetSiamoValue(0.42f, 0.68f, 0.4f));
            if (aeonHeroSprite != null)
                aeonHeroSprite.Color = Color.Lerp(Color.White, GetSiamoAeonColor(offset + 0.08f), GetSiamoValue(0.22f, 0.36f, 0.28f));
            if (morphoKnightSprite != null)
                morphoKnightSprite.Color = Color.Lerp(Color.White, GetSiamoMorphoColor(offset + 0.2f), GetSiamoValue(0.28f, 0.44f, 0.34f));
        }

        private void ApplySiamoBackgroundTheme()
        {
            bossBg?.SetSiamoTier(siamoZeroTier);
        }

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
            ConqueredPeakCascade,
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

        private static readonly SiamoAttackType[] BaseSiamoAttackPattern =
        {
            SiamoAttackType.CrescentBeamShot,
            SiamoAttackType.EnergySwordCombo,
            SiamoAttackType.TornadoSlash,
            SiamoAttackType.RevolutionSword,
            SiamoAttackType.RisingSpine,
            SiamoAttackType.DownThrust,
            SiamoAttackType.DrillStab,
            SiamoAttackType.EnergyShower,
            SiamoAttackType.VortexStrike,
            SiamoAttackType.DoubleSideSlash,
            SiamoAttackType.MorphoEmerge,
            SiamoAttackType.TimeborderCollapse
        };

        private static readonly SiamoAttackType[] DeltaSiamoAttackPattern =
        {
            SiamoAttackType.CrescentBeamShot,
            SiamoAttackType.EnergySwordCombo,
            SiamoAttackType.ConqueredPeakCascade,
            SiamoAttackType.TornadoSlash,
            SiamoAttackType.RevolutionSword,
            SiamoAttackType.EnergyShower,
            SiamoAttackType.VortexStrike,
            SiamoAttackType.DoubleSideSlash,
            SiamoAttackType.ConqueredPeakCascade,
            SiamoAttackType.MorphoEmerge,
            SiamoAttackType.TimeborderCollapse,
            SiamoAttackType.DownThrust
        };

        private static readonly SiamoAttackType[] CelestialSiamoAttackPattern =
        {
            SiamoAttackType.CrescentBeamShot,
            SiamoAttackType.RevolutionSword,
            SiamoAttackType.ConqueredPeakCascade,
            SiamoAttackType.EnergyShower,
            SiamoAttackType.VortexStrike,
            SiamoAttackType.DoubleSideSlash,
            SiamoAttackType.TimeborderCollapse,
            SiamoAttackType.ConqueredPeakCascade,
            SiamoAttackType.MorphoEmerge,
            SiamoAttackType.EnergySwordCombo,
            SiamoAttackType.DownThrust,
            SiamoAttackType.DrillStab
        };

        private SiamoAttackType ResolveDefaultSiamoAttack(int attackSeed)
        {
            SiamoAttackType[] pattern = siamoIdentity switch
            {
                SiamoZeroIdentity.Delta => DeltaSiamoAttackPattern,
                SiamoZeroIdentity.Celestial => CelestialSiamoAttackPattern,
                _ => BaseSiamoAttackPattern
            };

            return pattern[Math.Abs(attackSeed) % pattern.Length];
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
            arenaRadius = Math.Max(arenaRadius, GetSiamoArenaRadius());
            ClearSummonedKnightClones();
            automaticCloneSummonCooldown = 0.35f;

            setupSiamoZero();
            SetupSiamoZeroCombatSprites();
            ApplySiamoBackgroundTheme();

            // Dramatic activation
            Audio.Play(SFX_SIAMO_TRANSFORM, Position);
            var lvl = Scene as Level;
            lvl?.Shake(3f);
            lvl?.Flash(GetSiamoTimeborderColor(0.14f), true);
            lvl?.Displacement.AddBurst(Position, 3f, 384f, 768f, 4f);

            phaseWiggler.Start();
            EnsureBossMusicState(force: true);
        }

        private void SetupSiamoZeroCombatSprites()
        {
            // Aeon Hero animation sprite (overlay for sword attacks)
            if (aeonHeroSprite == null)
            {
                string aeonPath = BossSpriteAtlasRoot + ResolveSiamoAnimationPath(AeonHeroBasePath, AeonHeroBasePath);
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
                string morphoPath = BossSpriteAtlasRoot + ResolveSiamoAnimationPath(MorphoKnightBasePath, MorphoKnightBasePath);
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
                string tbPath = BossSpriteAtlasRoot + ResolveSiamoAnimationPath(TimebordersBasePath, TimebordersBasePath);
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
                // Frame path doesn't exist â€” silently skip
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
            float rootScale = GetSiamoValue(1.02f, 1.04f, 1.08f);
            float rootWiggle = GetSiamoValue(0.035f, 0.05f, 0.07f);
            float presenceScale = GetSiamoVariantScaleMultiplier();

            // Faster, more aggressive layer pulsing than Penumbra
            ApplyBossLayerTransform(siamoSprite, Vector2.Zero,
                Vector2.One * ((rootScale + phaseWiggler.Value * rootWiggle) * presenceScale),
                wingPulse * GetSiamoValue(0.01f, 0.015f, 0.022f));

            ApplyBossLayerTransform(
                siamoWingSprite, Vector2.Zero,
                new Vector2(
                    GetSiamoValue(1.1f, 1.2f, 1.28f) + wingPulse * GetSiamoValue(0.18f, 0.28f, 0.34f),
                    GetSiamoValue(0.84f, 0.78f, 0.72f) + Math.Abs(wingPulse) * GetSiamoValue(0.18f, 0.26f, 0.32f)) * presenceScale,
                wingPulse * GetSiamoValue(0.14f, 0.2f, 0.26f));

            ApplyBossLayerTransform(
                siamoEyeSprite, Vector2.Zero,
                new Vector2(
                    1f + eyePulse * GetSiamoValue(0.07f, 0.1f, 0.13f),
                    GetSiamoValue(0.9f, 0.84f, 0.8f) + Math.Abs(eyePulse) * GetSiamoValue(0.09f, 0.15f, 0.19f)) * presenceScale,
                eyePulse * GetSiamoValue(0.04f, 0.06f, 0.08f));

            ApplyBossLayerTransform(
                siamoPupilSprite, Vector2.Zero,
                new Vector2(
                    1f + pupilPulse * GetSiamoValue(0.05f, 0.07f, 0.1f),
                    1f + Math.Abs(pupilPulse) * GetSiamoValue(0.06f, 0.08f, 0.11f)) * presenceScale,
                pupilPulse * GetSiamoValue(0.03f, 0.04f, 0.055f));

            ApplySiamoThemeToSprites();

            // Timeborder pulsing overlay
            if (siamoTimeborderActive && hasTimeborderSprite)
            {
                siamoTimeborderTimer += Engine.DeltaTime;
                siamoTimeborderAlpha = GetSiamoValue(0.28f, 0.35f, 0.42f) + (float)Math.Sin(Scene.TimeActive * 3f) * GetSiamoValue(0.1f, 0.15f, 0.2f);
                timeborderSprite.Color = GetSiamoTimeborderColor(0.22f) * siamoTimeborderAlpha;
                timeborderSprite.Scale = Vector2.One * MathHelper.Lerp(1f, presenceScale, 0.9f);
                timeborderSprite.Visible = true;

                if (!timeborderSprite.Animating)
                    timeborderSprite.Play("loop");
            }

            // Sub-phase overlay visibility
            if (hasAeonHeroSprite)
                aeonHeroSprite.Visible = currentSiamoSubPhase == SiamoSubPhase.AeonHeroFake && aeonHeroSprite.Animating;
            if (hasMorphoKnightSprite)
                morphoKnightSprite.Visible = currentSiamoSubPhase == SiamoSubPhase.MorphoKnightFake && morphoKnightSprite.Animating;

            // Core light for Siamo phase â€” deep red / dark magenta
            if (coreLight != null)
            {
                Color siamoColor = GetSiamoCoreColor((energyPulse.Value + 1f) * 0.5f);
                coreLight.Color = siamoColor * 1.6f;
                coreLight.Alpha = 0.9f + phaseWiggler.Value * 0.4f + (presenceScale - 1f) * 0.2f;
                coreLight.StartRadius = GetSiamoValue(360f, 400f, 448f) * presenceScale;
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
                case SiamoAttackType.ConqueredPeakCascade:
                    siamoAttack_ConqueredPeakCascade();
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
        /// Crescent Beam Shot â€” fires 3 crescent projectiles in a fan pattern.
        /// Sprite: crescent_beam_shot
        /// </summary>
        private void siamoAttack_CrescentBeamShot()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "crescent_beam_shot");

            Audio.Play(SFX_SIAMO_BEAMDZ_CHARGE, Position);

            var lvl = Scene as Level;
            lvl?.Displacement.AddBurst(Position, 0.6f, 96f, 192f, 0.8f);

            Alarm.Set(this, GetSiamoValue(0.52f, 0.4f, 0.28f), () =>
            {
                Audio.Play(SFX_SIAMO_BEAM_FIRE, Position);
                lvl?.Shake(1.5f);

                var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();
                Vector2 baseDir = player != null
                    ? (player.Center - Position).SafeNormalize()
                    : new Vector2(facing, 0f);

                int shotCount = GetSiamoCount(3, 3, 5);
                float spreadRange = GetSiamoValue(0.26f, 0.3f, 0.46f);

                for (int i = 0; i < shotCount; i++)
                {
                    float lerp = shotCount <= 1 ? 0.5f : i / (float)(shotCount - 1);
                    float spread = MathHelper.Lerp(-spreadRange, spreadRange, lerp);
                    Vector2 dir = Calc.AngleToVector(baseDir.Angle() + spread, 1f);
                    lvl?.Add(new SiamoZeroCrescentProjectile(
                        ShotOrigin,
                        dir * GetSiamoValue(240f, 280f, 330f),
                        GetSiamoAeonColor(i * 0.08f)));
                    lvl?.ParticlesFG.Emit(PShoot, GetSiamoCount(4, 5, 6), ShotOrigin, dir * 6f);
                }
            });
        }

        /// <summary>
        /// Energy Sword Combo â€” 6-hit sword slash combo with teleporting.
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
            int comboHits = GetSiamoCount(5, 6, 8);

            for (int hit = 0; hit < comboHits; hit++)
            {
                // Teleport near player
                var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    Vector2 offset = new Vector2(
                        Calc.Random.Range(GetSiamoValue(-64f, -80f, -96f), GetSiamoValue(64f, 80f, 96f)),
                        Calc.Random.Range(GetSiamoValue(-48f, -60f, -80f), GetSiamoValue(48f, 60f, 80f))
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
                lvl?.Add(new SiamoZeroEnergyBlade(
                    Position,
                    bladeDir * GetSiamoValue(180f, 200f, 235f),
                    GetSiamoAeonColor(hit * 0.07f),
                    GetSiamoValue(0.7f, 0.8f, 0.95f)));
                lvl?.ParticlesFG.Emit(PBurst, GetSiamoCount(6, 8, 10), Position, Vector2.One * 10f);

                yield return GetSiamoValue(0.22f, 0.18f, 0.14f);
            }

            // Final shockwave
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Shake(1.5f);
            lvl?.Displacement.AddBurst(Position, 1.5f, 128f, 256f, 1f);

            int shockwaveParticles = GetSiamoCount(12, 16, 20);
            for (int i = 0; i < shockwaveParticles; i++)
            {
                float a = (i / (float)shockwaveParticles) * MathHelper.TwoPi;
                Vector2 dir = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
                lvl?.ParticlesFG.Emit(PShoot, 3, Position, dir * 10f);
            }
        }

        /// <summary>
        /// Conquered Peak Cascade â€” Celeste-style corner warps into crossing sword dives.
        /// Uses repeated diagonal rushes inspired by FinalBoss / Conquered Peak attack language.
        /// </summary>
        private void siamoAttack_ConqueredPeakCascade()
        {
            currentSiamoSubPhase = SiamoSubPhase.AeonHeroFake;
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "glide_sword");

            Audio.Play(SFX_SIAMO_BEAMDZ_CHARGE, Position);
            ShowTelegraph(BossTelegraphType.DashCyan, GetSiamoValue(0.42f, 0.34f, 0.26f));

            Add(new Coroutine(siamoConqueredPeakCascadeSequence()));
        }

        private IEnumerator siamoConqueredPeakCascadeSequence()
        {
            var lvl = Scene as Level;
            int passes = siamoIdentity switch
            {
                SiamoZeroIdentity.Celestial => 5,
                SiamoZeroIdentity.Delta => 4,
                _ => 3
            };

            float horizontalReach = GetSiamoValue(150f, 190f, 220f) * GetSiamoVariantScaleMultiplier();
            float verticalReach = GetSiamoValue(96f, 128f, 156f);

            for (int pass = 0; pass < passes; pass++)
            {
                var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();
                Vector2 target = player?.Center ?? Position;
                float side = pass % 2 == 0 ? -1f : 1f;
                Vector2 start = target + new Vector2(side * horizontalReach, -verticalReach - pass * 12f);
                Vector2 end = target + new Vector2(-side * horizontalReach * 0.68f, GetSiamoValue(20f, 36f, 52f));
                Vector2 dashDir = (end - start).SafeNormalize();
                if (dashDir == Vector2.Zero)
                    dashDir = new Vector2(side, 0f);

                Position = start;
                facing = target.X >= Position.X ? 1 : -1;

                Audio.Play(SFX_ELS_TELEPORT, Position);
                lvl?.Displacement.AddBurst(Position, 0.45f, 72f, 144f, 0.4f);
                lvl?.ParticlesFG.Emit(PBurst, GetSiamoCount(6, 8, 10), Position, Vector2.One * 8f);

                ShowTelegraph(BossTelegraphType.DashCyan, GetSiamoValue(0.18f, 0.14f, 0.1f));
                yield return GetSiamoValue(0.18f, 0.14f, 0.1f);

                PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, pass == passes - 1 ? "finish_slash" : "rapid_slash");
                Audio.Play(SFX_SIAMO_SWORD_SWING, Position);

                float dashDuration = GetSiamoValue(0.28f, 0.22f, 0.16f);
                for (float t = 0f; t < dashDuration; t += Engine.DeltaTime)
                {
                    float eased = Ease.CubeIn(t / dashDuration);
                    Position = Vector2.Lerp(start, end, eased);

                    if (Scene.OnInterval(GetSiamoValue(0.07f, 0.05f, 0.035f)))
                    {
                        Vector2 perpendicular = new Vector2(-dashDir.Y, dashDir.X);
                        lvl?.Add(new SiamoZeroEnergyBlade(
                            Position,
                            dashDir * GetSiamoValue(190f, 220f, 260f),
                            GetSiamoAeonColor(pass * 0.1f + t),
                            GetSiamoValue(0.42f, 0.56f, 0.72f)));
                        lvl?.Add(new SiamoZeroEnergyBlade(
                            Position,
                            perpendicular * GetSiamoValue(84f, 108f, 136f),
                            GetSiamoAuraAccentColor(pass * 0.12f),
                            GetSiamoValue(0.26f, 0.34f, 0.42f)));
                        lvl?.ParticlesFG.Emit(PBurst, GetSiamoCount(2, 3, 5), Position, Vector2.One * 6f);
                    }

                    yield return null;
                }

                Audio.Play(SFX_SIAMO_IMPACT, Position);
                lvl?.Shake(GetSiamoValue(1f, 1.25f, 1.5f));
                lvl?.Displacement.AddBurst(Position, 1.1f, 96f, 192f, 0.8f);

                int slashBursts = GetSiamoCount(3, 4, 5);
                for (int i = 0; i < slashBursts; i++)
                {
                    float spread = MathHelper.Lerp(-0.24f, 0.24f, slashBursts <= 1 ? 0.5f : i / (float)(slashBursts - 1));
                    Vector2 burstDir = Calc.AngleToVector(dashDir.Angle() + spread, GetSiamoValue(200f, 235f, 280f));
                    lvl?.Add(new SiamoZeroCrescentProjectile(Position, burstDir, GetSiamoAeonColor(0.12f + pass * 0.08f + i * 0.04f)));
                }

                yield return GetSiamoValue(0.22f, 0.18f, 0.12f);
            }

            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Flash(GetSiamoAuraAccentColor(0.22f) * 0.72f, false);
            lvl?.Displacement.AddBurst(Position, 1.75f, 160f, 320f, 1.6f);

            int finaleBlades = passes * 2 + GetSiamoCount(4, 6, 8);
            for (int i = 0; i < finaleBlades; i++)
            {
                float angle = (i / (float)finaleBlades) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, GetSiamoValue(200f, 240f, 290f));
                lvl?.Add(new SiamoZeroEnergyBlade(
                    Position,
                    dir,
                    GetSiamoAuraAccentColor(i * 0.05f),
                    GetSiamoValue(0.55f, 0.7f, 0.82f)));
            }

            SiamoReturnToAeonHero();
        }

        /// <summary>
        /// Tornado Slash â€” spinning tornado with trailing slash projectiles.
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
            float duration = GetSiamoValue(2.1f, 2.5f, 3f);
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
                if (Scene.OnInterval(GetSiamoValue(0.18f, 0.15f, 0.11f)))
                {
                    lvl?.Add(new SiamoZeroEnergyBlade(
                        Position,
                        Calc.AngleToVector(angle + MathHelper.PiOver2, GetSiamoValue(160f, 180f, 215f)),
                        GetSiamoAeonColor(t + 0.1f),
                        GetSiamoValue(0.5f, 0.6f, 0.75f)));
                    lvl?.ParticlesFG.Emit(PBurst, GetSiamoCount(3, 4, 6), Position, Vector2.One * 12f);
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
            int landingShots = GetSiamoCount(6, 8, 10);
            for (int i = 0; i < landingShots; i++)
            {
                float a = (i / (float)landingShots) * MathHelper.TwoPi;
                Vector2 dir = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
                lvl?.Add(new SiamoZeroCrescentProjectile(
                    Position,
                    dir * GetSiamoValue(180f, 200f, 240f),
                    GetSiamoAeonColor(0.22f + i * 0.06f)));
            }
        }

        /// <summary>
        /// Revolution Sword â€” spinning sword ring that expands outward.
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
            int waves = GetSiamoCount(2, 3, 4);

            for (int w = 0; w < waves; w++)
            {
                float baseAngle = w * (MathHelper.TwoPi / waves);
                int bladeCount = GetSiamoCount(4, 5, 6) + w * GetSiamoCount(1, 2, 3);
                float radius = GetSiamoValue(56f, 60f, 68f) + w * GetSiamoValue(34f, 40f, 48f);

                Audio.Play(SFX_SIAMO_SWORD_SWING, Position);
                lvl?.Shake(0.8f);

                for (int i = 0; i < bladeCount; i++)
                {
                    float angle = baseAngle + (i / (float)bladeCount) * MathHelper.TwoPi;
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    Vector2 spawnPos = Position + dir * radius;

                    lvl?.Add(new SiamoZeroEnergyBlade(
                        spawnPos,
                        dir * GetSiamoValue(190f, 220f, 255f),
                        GetSiamoAeonColor(w * 0.14f + i * 0.05f),
                        GetSiamoValue(0.85f, 1f, 1.15f)));
                    lvl?.ParticlesFG.Emit(PShoot, 3, spawnPos, dir * 4f);
                }

                lvl?.Displacement.AddBurst(Position, 1f, radius, radius + 128f, 0.8f);
                yield return GetSiamoValue(0.64f, 0.5f, 0.34f);
            }
        }

        /// <summary>
        /// Rising Spine â€” vertical chain of spine projectiles rising from the ground.
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
            int spineCount = GetSiamoCount(6, 8, 10);
            for (int i = 0; i < spineCount; i++)
            {
                float offsetX = (i - (spineCount - 1) * 0.5f) * GetSiamoValue(44f, 40f, 36f);
                Vector2 spinePos = new Vector2(baseX + offsetX, groundY);

                Audio.Play(SFX_SIAMO_RISING, spinePos);
                lvl?.Displacement.AddBurst(spinePos, 0.8f, 32f, 64f, 0.5f);

                lvl?.Add(new SiamoZeroSpinePillar(spinePos, GetSiamoAeonColor(i * 0.06f)));
                lvl?.ParticlesFG.Emit(PBurst, GetSiamoCount(4, 6, 8), spinePos, Vector2.One * 6f);

                yield return GetSiamoValue(0.15f, 0.12f, 0.09f);
            }

            // Final burst
            lvl?.Shake(1f);
        }

        /// <summary>
        /// Down Thrust â€” dives downward with a powerful thrust attack.
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
            Vector2 riseTarget = Position + new Vector2(0, -GetSiamoValue(108f, 120f, 148f));
            float riseDuration = GetSiamoValue(0.48f, 0.4f, 0.32f);
            Vector2 startPos = Position;

            for (float t = 0; t < riseDuration; t += Engine.DeltaTime)
            {
                Position = Vector2.Lerp(startPos, riseTarget, Ease.CubeOut(t / riseDuration));
                yield return null;
            }

            yield return GetSiamoValue(0.3f, 0.2f, 0.12f);

            // Track player position at this moment
            Vector2 thrustTarget = player?.Center ?? (Position + new Vector2(0, 200f));
            thrustTarget.Y += 40f; // Slightly below player

            // Dive down
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            startPos = Position;
            float diveDuration = GetSiamoValue(0.32f, 0.25f, 0.18f);

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
            lvl?.Flash(GetSiamoAeonColor(0.28f) * 0.6f, false);

            // Ground shockwave â€” spawn blades radiating outward
            int shockwaveBlades = GetSiamoCount(8, 10, 12);
            for (int i = 0; i < shockwaveBlades; i++)
            {
                float angle = (i / (float)shockwaveBlades) * MathHelper.TwoPi;
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                lvl?.Add(new SiamoZeroEnergyBlade(
                    Position,
                    dir * GetSiamoValue(220f, 250f, 290f),
                    GetSiamoAeonColor(i * 0.05f),
                    GetSiamoValue(1f, 1.2f, 1.35f)));
            }
        }

        /// <summary>
        /// Drill Stab â€” rapid forward drill attack leaving projectile trail.
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

            float drillDistance = GetSiamoValue(260f, 300f, 340f);
            float drillDuration = GetSiamoValue(0.42f, 0.35f, 0.28f);
            Vector2 startPos = Position;
            Vector2 endPos = startPos + direction * drillDistance;

            for (float t = 0; t < drillDuration; t += Engine.DeltaTime)
            {
                Position = Vector2.Lerp(startPos, endPos, Ease.CubeIn(t / drillDuration));

                // Leave blade trail behind
                if (Scene.OnInterval(GetSiamoValue(0.07f, 0.05f, 0.035f)))
                {
                    Vector2 perpDir = new Vector2(-direction.Y, direction.X);
                    lvl?.Add(new SiamoZeroEnergyBlade(
                        Position,
                        perpDir * GetSiamoValue(100f, 120f, 145f),
                        GetSiamoAeonColor(0.4f),
                        GetSiamoValue(0.42f, 0.5f, 0.65f)));
                    lvl?.Add(new SiamoZeroEnergyBlade(
                        Position,
                        -perpDir * GetSiamoValue(100f, 120f, 145f),
                        GetSiamoAeonColor(0.52f),
                        GetSiamoValue(0.42f, 0.5f, 0.65f)));
                    lvl?.ParticlesFG.Emit(PBurst, GetSiamoCount(2, 3, 5), Position, Vector2.One * 6f);
                }

                yield return null;
            }

            // End impact
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Shake(1.5f);
            lvl?.Displacement.AddBurst(Position, 1.5f, 96f, 192f, 1f);
        }

        /// <summary>
        /// Energy Shower â€” rain of 30 energy projectiles from above.
        /// Sprites: thirty_energy_shower (5 sub-anims a-e)
        /// </summary>
        private void siamoAttack_EnergyShower()
        {
            PlayBossAnimationSet(ElsPhase.SiamoZero, "hit", "boss");
            PlaySiamoOverlay(SiamoSubPhase.AeonHeroFake, "thirty_energy_shower");

            Audio.Play(SFX_SIAMO_BEAMDZ_CHARGE, Position);

            Add(new Coroutine(siamoEnergyShowerSequence()));
        }

        private IEnumerator siamoEnergyShowerSequence()
        {
            var lvl = Scene as Level;

            // Charge up
            lvl?.Flash(GetSiamoAeonColor(0.35f) * 0.4f, false);
            lvl?.Displacement.AddBurst(Position, 1f, 128f, 256f, 1f);
            yield return GetSiamoValue(0.75f, 0.6f, 0.42f);

            // Rain 30 energy projectiles
            Audio.Play(SFX_SIAMO_BEAM_FIRE, Position);
            lvl?.Shake(2f);

            var player = lvl?.Tracker.GetEntity<global::Celeste.Player>();
            float centerX = player?.X ?? Position.X;
            int projectileCount = GetSiamoCount(20, 30, 42);

            for (int i = 0; i < projectileCount; i++)
            {
                float spawnX = centerX + Calc.Random.Range(-200f, 200f);
                float spawnY = (lvl?.Camera.Top ?? Position.Y - 200f) - 32f;
                Vector2 spawnPos = new Vector2(spawnX, spawnY);

                float angleToCenter = (float)Math.Atan2(Position.Y - spawnY, centerX - spawnX);
                Vector2 vel = Calc.AngleToVector(angleToCenter + Calc.Random.Range(-0.3f, 0.3f), Calc.Random.Range(180f, 320f));

                lvl?.Add(new SiamoZeroCrescentProjectile(spawnPos, vel, GetSiamoAeonColor(i * 0.04f)));

                if (i % 5 == 0)
                    Audio.Play(SFX_ELS_RIFT_BULLET, spawnPos);

                yield return GetSiamoValue(0.07f, 0.05f, 0.035f);
            }

            yield return GetSiamoValue(0.4f, 0.3f, 0.18f);
            lvl?.Shake(1f);
        }

        #endregion

        #region Morpho Knight Fake Attacks

        /// <summary>
        /// Vortex Strike â€” summons a vortex that pulls player in, then strikes.
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
            lvl?.Flash(GetSiamoMorphoColor(0.16f) * 0.5f, false);
            yield return GetSiamoValue(0.75f, 0.6f, 0.45f);

            // Pull phase â€” displacement pulls inward
            PlaySiamoOverlay(SiamoSubPhase.MorphoKnightFake, "vortex_pull");
            Audio.Play(SFX_SIAMO_VORTEX, Position);

            for (float t = 0; t < GetSiamoValue(1.2f, 1.5f, 1.9f); t += Engine.DeltaTime)
            {
                lvl?.Displacement.AddBurst(Position, -0.8f, 256f, 512f, 0.2f);

                // Inward particles
                for (int i = 0; i < GetSiamoCount(3, 4, 6); i++)
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
            lvl?.Flash(GetSiamoMorphoColor(0.28f), true);
            lvl?.Displacement.AddBurst(Position, 3f, 256f, 512f, 2f);

            // Explosion of blades
            int vortexBladeCount = GetSiamoCount(10, 12, 16);
            for (int i = 0; i < vortexBladeCount; i++)
            {
                float angle = (i / (float)vortexBladeCount) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                lvl?.Add(new SiamoZeroEnergyBlade(
                    Position,
                    dir * GetSiamoValue(260f, 300f, 340f),
                    GetSiamoMorphoColor(i * 0.06f),
                    GetSiamoValue(1f, 1.2f, 1.35f)));
            }
        }

        /// <summary>
        /// Double Side Slash â€” two sweeping crescent slashes from left/right.
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

            int leftSlashCount = GetSiamoCount(5, 6, 8);
            for (int i = 0; i < leftSlashCount; i++)
            {
                float angle = MathHelper.PiOver2 + (i / (float)leftSlashCount) * MathHelper.Pi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                lvl?.Add(new SiamoZeroCrescentProjectile(
                    Position + new Vector2(-80f, 0f),
                    dir * GetSiamoValue(230f, 260f, 300f),
                    GetSiamoMorphoColor(0.08f + i * 0.05f)));
            }
            lvl?.Displacement.AddBurst(Position + new Vector2(-80f, 0f), 1f, 96f, 192f, 0.8f);

            yield return GetSiamoValue(0.45f, 0.35f, 0.24f);

            // Right slash
            Audio.Play(SFX_SIAMO_SWORD_SWING, Position);
            lvl?.Shake(1f);

            int rightSlashCount = GetSiamoCount(5, 6, 8);
            for (int i = 0; i < rightSlashCount; i++)
            {
                float angle = -MathHelper.PiOver2 + (i / (float)rightSlashCount) * MathHelper.Pi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                lvl?.Add(new SiamoZeroCrescentProjectile(
                    Position + new Vector2(80f, 0f),
                    dir * GetSiamoValue(230f, 260f, 300f),
                    GetSiamoMorphoColor(0.34f + i * 0.05f)));
            }
            lvl?.Displacement.AddBurst(Position + new Vector2(80f, 0f), 1f, 96f, 192f, 0.8f);
        }

        /// <summary>
        /// Morpho Emerge â€” disappears then erupts from below with a massive strike.
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

            yield return GetSiamoValue(0.95f, 0.8f, 0.6f);

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
            lvl?.Flash(GetSiamoMorphoColor(0.42f) * 0.8f, true);
            lvl?.Displacement.AddBurst(Position, 2.5f, 192f, 384f, 2f);

            // Upward energy pillar (spawn spine pillars going up)
            int pillarCount = GetSiamoCount(5, 6, 8);
            for (int i = 0; i < pillarCount; i++)
            {
                Vector2 pillarPos = Position - new Vector2(0, i * 48f);
                lvl?.Add(new SiamoZeroSpinePillar(pillarPos, GetSiamoMorphoColor(0.18f + i * 0.06f)));
                lvl?.ParticlesFG.Emit(PBurst, 4, pillarPos, Vector2.One * 8f);
            }

            // Rise up
            Vector2 startPos = Position;
            float riseDuration = GetSiamoValue(0.5f, 0.4f, 0.3f);
            Vector2 riseTarget = Position - new Vector2(0, GetSiamoValue(124f, 140f, 164f));
            for (float t = 0; t < riseDuration; t += Engine.DeltaTime)
            {
                Position = Vector2.Lerp(startPos, riseTarget, Ease.CubeOut(t / riseDuration));
                yield return null;
            }
        }

        /// <summary>
        /// Timeborder Collapse â€” activates the 120-frame timeborder overlay and
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

            lvl?.Flash(GetSiamoTimeborderColor(0.4f), true);
            lvl?.Shake(3f);
            lvl?.Displacement.AddBurst(Position, 3f, 384f, 768f, 4f);

            yield return GetSiamoValue(0.68f, 0.5f, 0.34f);

            // Wave 1: projectiles from cardinal directions
            int waveCount = GetSiamoCount(2, 3, 4);
            for (int wave = 0; wave < waveCount; wave++)
            {
                Audio.Play(SFX_ELS_SHELLCRACK, Position);
                lvl?.Shake(1.5f);

                float cameraLeft = lvl?.Camera.Left ?? Position.X - 200f;
                float cameraRight = lvl?.Camera.Right ?? Position.X + 200f;
                float cameraTop = lvl?.Camera.Top ?? Position.Y - 200f;
                float cameraBottom = lvl?.Camera.Bottom ?? Position.Y + 200f;

                int projectilesPerSide = GetSiamoCount(5, 6, 8) + wave * GetSiamoCount(1, 2, 3);
                for (int i = 0; i < projectilesPerSide; i++)
                {
                    float fraction = (i + 0.5f) / projectilesPerSide;

                    // From left
                    lvl?.Add(new SiamoZeroCrescentProjectile(
                        new Vector2(cameraLeft - 16f, MathHelper.Lerp(cameraTop, cameraBottom, fraction)),
                        new Vector2(Calc.Random.Range(GetSiamoValue(130f, 150f, 180f), GetSiamoValue(220f, 250f, 320f)), 0f), GetSiamoTimeborderColor(wave * 0.18f + i * 0.03f)));

                    // From right
                    lvl?.Add(new SiamoZeroCrescentProjectile(
                        new Vector2(cameraRight + 16f, MathHelper.Lerp(cameraTop, cameraBottom, fraction)),
                        new Vector2(Calc.Random.Range(GetSiamoValue(-220f, -250f, -320f), GetSiamoValue(-130f, -150f, -180f)), 0f), GetSiamoTimeborderColor(0.22f + wave * 0.18f + i * 0.03f)));
                }

                yield return GetSiamoValue(1f, 0.8f, 0.65f);
            }

            // Final massive shockwave
            Audio.Play(SFX_ELS_TIME_MANIPULATOR_END, Position);
            Audio.Play(SFX_SIAMO_IMPACT, Position);
            lvl?.Shake(4f);
            lvl?.Flash(Color.White, true);
            lvl?.Displacement.AddBurst(Position, 4f, 512f, 1024f, 5f);

            // 24 projectiles in all directions
            int collapseBurstCount = GetSiamoCount(18, 24, 32);
            for (int i = 0; i < collapseBurstCount; i++)
            {
                float angle = (i / (float)collapseBurstCount) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, Calc.Random.Range(200f, 350f));
                lvl?.Add(new SiamoZeroCrescentProjectile(Position, dir, GetSiamoTimeborderColor(i * 0.04f)));
            }

            yield return GetSiamoValue(2.4f, 2f, 1.4f);

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
            lvl?.Flash(GetSiamoMorphoColor(0.32f) * 0.7f, true);
        }

        #endregion
    }
}
