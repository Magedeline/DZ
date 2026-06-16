namespace Celeste.Effects
{
    /// <summary>
    /// Els True Final Boss backdrop effect.
    /// Uses the authored els_parallax atlas to build a layered void backdrop with
    /// patterned corruption bands, tiled eyes, stars, and tier-aware color treatment.
    /// </summary>
    [CustomBackdrop("DZ/ElsTrueFinalBackdrop")]
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

        private readonly MTexture backgroundLoopTexture;
        private readonly MTexture ringsBottomTexture;
        private readonly MTexture ringsCenterTexture;
        private readonly MTexture starsSparseTexture;
        private readonly MTexture starsDenseTexture;
        private readonly MTexture eyesBandBaseTexture;
        private readonly MTexture eyesBandIrisTexture;
        private readonly MTexture streaksTexture;
        private readonly MTexture topStripTexture;

        private Vector2 scrollMultiplier = Vector2.One;
        private Vector2 scrollSpeed = Vector2.Zero;
        private Color tintColor = Color.White;
        private float animationTime;
        private float burstTimer;
        private bool flipX;
        private bool flipY;
        private bool loopX = true;
        private bool loopY = true;
        private Vector2 currentWind = Vector2.Zero;
        private Vector2 targetWind = Vector2.Zero;
        private Vector2 windViewOffset = Vector2.Zero;
        private float windInfluence;
        private float windDirection = 1f;
        private float targetWindDirection = 1f;
        private float windTransitionSpeed = 2f;
        private global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier currentTier = global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.SoulBlack;

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
        public bool WindAffectsView = true;
        public bool WindAffectsSpinDirection = true;
        public float WindViewStrength = 1f;
        public float SpinDirection = 1f;
        public float WindIrisSpinStrength = 0.6f;

        public ElsTrueFinalBackdrop()
        {
            backgroundLoopTexture = TryGetTexture(TextureRoot + "background_loop");
            ringsBottomTexture = TryGetTexture(TextureRoot + "rings_bottom");
            ringsCenterTexture = TryGetTexture(TextureRoot + "rings_center");
            starsSparseTexture = TryGetTexture(TextureRoot + "stars_sparse");
            starsDenseTexture = TryGetTexture(TextureRoot + "stars_dense");
            eyesBandBaseTexture = TryGetTexture(TextureRoot + "eyes_band_base");
            eyesBandIrisTexture = TryGetTexture(TextureRoot + "eyes_band_iris");
            streaksTexture = TryGetOptionalTexture(TextureRoot + "streaks");
            topStripTexture = TryGetOptionalTexture(TextureRoot + "top_strip");

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

            if (data.HasAttr("windAffectsView"))
                WindAffectsView = data.AttrBool("windAffectsView", true);

            if (data.HasAttr("windAffectsSpinDirection"))
                WindAffectsSpinDirection = data.AttrBool("windAffectsSpinDirection", true);

            if (data.HasAttr("windViewStrength"))
                WindViewStrength = Math.Max(0f, data.AttrFloat("windViewStrength", 1f));

            if (data.HasAttr("windIrisSpinStrength"))
                WindIrisSpinStrength = Math.Max(0f, data.AttrFloat("windIrisSpinStrength", 0.6f));

            if (data.HasAttr("spinDirection"))
                SpinDirection = MathHelper.Clamp(data.AttrFloat("spinDirection", 1f), -1f, 1f);
            else if (data.HasAttr("direction"))
                SpinDirection = MathHelper.Clamp(data.AttrFloat("direction", 1f), -1f, 1f);

            if (data.HasAttr("windTransitionSpeed"))
                windTransitionSpeed = Math.Max(0.01f, data.AttrFloat("windTransitionSpeed", 2f));

            windDirection = SpinDirection;
            targetWindDirection = SpinDirection;

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

        public void SetSiamoTier(global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier tier)
        {
            currentTier = tier;

            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    BackgroundColor = PinkBaseColor;
                    VoidColor = Calc.HexToColor("14060f");
                    break;

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
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

            UpdateWindSystem(scene as Level);

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
            float effectiveSpinDirection = WindAffectsSpinDirection ? windDirection : SpinDirection;
            float spinSpeedMultiplier = WindAffectsSpinDirection ? 1f + windInfluence * 0.6f : 1f;
            float spinTime = animationTime * effectiveSpinDirection * spinSpeedMultiplier;
            float tierPatternBoost = 0.84f + tierSparkle * 0.18f;
            float patternPulse = 1f + (float)Math.Sin(spinTime * (0.32f + GridExpansionSpeed * 0.18f) * tierMotion) * (0.035f + intensityFactor * 0.01f);
            float membranePulse = 1f + (float)Math.Sin(spinTime * (0.58f + CorruptionSpeed * 0.16f) * tierMotion + 0.45f) * (0.06f + burst * 0.04f);
            float ringPulse = 1f + (float)Math.Sin(spinTime * (0.55f + GridExpansionSpeed * 0.35f) * tierMotion) * (0.03f + intensityFactor * 0.008f) + burst * 0.08f;
            float ringCenterY = MathHelper.Lerp(100f, 88f, MathHelper.Clamp((VoidRadius - 24f) / 100f, 0f, 1f));
            float lowerRingY = ScreenHeight + 10f - MathHelper.Clamp((VoidRadius - 60f) * 0.12f, -6f, 10f);
            float membraneY = ringCenterY - 8f + (float)Math.Sin(spinTime * 0.38f * tierMotion) * 3.5f;

            Vector2 camera = level.Camera.Position;
            float windBlend = WindAffectsView ? MathHelper.Clamp(0.3f + windInfluence * 0.7f, 0f, 1f) : 0f;
            Vector2 viewWindOffset = windViewOffset * windBlend;
            Vector2 deepWindLayerOffset = viewWindOffset * -1.15f;
            Vector2 midWindLayerOffset = viewWindOffset * -0.75f;
            Vector2 surfaceWindLayerOffset = viewWindOffset * -0.35f;
            Vector2 ringViewOffset = new Vector2(viewWindOffset.X * 0.85f, viewWindOffset.Y * 0.65f);
            Vector2 lowerRingViewOffset = new Vector2(viewWindOffset.X * 0.55f, viewWindOffset.Y * 0.4f);
            float ringOrbitAngle = spinTime * (0.42f + GridExpansionSpeed * 0.12f) * (1f + windInfluence * 0.55f);
            Vector2 ringOrbitOffset = new Vector2(
                (float)Math.Cos(ringOrbitAngle) * (1.9f + windInfluence * 3.4f),
                (float)Math.Sin(ringOrbitAngle) * (1.05f + windInfluence * 2f)
            );
            float irisSpinAngle = spinTime * (0.95f + RainbowSpeed * 0.22f) * WindIrisSpinStrength * (1f + windInfluence * 1.9f);

            Color baseColor = ApplyTint(BackgroundColor);
            Color voidColor = ApplyTint(VoidColor);
            Color overlayColor = ApplyTint(GetOverlayColor(0.18f));
            Color accentColor = ApplyTint(GetAccentColor(0.06f));
            Color secondaryAccentColor = ApplyTint(GetSecondaryAccentColor(0.12f));
            Color highlightColor = ApplyTint(GetHighlightColor(0.24f));
            Color eyeBaseColor = ApplyTint(GetEyeBaseColor(0.32f));
            Color irisColor = ApplyTint(GetIrisColor(0.48f));
            Color burstFlashColor = ApplyTint(GetBurstFlashColor(0.64f));
            Color upperVeilColor = Color.Lerp(baseColor, overlayColor, 0.36f);
            Color lowerVeilColor = Color.Lerp(baseColor, voidColor, 0.58f);

            Draw.Rect(-16f, -16f, ScreenWidth + 32f, ScreenHeight + 32f, baseColor * masterAlpha);

            float overlayAlpha = MathHelper.Clamp(0.08f + intensityFactor * 0.06f + burst * 0.12f, 0f, 0.5f);
            Draw.Rect(-16f, -16f, ScreenWidth + 32f, ScreenHeight + 32f, overlayColor * masterAlpha * overlayAlpha);
            Draw.Rect(-16f, -16f, ScreenWidth + 32f, 56f, upperVeilColor * masterAlpha * MathHelper.Clamp(0.05f + intensityFactor * 0.018f, 0f, 0.11f));
            Draw.Rect(-16f, 128f, ScreenWidth + 32f, 68f, lowerVeilColor * masterAlpha * MathHelper.Clamp(0.07f + intensityFactor * 0.02f, 0f, 0.16f));

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

            DrawLayer(
                camera,
                backgroundLoopTexture,
                new Vector2(0.016f, 0.01f) * tierParallax,
                new Vector2((-4f - CorruptionSpeed * 3f) * tierMotion * effectiveSpinDirection, 0.32f * tierMotion),
                new Vector2((float)Math.Sin(spinTime * 0.18f) * 14f, -18f + (float)Math.Cos(spinTime * 0.27f) * 10f) + deepWindLayerOffset,
                Color.Lerp(overlayColor, accentColor, 0.12f),
                masterAlpha * MathHelper.Clamp((0.04f + intensityFactor * 0.018f + burst * 0.05f) * tierPatternBoost, 0f, 0.11f),
                new Vector2(1.42f, 1.08f) * patternPulse,
                true,
                true
            );

            DrawLayer(
                camera,
                backgroundLoopTexture,
                new Vector2(0.045f, 0.018f) * tierParallax,
                new Vector2((-8f - CorruptionSpeed * 6f - GridExpansionSpeed * 3f) * tierMotion * effectiveSpinDirection, 0.62f * tierMotion),
                new Vector2(0f, 26f + (float)Math.Sin(spinTime * 0.52f) * 8f) + midWindLayerOffset,
                Color.Lerp(overlayColor, accentColor, 0.22f),
                masterAlpha * MathHelper.Clamp((0.06f + intensityFactor * 0.026f + burst * 0.07f) * (0.82f + tierSparkle * 0.14f), 0f, 0.17f),
                new Vector2(1.14f + burst * 0.03f, 0.9f + burst * 0.02f) * (1f + (float)Math.Sin(spinTime * 0.46f) * 0.025f),
                true,
                true
            );

            DrawLayer(
                camera,
                streaksTexture,
                new Vector2(0.09f, 0.024f) * tierParallax,
                new Vector2((-12f - CorruptionSpeed * 8f) * tierMotion * effectiveSpinDirection, 0.38f * tierMotion),
                new Vector2(0f, 18f + (float)Math.Sin(spinTime * 0.58f) * 6f) + midWindLayerOffset,
                Color.Lerp(secondaryAccentColor, highlightColor, 0.24f),
                masterAlpha * MathHelper.Clamp((0.08f + intensityFactor * 0.03f + burst * 0.08f) * (0.8f + tierMotion * 0.16f), 0f, 0.28f),
                Vector2.One,
                true,
                true
            );

            DrawLayer(
                camera,
                backgroundLoopTexture,
                new Vector2(0.12f, 0.02f) * tierParallax,
                new Vector2((-14f - GridExpansionSpeed * 12f) * tierMotion * effectiveSpinDirection, 0f),
                new Vector2(0f, -72f + (float)Math.Sin(spinTime * 0.7f) * 5f) + surfaceWindLayerOffset,
                Color.Lerp(secondaryAccentColor, highlightColor, 0.18f),
                masterAlpha * MathHelper.Clamp((0.05f + RainbowEdgeIntensity * 0.035f + burst * 0.05f) * (0.9f + tierSparkle * 0.08f), 0f, 0.12f),
                new Vector2(1.28f, 0.54f) * (1f + (float)Math.Sin(spinTime * 0.4f) * 0.02f),
                true,
                false
            );

            DrawLayer(
                camera,
                starsDenseTexture,
                new Vector2(0.03f, 0.018f) * tierParallax,
                new Vector2(-4f * tierMotion * effectiveSpinDirection, 0.55f * tierMotion),
                deepWindLayerOffset * 0.6f,
                Color.Lerp(highlightColor, secondaryAccentColor, 0.32f),
                masterAlpha * MathHelper.Clamp((0.14f + intensityFactor * 0.035f + burst * 0.1f) * (0.88f + tierSparkle * 0.14f), 0f, 0.56f),
                Vector2.One,
                true,
                true
            );

            DrawLayer(
                camera,
                starsSparseTexture,
                new Vector2(0.06f, 0.03f) * tierParallax,
                new Vector2(-9f * tierMotion * effectiveSpinDirection, 1.05f * tierMotion),
                midWindLayerOffset * 0.8f,
                Color.Lerp(highlightColor, secondaryAccentColor, 0.15f),
                masterAlpha * MathHelper.Clamp((0.22f + intensityFactor * 0.05f + burst * 0.14f) * tierSparkle, 0f, 0.76f),
                Vector2.One,
                true,
                true
            );

            float eyeBandY = GetTierEyeBandBaseY() + (float)Math.Sin(spinTime * (0.45f + CorruptionSpeed * 0.18f) * tierMotion) * (2.5f + tierSparkle * 1.2f);
            Vector2 eyesOffset = GetLayerOffset(camera, eyesBandBaseTexture, new Vector2(0.14f, 0.035f) * tierParallax, new Vector2((-18f - CorruptionSpeed * 10f) * tierMotion * effectiveSpinDirection, 0f), new Vector2(viewWindOffset.X * 0.16f, eyeBandY + viewWindOffset.Y * 0.05f), Vector2.One, true, false);
            DrawRepeated(
                eyesBandBaseTexture,
                eyesOffset,
                eyeBaseColor,
                masterAlpha * MathHelper.Clamp((0.16f + intensityFactor * 0.05f) * GetTierEyeBandAlphaBoost(), 0f, 0.56f),
                Vector2.One,
                true,
                false
            );

            DrawRepeated(
                eyesBandIrisTexture,
                eyesOffset + new Vector2((float)Math.Sin(spinTime * (1.25f + RainbowSpeed * 0.25f) * tierMotion) * (4f + burst * 8f + tierSparkle * 1.5f), 0f),
                irisColor,
                masterAlpha * MathHelper.Clamp((0.14f + RainbowEdgeIntensity * 0.1f + burst * 0.1f) * GetTierEyeBandAlphaBoost(), 0f, 0.68f),
                Vector2.One,
                true,
                false
            );

            DrawLayer(
                camera,
                topStripTexture,
                new Vector2(0.16f, 0.025f) * tierParallax,
                new Vector2((-22f - GridExpansionSpeed * 14f) * tierMotion * effectiveSpinDirection, 0f),
                new Vector2(surfaceWindLayerOffset.X * 0.35f, 12f + viewWindOffset.Y * 0.05f),
                highlightColor,
                masterAlpha * MathHelper.Clamp((0.12f + RainbowEdgeIntensity * 0.08f + burst * 0.06f) * (0.92f + tierSparkle * 0.08f), 0f, 0.42f),
                Vector2.One,
                true,
                false
            );

            Vector2 lowerEyesScale = new Vector2(1.08f, 1f);
            float lowerEyeBandY = ringCenterY + 30f + (float)Math.Sin(spinTime * (0.34f + CorruptionSpeed * 0.12f) * tierMotion + 1.7f) * (3f + burst * 1.8f);
            Vector2 lowerEyesOffset = GetLayerOffset(camera, eyesBandBaseTexture, new Vector2(0.09f, 0.022f) * tierParallax, new Vector2((-12f - CorruptionSpeed * 7f) * tierMotion * effectiveSpinDirection, 0f), new Vector2(viewWindOffset.X * 0.1f, lowerEyeBandY + viewWindOffset.Y * 0.04f), lowerEyesScale, true, false);
            DrawRepeated(
                eyesBandBaseTexture,
                lowerEyesOffset,
                Color.Lerp(eyeBaseColor, accentColor, 0.18f),
                masterAlpha * MathHelper.Clamp((0.08f + intensityFactor * 0.025f + burst * 0.04f) * GetTierEyeBandAlphaBoost(), 0f, 0.32f),
                lowerEyesScale,
                true,
                false
            );

            DrawRepeated(
                eyesBandIrisTexture,
                lowerEyesOffset + new Vector2((float)Math.Sin(spinTime * (1.1f + RainbowSpeed * 0.18f) * tierMotion + 0.8f) * (6f + burst * 6f + tierSparkle), 0f),
                irisColor,
                masterAlpha * MathHelper.Clamp((0.07f + RainbowEdgeIntensity * 0.07f + burst * 0.08f) * GetTierEyeBandAlphaBoost(), 0f, 0.38f),
                lowerEyesScale,
                true,
                false
            );

            Vector2 ringParallax = new Vector2(camera.X * 0.11f * scrollMultiplier.X * tierParallax, camera.Y * 0.072f * scrollMultiplier.Y * tierParallax);
            Vector2 ringCenter = new Vector2(ScreenWidth * 0.5f, ringCenterY) + ringViewOffset + ringOrbitOffset - ringParallax * 0.5f;
            float ringAlpha = masterAlpha * MathHelper.Clamp(0.26f + intensityFactor * 0.08f + RainbowEdgeIntensity * 0.06f + burst * 0.18f, 0f, 0.88f);
            Vector2 irisCenter = new Vector2(ScreenWidth * 0.5f, membraneY) + ringViewOffset * 0.76f + ringOrbitOffset * 0.62f - ringParallax * 0.42f;
            Vector2 irisSpinOffset = Calc.AngleToVector(irisSpinAngle, 1f) * (1.4f + windInfluence * 4.2f * WindIrisSpinStrength + burst * 1.8f);

            DrawCenteredRotated(
                eyesBandBaseTexture,
                irisCenter,
                Color.Lerp(eyeBaseColor, secondaryAccentColor, 0.16f),
                masterAlpha * MathHelper.Clamp(0.12f + intensityFactor * 0.035f + burst * 0.06f, 0f, 0.3f),
                new Vector2(voidScale * 1.18f * membranePulse, voidScale * 0.62f * membranePulse),
                spinTime * 0.08f
            );

            DrawCenteredRotated(
                eyesBandBaseTexture,
                irisCenter,
                Color.Lerp(eyeBaseColor, highlightColor, 0.14f),
                masterAlpha * MathHelper.Clamp(0.11f + intensityFactor * 0.035f + burst * 0.05f, 0f, 0.28f),
                new Vector2(voidScale * 0.62f, voidScale * 0.36f),
                irisSpinAngle * 0.14f
            );

            DrawCenteredRotated(
                eyesBandBaseTexture,
                irisCenter,
                Color.Lerp(overlayColor, accentColor, 0.22f),
                masterAlpha * MathHelper.Clamp(0.06f + intensityFactor * 0.025f + windInfluence * 0.08f, 0f, 0.18f),
                new Vector2(voidScale * 0.46f, voidScale * 0.18f),
                -irisSpinAngle * 0.3f
            );

            DrawCenteredRotated(
                eyesBandIrisTexture,
                irisCenter + irisSpinOffset,
                irisColor,
                masterAlpha * MathHelper.Clamp(0.18f + RainbowEdgeIntensity * 0.08f + windInfluence * 0.12f + burst * 0.08f, 0f, 0.42f),
                new Vector2(voidScale * 0.82f, voidScale * 0.82f),
                irisSpinAngle
            );

            DrawCenteredRotated(
                eyesBandIrisTexture,
                irisCenter - irisSpinOffset * 0.55f,
                Color.Lerp(highlightColor, irisColor, 0.35f),
                masterAlpha * MathHelper.Clamp(0.07f + windInfluence * 0.08f + burst * 0.04f, 0f, 0.18f),
                new Vector2(voidScale * 0.92f, voidScale * 0.92f),
                -irisSpinAngle * 0.65f
            );

            DrawCentered(
                ringsCenterTexture,
                ringCenter + new Vector2(0f, 3f),
                Color.Lerp(voidColor, accentColor, 0.16f),
                ringAlpha * 0.22f,
                Vector2.One * (voidScale * tierRingScale * 1.28f * (1f + burst * 0.04f)),
                new Vector2(0.5f, 0.5f)
            );

            DrawCentered(
                ringsCenterTexture,
                ringCenter + ringParallax * 0.08f + new Vector2((float)Math.Cos(spinTime * 0.8f * tierMotion) * (1.2f + windInfluence * 1.2f), (float)Math.Sin(spinTime * 0.8f * tierMotion) * (2f + windInfluence * 1.3f)),
                Color.Lerp(secondaryAccentColor, highlightColor, 0.32f),
                ringAlpha * 0.34f,
                Vector2.One * (voidScale * tierRingScale * (1.1f + burst * 0.08f) * ringPulse),
                new Vector2(0.5f, 0.5f)
            );

            DrawCentered(
                ringsCenterTexture,
                ringCenter,
                accentColor,
                ringAlpha,
                Vector2.One * (voidScale * tierRingScale * ringPulse),
                new Vector2(0.5f, 0.5f)
            );

            DrawCentered(
                ringsBottomTexture,
                new Vector2(ScreenWidth * 0.5f, lowerRingY) + lowerRingViewOffset + ringOrbitOffset * 0.35f - ringParallax * 0.68f,
                Color.Lerp(accentColor, highlightColor, 0.45f),
                ringAlpha,
                Vector2.One * (voidScale * tierRingScale * 1.14f * (1f + burst * 0.05f)),
                new Vector2(0.5f, 1f)
            );

            DrawCentered(
                ringsBottomTexture,
                new Vector2(ScreenWidth * 0.5f, lowerRingY + 4f) + lowerRingViewOffset + ringOrbitOffset * 0.22f - ringParallax * 0.74f,
                Color.Lerp(voidColor, accentColor, 0.18f),
                ringAlpha * 0.24f,
                Vector2.One * (voidScale * tierRingScale * 1.28f * (1f + burst * 0.04f)),
                new Vector2(0.5f, 1f)
            );
        }

        private static MTexture TryGetOptionalTexture(string path)
        {
            try
            {
                return GFX.Game[path];
            }
            catch
            {
                return null;
            }
        }

        private static MTexture TryGetTexture(string path)
        {
            try
            {
                return GFX.Game[path];
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ/ElsTrueFinalBackdrop", $"Missing backdrop texture '{path}': {ex.Message}");
                return null;
            }
        }

        private float BurstStrength()
        {
            if (burstTimer <= 0f)
                return 0f;

            return Ease.CubeOut(burstTimer / BurstDuration);
        }

        private void UpdateWindSystem(Level level)
        {
            targetWind = level != null ? level.Wind : Vector2.Zero;
            currentWind = Vector2.Lerp(currentWind, targetWind, Engine.DeltaTime * 3f);
            windInfluence = MathHelper.Clamp(currentWind.Length() / 800f, 0f, 1f);

            if (Math.Abs(currentWind.X) > 50f)
                targetWindDirection = currentWind.X < 0f ? 1f : -1f;
            else if (Math.Abs(currentWind.Y) > 50f)
                targetWindDirection = currentWind.Y < 0f ? 1f : -1f;
            else
                targetWindDirection = SpinDirection;

            windDirection = Calc.Approach(windDirection, targetWindDirection, Engine.DeltaTime * windTransitionSpeed);

            Vector2 targetViewOffset = Vector2.Zero;
            if (WindAffectsView && WindViewStrength > 0f)
            {
                targetViewOffset = new Vector2(currentWind.X * 0.09f, currentWind.Y * 0.07f) * WindViewStrength;
            }

            float lerp = 1f - (float)Math.Pow(0.01f, Engine.DeltaTime * windTransitionSpeed);
            windViewOffset += (targetViewOffset - windViewOffset) * lerp;
        }

        private Color GetOverlayColor(float offset)
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkOverlayColor, PinkAccentColor, 0.2f + ((float)Math.Sin(animationTime * 0.45f + offset) * 0.5f + 0.5f) * 0.16f);

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(StellarrussOverlayColor, SampleStellarrussColor(animationTime * 0.42f + offset), 0.26f);

                default:
                    return Color.Lerp(SoulOverlayColor, SoulAccentColor, 0.12f + ((float)Math.Sin(animationTime * 0.35f + offset) * 0.5f + 0.5f) * 0.08f);
            }
        }

        private Color GetAccentColor(float offset)
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkAccentColor, PinkHighlightColor, 0.28f + ((float)Math.Sin(animationTime * (0.8f + RainbowSpeed * 0.1f) + offset) * 0.5f + 0.5f) * 0.22f);

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return SampleStellarrussColor(animationTime * (0.7f + RainbowSpeed * 0.08f) + offset);

                default:
                    return Color.Lerp(SoulAccentColor, SoulHighlightColor, 0.22f + ((float)Math.Sin(animationTime * 0.65f + offset) * 0.5f + 0.5f) * 0.18f);
            }
        }

        private Color GetSecondaryAccentColor(float offset)
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkHighlightColor, PinkAccentColor, 0.52f + ((float)Math.Sin(animationTime * 0.9f + offset) * 0.5f + 0.5f) * 0.14f);

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(Color.White, SampleStellarrussColor(animationTime * 1.15f + offset), 0.72f);

                default:
                    return Color.Lerp(SoulHighlightColor, SoulAccentColor, 0.4f + ((float)Math.Sin(animationTime * 0.7f + offset) * 0.5f + 0.5f) * 0.12f);
            }
        }

        private Color GetHighlightColor(float offset)
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkHighlightColor, PinkAccentColor, 0.12f + ((float)Math.Sin(animationTime * 0.55f + offset) * 0.5f + 0.5f) * 0.14f);

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(Color.White, SampleStellarrussColor(animationTime * (0.95f + RainbowSpeed * 0.12f) + offset), 0.52f);

                default:
                    return Color.Lerp(SoulEyeColor, SoulAccentColor, 0.08f + ((float)Math.Sin(animationTime * 0.5f + offset) * 0.5f + 0.5f) * 0.12f);
            }
        }

        private Color GetEyeBaseColor(float offset)
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(Color.White, PinkHighlightColor, 0.28f);

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(Color.White, SampleStellarrussColor(animationTime * 0.6f + offset), 0.16f);

                default:
                    return SoulEyeColor;
            }
        }

        private Color GetIrisColor(float offset)
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkAccentColor, PinkIrisColor, 0.38f + ((float)Math.Sin(animationTime * (1f + RainbowSpeed * 0.12f) + offset) * 0.5f + 0.5f) * 0.22f);

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return SampleStellarrussColor(animationTime * (1.12f + RainbowSpeed * 0.15f) + offset);

                default:
                    return Color.Lerp(SoulAccentColor, SoulIrisColor, 0.28f + ((float)Math.Sin(animationTime * 0.85f + offset) * 0.5f + 0.5f) * 0.18f);
            }
        }

        private Color GetBurstFlashColor(float offset)
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return Color.Lerp(PinkHighlightColor, PinkAccentColor, 0.36f + ((float)Math.Sin(animationTime * 1.1f + offset) * 0.5f + 0.5f) * 0.12f);

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return Color.Lerp(Color.White, SampleStellarrussColor(animationTime * 1.4f + offset), 0.62f);

                default:
                    return Color.Lerp(SoulHighlightColor, SoulIrisColor, 0.32f + ((float)Math.Sin(animationTime * 0.95f + offset) * 0.5f + 0.5f) * 0.08f);
            }
        }

        private float GetTierMotionMultiplier()
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return 1.06f;

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return 1.24f;

                default:
                    return 0.92f;
            }
        }

        private float GetTierParallaxMultiplier()
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return 1.02f;

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return 1.16f;

                default:
                    return 0.9f;
            }
        }

        private float GetTierSparkleBoost()
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return 1.04f;

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return 1.22f;

                default:
                    return 0.9f;
            }
        }

        private float GetTierRingScaleBoost()
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return 1.02f;

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return 1.08f;

                default:
                    return 0.96f;
            }
        }

        private float GetTierEyeBandBaseY()
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return 44f;

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    return 40f;

                default:
                    return 46f;
            }
        }

        private float GetTierEyeBandAlphaBoost()
        {
            switch (currentTier)
            {
                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    return 1.08f;

                case global::Celeste.Entities.SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
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

        private void DrawLayer(Vector2 camera, MTexture texture, Vector2 parallax, Vector2 drift, Vector2 baseOffset, Color color, float alpha, Vector2 scale, bool repeatX, bool repeatY)
        {
            DrawRepeated(
                texture,
                GetLayerOffset(camera, texture, parallax, drift, baseOffset, scale, repeatX, repeatY),
                color,
                alpha,
                scale,
                repeatX,
                repeatY
            );
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

        private void DrawCenteredRotated(MTexture texture, Vector2 position, Color color, float alpha, Vector2 scale, float rotation)
        {
            if (texture == null || alpha <= 0.001f)
                return;

            Vector2 drawScale = new Vector2(
                (flipX ? -1f : 1f) * Math.Abs(scale.X),
                (flipY ? -1f : 1f) * Math.Abs(scale.Y)
            );

            texture.DrawCentered(position, color * alpha, drawScale, rotation);
        }

        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}