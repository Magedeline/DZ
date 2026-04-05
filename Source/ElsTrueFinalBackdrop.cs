namespace MaggyHelper.Effects
{
    /// <summary>
    /// Els True Final Boss backdrop effect.
    /// Uses authored sprite layers to build a parallax-heavy void backdrop with
    /// ring bands, tiled eyes, stars, and tier-aware color treatment.
    /// </summary>
    [CustomBackdrop("MaggyHelper/ElsTrueFinalBackdrop")]
    [HotReloadable]
    public class ElsTrueFinalBackdrop : Backdrop
    {
        private const float ScreenWidth = 320f;
        private const float ScreenHeight = 180f;
        private const float BurstDuration = 0.75f;
        private const string TextureRoot = "bgs/maggy/20/els_parallax/";

        private static readonly Color PinkBaseColor = Calc.HexToColor("09030b");
        private static readonly Color PinkOverlayColor = Calc.HexToColor("4d1638");
        private static readonly Color PinkAccentColor = Calc.HexToColor("ff7bcd");
        private static readonly Color PinkHighlightColor = Calc.HexToColor("ffe8f7");
        private static readonly Color PinkIrisColor = Calc.HexToColor("ff4ca7");

        private static readonly Color SoulBaseColor = Calc.HexToColor("020203");
        private static readonly Color SoulOverlayColor = Calc.HexToColor("210a16");
        private static readonly Color SoulAccentColor = Calc.HexToColor("6b1630");
        private static readonly Color SoulHighlightColor = Calc.HexToColor("d6c6d8");
        private static readonly Color SoulEyeColor = Calc.HexToColor("f2eaf2");
        private static readonly Color SoulIrisColor = Calc.HexToColor("c92a58");

        private static readonly Color StellarrussBaseColor = Calc.HexToColor("05060d");
        private static readonly Color StellarrussOverlayColor = Calc.HexToColor("11172d");
        private static readonly Color[] StellarrussPalette =
        {
            Calc.HexToColor("ffffff"),
            Calc.HexToColor("ff6cce"),
            Calc.HexToColor("ff8c42"),
            Calc.HexToColor("ffe66d"),
            Calc.HexToColor("62ffcf"),
            Calc.HexToColor("6ec7ff"),
            Calc.HexToColor("b68cff")
        };

        private readonly MTexture ringsBottomTexture;
        private readonly MTexture ringsCenterTexture;
        private readonly MTexture topStripTexture;
        private readonly MTexture streaksTexture;
        private readonly MTexture starsSparseTexture;
        private readonly MTexture starsDenseTexture;
        private readonly MTexture eyesBandBaseTexture;
        private readonly MTexture eyesBandIrisTexture;

        private Vector2 scrollMultiplier = Vector2.One;
        private Vector2 scrollSpeed = Vector2.Zero;
        private Color tintColor = Color.White;
        private float animationTime;
        private float burstTimer;
        private bool flipX;
        private bool flipY;
        private bool loopX = true;
        private bool loopY = true;
        private MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier currentTier = MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.SoulBlack;

        public float Alpha = 1f;
        public float Intensity = 1f;
        public new float Speed = 1f;
        public float VoidRadius = 60f;
        public float RainbowEdgeIntensity = 1f;
        public float GridExpansionSpeed = 0.4f;
        public float RainbowSpeed = 1.5f;
        public float CorruptionSpeed = 0.8f;
        public Color VoidColor = Color.Black;
        public Color BackgroundColor = SoulBaseColor;

        public ElsTrueFinalBackdrop()
        {
            ringsBottomTexture = TryGetTexture(TextureRoot + "rings_bottom");
            ringsCenterTexture = TryGetTexture(TextureRoot + "rings_center");
            topStripTexture = TryGetTexture(TextureRoot + "top_strip");
            streaksTexture = TryGetTexture(TextureRoot + "streaks");
            starsSparseTexture = TryGetTexture(TextureRoot + "stars_sparse");
            starsDenseTexture = TryGetTexture(TextureRoot + "stars_dense");
            eyesBandBaseTexture = TryGetTexture(TextureRoot + "eyes_band_base");
            eyesBandIrisTexture = TryGetTexture(TextureRoot + "eyes_band_iris");

            SetSiamoTier(currentTier);
        }

        public ElsTrueFinalBackdrop(BinaryPacker.Element data) : this()
        {
            if (data.HasAttr("intensity"))
                Intensity = data.AttrFloat("intensity", 1f);

            if (data.HasAttr("speed"))
                Speed = data.AttrFloat("speed", 1f);

            if (data.HasAttr("voidRadius"))
                VoidRadius = data.AttrFloat("voidRadius", 60f);

            if (data.HasAttr("rainbowEdgeIntensity"))
                RainbowEdgeIntensity = data.AttrFloat("rainbowEdgeIntensity", 1f);

            if (data.HasAttr("gridExpansionSpeed"))
                GridExpansionSpeed = data.AttrFloat("gridExpansionSpeed", 0.4f);

            if (data.HasAttr("rainbowSpeed"))
                RainbowSpeed = data.AttrFloat("rainbowSpeed", 1.5f);

            if (data.HasAttr("corruptionSpeed"))
                CorruptionSpeed = data.AttrFloat("corruptionSpeed", 0.8f);

            if (data.HasAttr("alpha"))
                Alpha = MathHelper.Clamp(data.AttrFloat("alpha", 1f), 0f, 1f);

            if (data.HasAttr("scrollX") || data.HasAttr("scrollY"))
            {
                scrollMultiplier = new Vector2(
                    data.AttrFloat("scrollX", 1f),
                    data.AttrFloat("scrollY", 1f)
                );
            }

            if (data.HasAttr("speedX") || data.HasAttr("speedY"))
            {
                scrollSpeed = new Vector2(
                    data.AttrFloat("speedX", 0f),
                    data.AttrFloat("speedY", 0f)
                );
            }

            if (data.HasAttr("color"))
            {
                try
                {
                    tintColor = Calc.HexToColor(data.Attr("color", "FFFFFF"));
                }
                catch
                {
                    tintColor = Color.White;
                }
            }

            if (data.HasAttr("flipX"))
                flipX = data.AttrBool("flipX", false);

            if (data.HasAttr("flipY"))
                flipY = data.AttrBool("flipY", false);

            if (data.HasAttr("loopX"))
                loopX = data.AttrBool("loopX", true);

            if (data.HasAttr("loopY"))
                loopY = data.AttrBool("loopY", true);
        }

        public void SetSiamoTier(MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier tier)
        {
            currentTier = tier;

            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    BackgroundColor = PinkBaseColor;
                    VoidColor = Calc.HexToColor("14060f");
                    break;

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    BackgroundColor = StellarrussBaseColor;
                    VoidColor = Calc.HexToColor("04050b");
                    break;

                default:
                    BackgroundColor = SoulBaseColor;
                    VoidColor = Calc.HexToColor("040305");
                    break;
            }
        }

        public void TriggerBurst()
        {
            burstTimer = BurstDuration;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            animationTime += Engine.DeltaTime * Math.Max(Speed, 0f);

            if (burstTimer > 0f)
                burstTimer = Math.Max(0f, burstTimer - Engine.DeltaTime);
        }

        public override void Render(Scene scene)
        {
            if (!Visible)
                return;

            Level level = scene as Level;
            if (level == null)
                return;

            float masterAlpha = MathHelper.Clamp(Alpha * FadeAlphaMultiplier, 0f, 1f);
            if (masterAlpha <= 0.001f)
                return;

            float intensityFactor = Math.Max(0f, Intensity);
            float burst = BurstStrength();
            float voidScale = MathHelper.Lerp(0.75f, 1.58f, MathHelper.Clamp((VoidRadius - 28f) / 92f, 0f, 1f));
            float tierMotion = GetTierMotionMultiplier();
            float tierParallax = GetTierParallaxMultiplier();
            float tierSparkle = GetTierSparkleBoost();
            float tierRingScale = GetTierRingScaleBoost();
            float ringPulse = 1f + (float)Math.Sin(animationTime * (0.55f + GridExpansionSpeed * 0.35f) * tierMotion) * (0.03f + intensityFactor * 0.008f) + burst * 0.08f;
            float ringCenterY = MathHelper.Lerp(100f, 88f, MathHelper.Clamp((VoidRadius - 24f) / 100f, 0f, 1f));
            float lowerRingY = ScreenHeight + 10f - MathHelper.Clamp((VoidRadius - 60f) * 0.12f, -6f, 10f);

            Vector2 camera = level.Camera.Position;

            Color baseColor = ApplyTint(BackgroundColor);
            Color overlayColor = ApplyTint(GetOverlayColor(0.18f));
            Color accentColor = ApplyTint(GetAccentColor(0.06f));
            Color secondaryAccentColor = ApplyTint(GetSecondaryAccentColor(0.12f));
            Color highlightColor = ApplyTint(GetHighlightColor(0.24f));
            Color eyeBaseColor = ApplyTint(GetEyeBaseColor(0.32f));
            Color irisColor = ApplyTint(GetIrisColor(0.48f));
            Color burstFlashColor = ApplyTint(GetBurstFlashColor(0.64f));

            Draw.Rect(-16f, -16f, ScreenWidth + 32f, ScreenHeight + 32f, baseColor * masterAlpha);

            float overlayAlpha = MathHelper.Clamp(0.08f + intensityFactor * 0.06f + burst * 0.12f, 0f, 0.5f);
            Draw.Rect(-16f, -16f, ScreenWidth + 32f, ScreenHeight + 32f, overlayColor * masterAlpha * overlayAlpha);

            if (burst > 0f)
            {
                Draw.Rect(
                    -16f,
                    -16f,
                    ScreenWidth + 32f,
                    ScreenHeight + 32f,
                    burstFlashColor * masterAlpha * MathHelper.Clamp(0.05f + burst * 0.14f, 0f, 0.2f)
                );
            }

            DrawRepeated(
                starsDenseTexture,
                GetLayerOffset(camera, starsDenseTexture, new Vector2(0.03f, 0.018f) * tierParallax, new Vector2(-4f * tierMotion, 0.55f * tierMotion), Vector2.Zero, Vector2.One, true, true),
                Color.Lerp(highlightColor, secondaryAccentColor, 0.32f),
                masterAlpha * MathHelper.Clamp((0.18f + intensityFactor * 0.04f + burst * 0.1f) * (0.88f + tierSparkle * 0.14f), 0f, 0.68f),
                Vector2.One,
                true,
                true
            );

            DrawRepeated(
                starsSparseTexture,
                GetLayerOffset(camera, starsSparseTexture, new Vector2(0.06f, 0.03f) * tierParallax, new Vector2(-9f * tierMotion, 1.05f * tierMotion), Vector2.Zero, Vector2.One, true, true),
                Color.Lerp(highlightColor, secondaryAccentColor, 0.15f),
                masterAlpha * MathHelper.Clamp((0.28f + intensityFactor * 0.06f + burst * 0.14f) * tierSparkle, 0f, 0.85f),
                Vector2.One,
                true,
                true
            );

            DrawRepeated(
                streaksTexture,
                GetLayerOffset(camera, streaksTexture, new Vector2(0.095f, 0.024f) * tierParallax, new Vector2((-14f - CorruptionSpeed * 8f) * tierMotion, 0.45f), new Vector2(0f, 24f + (float)Math.Sin(animationTime * 0.7f) * 8f), Vector2.One, true, true),
                secondaryAccentColor,
                masterAlpha * MathHelper.Clamp((0.1f + intensityFactor * 0.035f + burst * 0.16f) * (0.82f + tierMotion * 0.18f), 0f, 0.42f),
                Vector2.One,
                true,
                true
            );

            float eyeBandY = GetTierEyeBandBaseY() + (float)Math.Sin(animationTime * (0.45f + CorruptionSpeed * 0.18f) * tierMotion) * (2.5f + tierSparkle * 1.2f);
            Vector2 eyesOffset = GetLayerOffset(camera, eyesBandBaseTexture, new Vector2(0.14f, 0.035f) * tierParallax, new Vector2((-18f - CorruptionSpeed * 10f) * tierMotion, 0f), new Vector2(0f, eyeBandY), Vector2.One, true, false);
            DrawRepeated(
                eyesBandBaseTexture,
                eyesOffset,
                eyeBaseColor,
                masterAlpha * MathHelper.Clamp((0.18f + intensityFactor * 0.06f) * GetTierEyeBandAlphaBoost(), 0f, 0.66f),
                Vector2.One,
                true,
                false
            );

            DrawRepeated(
                eyesBandIrisTexture,
                eyesOffset + new Vector2((float)Math.Sin(animationTime * (1.25f + RainbowSpeed * 0.25f) * tierMotion) * (4f + burst * 8f + tierSparkle * 1.5f), 0f),
                irisColor,
                masterAlpha * MathHelper.Clamp((0.16f + RainbowEdgeIntensity * 0.11f + burst * 0.1f) * GetTierEyeBandAlphaBoost(), 0f, 0.76f),
                Vector2.One,
                true,
                false
            );

            DrawRepeated(
                topStripTexture,
                GetLayerOffset(camera, topStripTexture, new Vector2(0.18f, 0.026f) * tierParallax, new Vector2((-24f - GridExpansionSpeed * 16f) * tierMotion, 0f), new Vector2(0f, 14f), Vector2.One, true, false),
                highlightColor,
                masterAlpha * MathHelper.Clamp((0.14f + RainbowEdgeIntensity * 0.09f + burst * 0.08f) * (0.92f + tierSparkle * 0.1f), 0f, 0.58f),
                Vector2.One,
                true,
                false
            );

            Vector2 ringParallax = new Vector2(camera.X * 0.11f * scrollMultiplier.X * tierParallax, camera.Y * 0.072f * scrollMultiplier.Y * tierParallax);
            float ringAlpha = masterAlpha * MathHelper.Clamp(0.26f + intensityFactor * 0.08f + RainbowEdgeIntensity * 0.06f + burst * 0.18f, 0f, 0.88f);

            DrawCentered(
                ringsCenterTexture,
                new Vector2(ScreenWidth * 0.5f, ringCenterY) - ringParallax * 0.42f + new Vector2(0f, (float)Math.Sin(animationTime * 0.8f * tierMotion) * 2f),
                Color.Lerp(secondaryAccentColor, highlightColor, 0.32f),
                ringAlpha * 0.34f,
                Vector2.One * (voidScale * tierRingScale * (1.1f + burst * 0.08f) * ringPulse),
                new Vector2(0.5f, 0.5f)
            );

            DrawCentered(
                ringsCenterTexture,
                new Vector2(ScreenWidth * 0.5f, ringCenterY) - ringParallax * 0.5f,
                accentColor,
                ringAlpha,
                Vector2.One * (voidScale * tierRingScale * ringPulse),
                new Vector2(0.5f, 0.5f)
            );

            DrawCentered(
                ringsBottomTexture,
                new Vector2(ScreenWidth * 0.5f, lowerRingY) - ringParallax * 0.68f,
                Color.Lerp(accentColor, highlightColor, 0.45f),
                ringAlpha,
                Vector2.One * (voidScale * tierRingScale * 1.14f * (1f + burst * 0.05f)),
                new Vector2(0.5f, 1f)
            );

            DrawCentered(
                ringsBottomTexture,
                new Vector2(ScreenWidth * 0.5f, lowerRingY + 4f) - ringParallax * 0.74f,
                Color.Lerp(VoidColor, accentColor, 0.18f),
                ringAlpha * 0.24f,
                Vector2.One * (voidScale * tierRingScale * 1.28f * (1f + burst * 0.04f)),
                new Vector2(0.5f, 1f)
            );
        }

        private static MTexture TryGetTexture(string path)
        {
            try
            {
                return GFX.Game[path];
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/ElsTrueFinalBackdrop", $"Missing backdrop texture '{path}': {ex.Message}");
                return null;
            }
        }

        private float BurstStrength()
        {
            if (burstTimer <= 0f)
                return 0f;

            return Ease.CubeOut(burstTimer / BurstDuration);
        }

        private Color GetOverlayColor(float offset)
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkOverlayColor, PinkAccentColor, 0.2f + ((float)Math.Sin(animationTime * 0.45f + offset) * 0.5f + 0.5f) * 0.16f);

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(StellarrussOverlayColor, SampleStellarrussColor(animationTime * 0.42f + offset), 0.26f);

                default:
                    return Color.Lerp(SoulOverlayColor, SoulAccentColor, 0.12f + ((float)Math.Sin(animationTime * 0.35f + offset) * 0.5f + 0.5f) * 0.08f);
            }
        }

        private Color GetAccentColor(float offset)
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkAccentColor, PinkHighlightColor, 0.28f + ((float)Math.Sin(animationTime * (0.8f + RainbowSpeed * 0.1f) + offset) * 0.5f + 0.5f) * 0.22f);

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return SampleStellarrussColor(animationTime * (0.7f + RainbowSpeed * 0.08f) + offset);

                default:
                    return Color.Lerp(SoulAccentColor, SoulHighlightColor, 0.22f + ((float)Math.Sin(animationTime * 0.65f + offset) * 0.5f + 0.5f) * 0.18f);
            }
        }

        private Color GetSecondaryAccentColor(float offset)
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkHighlightColor, PinkAccentColor, 0.52f + ((float)Math.Sin(animationTime * 0.9f + offset) * 0.5f + 0.5f) * 0.14f);

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(Color.White, SampleStellarrussColor(animationTime * 1.15f + offset), 0.72f);

                default:
                    return Color.Lerp(SoulHighlightColor, SoulAccentColor, 0.4f + ((float)Math.Sin(animationTime * 0.7f + offset) * 0.5f + 0.5f) * 0.12f);
            }
        }

        private Color GetHighlightColor(float offset)
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkHighlightColor, PinkAccentColor, 0.12f + ((float)Math.Sin(animationTime * 0.55f + offset) * 0.5f + 0.5f) * 0.14f);

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(Color.White, SampleStellarrussColor(animationTime * (0.95f + RainbowSpeed * 0.12f) + offset), 0.52f);

                default:
                    return Color.Lerp(SoulEyeColor, SoulAccentColor, 0.08f + ((float)Math.Sin(animationTime * 0.5f + offset) * 0.5f + 0.5f) * 0.12f);
            }
        }

        private Color GetEyeBaseColor(float offset)
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(Color.White, PinkHighlightColor, 0.28f);

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(Color.White, SampleStellarrussColor(animationTime * 0.6f + offset), 0.16f);

                default:
                    return SoulEyeColor;
            }
        }

        private Color GetIrisColor(float offset)
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkAccentColor, PinkIrisColor, 0.38f + ((float)Math.Sin(animationTime * (1f + RainbowSpeed * 0.12f) + offset) * 0.5f + 0.5f) * 0.22f);

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return SampleStellarrussColor(animationTime * (1.12f + RainbowSpeed * 0.15f) + offset);

                default:
                    return Color.Lerp(SoulAccentColor, SoulIrisColor, 0.28f + ((float)Math.Sin(animationTime * 0.85f + offset) * 0.5f + 0.5f) * 0.18f);
            }
        }

        private Color GetBurstFlashColor(float offset)
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkHighlightColor, PinkAccentColor, 0.36f + ((float)Math.Sin(animationTime * 1.1f + offset) * 0.5f + 0.5f) * 0.12f);

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(Color.White, SampleStellarrussColor(animationTime * 1.4f + offset), 0.62f);

                default:
                    return Color.Lerp(SoulHighlightColor, SoulIrisColor, 0.32f + ((float)Math.Sin(animationTime * 0.95f + offset) * 0.5f + 0.5f) * 0.08f);
            }
        }

        private float GetTierMotionMultiplier()
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return 1.06f;

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return 1.24f;

                default:
                    return 0.92f;
            }
        }

        private float GetTierParallaxMultiplier()
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return 1.02f;

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return 1.16f;

                default:
                    return 0.9f;
            }
        }

        private float GetTierSparkleBoost()
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return 1.04f;

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return 1.22f;

                default:
                    return 0.9f;
            }
        }

        private float GetTierRingScaleBoost()
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return 1.02f;

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return 1.08f;

                default:
                    return 0.96f;
            }
        }

        private float GetTierEyeBandBaseY()
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return 44f;

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return 40f;

                default:
                    return 46f;
            }
        }

        private float GetTierEyeBandAlphaBoost()
        {
            switch (currentTier)
            {
                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Pink:
                    return 1.08f;

                case MaggyHelper.Entities.ElsTrueFinalBoss.SiamoZeroTier.Stellarruss:
                    return 1.16f;

                default:
                    return 0.92f;
            }
        }

        private static Color SampleStellarrussColor(float offset)
        {
            int count = StellarrussPalette.Length;
            float wrapped = offset % count;
            if (wrapped < 0f)
                wrapped += count;

            int left = (int)Math.Floor(wrapped) % count;
            int right = (left + 1) % count;
            float blend = wrapped - (float)Math.Floor(wrapped);
            return Color.Lerp(StellarrussPalette[left], StellarrussPalette[right], blend);
        }

        private Color ApplyTint(Color color)
        {
            return new Color(color.ToVector4() * tintColor.ToVector4());
        }

        private Vector2 GetLayerOffset(Vector2 camera, MTexture texture, Vector2 parallax, Vector2 drift, Vector2 baseOffset, Vector2 scale, bool repeatX, bool repeatY)
        {
            repeatX &= loopX;
            repeatY &= loopY;

            Vector2 offset = new Vector2(
                -camera.X * parallax.X * scrollMultiplier.X + animationTime * (drift.X + scrollSpeed.X * 16f) + baseOffset.X,
                -camera.Y * parallax.Y * scrollMultiplier.Y + animationTime * (drift.Y + scrollSpeed.Y * 16f) + baseOffset.Y
            );

            if (texture == null)
                return offset;

            float width = Math.Max(1f, texture.Width * Math.Abs(scale.X));
            float height = Math.Max(1f, texture.Height * Math.Abs(scale.Y));

            if (repeatX)
                offset.X = -Mod(offset.X, width);

            if (repeatY)
                offset.Y = -Mod(offset.Y, height);

            return offset;
        }

        private void DrawRepeated(MTexture texture, Vector2 offset, Color color, float alpha, Vector2 scale, bool repeatX, bool repeatY)
        {
            if (texture == null || alpha <= 0.001f)
                return;

            repeatX &= loopX;
            repeatY &= loopY;

            Vector2 baseScale = new Vector2(Math.Abs(scale.X), Math.Abs(scale.Y));
            Vector2 drawScale = new Vector2((flipX ? -1f : 1f) * baseScale.X, (flipY ? -1f : 1f) * baseScale.Y);
            float stepX = Math.Max(1f, texture.Width * baseScale.X);
            float stepY = Math.Max(1f, texture.Height * baseScale.Y);
            float maxX = repeatX ? ScreenWidth + stepX : offset.X + 1f;
            float maxY = repeatY ? ScreenHeight + stepY : offset.Y + 1f;

            for (float x = offset.X; x < maxX; x += stepX)
            {
                for (float y = offset.Y; y < maxY; y += stepY)
                {
                    Vector2 drawPos = new Vector2(x + (flipX ? stepX : 0f), y + (flipY ? stepY : 0f));
                    texture.Draw(drawPos, Vector2.Zero, color * alpha, drawScale);

                    if (!repeatY)
                        break;
                }

                if (!repeatX)
                    break;
            }
        }

        private void DrawCentered(MTexture texture, Vector2 position, Color color, float alpha, Vector2 scale, Vector2 justify)
        {
            if (texture == null || alpha <= 0.001f)
                return;

            Vector2 drawScale = new Vector2(
                (flipX ? -1f : 1f) * Math.Abs(scale.X),
                (flipY ? -1f : 1f) * Math.Abs(scale.Y)
            );

            texture.DrawJustified(position, justify, color * alpha, drawScale);
        }

        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}