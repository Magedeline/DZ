#nullable enable

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Draws a swirling "Astral Birth Void" halo above the normal moon on the
    /// 3D overworld mountain.  Uses three layered textures from
    /// <c>Graphics/Atlases/Mountain</c>:
    ///   - <c>void.png</c>       – deep space nebula background
    ///   - <c>spacestars.png</c>  – pink star particle overlay
    ///   - <c>starstream.png</c>  – faint cosmic glow layer
    /// 
    /// Each layer is tinted with a slowly cycling rainbow hue that shifts
    /// through deep indigo → violet → magenta → rose → teal and back,
    /// giving the overworld sky a cosmic, otherworldly feel.
    /// </summary>
    [HotReloadable]
    public class AstralBirthVoidOverlay : Entity
    {
        // ── Configuration ───────────────────────────────────────
        /// <summary>Where the void centre sits (world space).</summary>
        public new Vector3 Position { get; set; } = Vector3.Zero;

        /// <summary>Extra offset applied on top of Position (e.g. above the moon).</summary>
        public Vector3 Offset { get; set; } = new Vector3(0f, 2.5f, 0f);

        /// <summary>Base radius of the void disc.</summary>
        public float Radius { get; set; } = 1.2f;

        /// <summary>If true, the effect is visible and fading in.</summary>
        public bool Show { get; set; } = true;

        // ── Deep-space rainbow palette ──────────────────────────
        /// <summary>Base deep-space colour (dark indigo from void.png).</summary>
        public Color DeepSpaceColor { get; set; } = new Color(15, 8, 45, 255);
        /// <summary>Nebula cloud tint (purple-blue from void.png).</summary>
        public Color NebulaColor { get; set; } = new Color(60, 20, 100, 255);
        /// <summary>Star particle pink glow (from spacestars.png).</summary>
        public Color StarPinkColor { get; set; } = new Color(255, 100, 180, 255);
        /// <summary>Bright core white-pink.</summary>
        public Color CoreColor { get; set; } = new Color(255, 210, 255, 255);

        /// <summary>Speed of the rainbow hue cycle (full rotations per second).</summary>
        public float RainbowSpeed { get; set; } = 0.06f;

        /// <summary>How much rainbow tint mixes into each layer (0 = none, 1 = full).</summary>
        public float RainbowIntensity { get; set; } = 0.35f;

        // ── Internals ───────────────────────────────────────────
        private readonly MountainRenderer renderer;

        // Background space layers (void.png)
        private const int VOID_LAYERS = 3;
        private readonly Billboard?[] voidBillboards = new Billboard?[VOID_LAYERS];
        private readonly float[] layerRotSpeeds = new float[VOID_LAYERS];
        private readonly float[] layerPhases    = new float[VOID_LAYERS];
        private readonly float[] layerScales    = new float[VOID_LAYERS];

        // Star particle layer (spacestars.png)
        private const int STAR_COUNT = 18;
        private readonly Billboard?[] starBillboards = new Billboard?[STAR_COUNT];
        private readonly float[]  starAngles   = new float[STAR_COUNT];
        private readonly float[]  starRadii    = new float[STAR_COUNT];
        private readonly float[]  starSpeeds   = new float[STAR_COUNT];
        private readonly float[]  starPhases   = new float[STAR_COUNT];

        // Glow stream layer (starstream.png)
        private const int STREAM_LAYERS = 3;
        private readonly Billboard?[] streamBillboards = new Billboard?[STREAM_LAYERS];
        private readonly float[] streamPhases = new float[STREAM_LAYERS];

        // Core bright spot
        private Billboard? coreBillboard;

        private float alpha;
        private float globalTime;
        private float pulsePhase;
        private float rainbowHue;

        // ── Rainbow helper ──────────────────────────────────────
        /// <summary>Convert HSV (h 0-360, s/v 0-1) to an XNA Color.</summary>
        private static Color HsvToColor(float h, float s, float v, float a = 1f)
        {
            h %= 360f;
            if (h < 0f) h += 360f;
            float c = v * s;
            float x = c * (1f - Math.Abs(h / 60f % 2f - 1f));
            float m = v - c;
            float r, g, b;
            if      (h < 60f)  { r = c; g = x; b = 0; }
            else if (h < 120f) { r = x; g = c; b = 0; }
            else if (h < 180f) { r = 0; g = c; b = x; }
            else if (h < 240f) { r = 0; g = x; b = c; }
            else if (h < 300f) { r = x; g = 0; b = c; }
            else               { r = c; g = 0; b = x; }
            return new Color(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255),
                (byte)(a * 255));
        }

        // ─────────────────────────────────────────────────────────
        public AstralBirthVoidOverlay(MountainRenderer mountainRenderer)
        {
            renderer = mountainRenderer ?? throw new ArgumentNullException(nameof(mountainRenderer));
            Depth = -9400; // just behind MountainMoonAlt (-9500)

            // Seed void layer parameters
            for (int i = 0; i < VOID_LAYERS; i++)
            {
                float t = (float)i / VOID_LAYERS;
                layerRotSpeeds[i] = Calc.Random.Range(3f, 14f) * (Calc.Random.Chance(0.5f) ? 1f : -1f);
                layerPhases[i]    = Calc.Random.NextFloat() * MathHelper.TwoPi;
                layerScales[i]    = 1f + t * 0.5f;
            }

            // Seed star parameters – more particles, wider spread
            for (int i = 0; i < STAR_COUNT; i++)
            {
                starAngles[i]  = Calc.Random.NextFloat() * MathHelper.TwoPi;
                starRadii[i]   = Calc.Random.Range(0.5f, 1.6f);
                starSpeeds[i]  = Calc.Random.Range(4f, 20f) * (Calc.Random.Chance(0.5f) ? 1f : -1f);
                starPhases[i]  = Calc.Random.NextFloat() * MathHelper.TwoPi;
            }

            // Seed stream layer parameters
            for (int i = 0; i < STREAM_LAYERS; i++)
            {
                streamPhases[i] = Calc.Random.NextFloat() * MathHelper.TwoPi;
            }
        }

        // ─────────────────────────────────────────────────────────
        // Content loading
        // ─────────────────────────────────────────────────────────
        public void LoadContent()
        {
            try
            {
                // ── Layer 1: Deep space nebula (moon_alt.png) ───────
                MTexture? voidTex = MTN.Mountain.Has("moon_alt") ? MTN.Mountain["moon_alt"] : null;
                if (voidTex == null)
                {
                    IngesteLogger.Warn("AstralBirthVoidOverlay: 'moon_alt' texture not found");
                    // can still proceed with stars
                }

                if (voidTex != null)
                {
                    for (int i = 0; i < VOID_LAYERS; i++)
                    {
                        float t = (float)i / VOID_LAYERS;
                        Color layerTint = Color.Lerp(DeepSpaceColor, NebulaColor, t);
                        layerTint.A = (byte)(100 - i * 20);

                        var bb = new Billboard(voidTex, Vector3.Zero)
                        {
                            Scale = Vector2.One * Radius * layerScales[i],
                            Color = layerTint,
                        };
                        voidBillboards[i] = bb;
                        Add(bb);
                    }
                }

                // ── Layer 2: Star particles (spacestars.png) ────
                MTexture? starTex = MTN.Mountain.Has("spacestars") ? MTN.Mountain["spacestars"] : voidTex;
                if (starTex != null)
                {
                    for (int i = 0; i < STAR_COUNT; i++)
                    {
                        // Alternate between pink and white-ish stars like the source image
                        Color starBase = Calc.Random.Chance(0.6f)
                            ? StarPinkColor   // pink glow
                            : CoreColor;      // white-hot
                        starBillboards[i] = new Billboard(starTex, Vector3.Zero)
                        {
                            Scale = Vector2.One * Calc.Random.Range(0.03f, 0.10f),
                            Color = starBase * Calc.Random.Range(0.5f, 0.9f),
                        };
                        Add(starBillboards[i]);
                    }
                }

                // ── Layer 3: Faint glow streams (starstream.png) ─
                MTexture? streamTex = MTN.Mountain.Has("starstream") ? MTN.Mountain["starstream"]
                                    : MTN.Mountain.Has("space")      ? MTN.Mountain["space"]
                                    : null;
                if (streamTex != null)
                {
                    for (int i = 0; i < STREAM_LAYERS; i++)
                    {
                        float t = (float)i / STREAM_LAYERS;
                        streamBillboards[i] = new Billboard(streamTex, Vector3.Zero)
                        {
                            Scale = Vector2.One * Radius * (1.2f + t * 0.4f),
                            Color = new Color(255, 200, 255, 15),  // very faint pink-white
                        };
                        Add(streamBillboards[i]);
                    }
                }

                // ── Core: bright moon_alt at centre ─────────────
                if (MTN.Mountain.Has("moon_alt"))
                {
                    coreBillboard = new Billboard(MTN.Mountain["moon_alt"], Vector3.Zero)
                    {
                        Scale = Vector2.One * Radius * 0.25f,
                        Color = CoreColor * 0.6f,
                    };
                    Add(coreBillboard);
                }

                IngesteLogger.Info("AstralBirthVoidOverlay: deep-space rainbow content loaded");
            }
            catch (Exception ex)
            {
                IngesteLogger.Warn($"AstralBirthVoidOverlay: LoadContent failed – {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────
        // Update
        // ─────────────────────────────────────────────────────────
        public override void Update()
        {
            base.Update();

            float dt = Engine.DeltaTime;
            globalTime += dt;

            // Visibility fade
            alpha = Calc.Approach(alpha, Show ? 1f : 0f, dt * 2.5f);

            // Pulse
            pulsePhase += dt * 0.6f;
            float pulse = 1f + 0.06f * (float)Math.Sin(pulsePhase * MathHelper.TwoPi);

            // ── Rainbow hue cycle ───────────────────────────────
            rainbowHue += dt * RainbowSpeed * 360f;  // degrees
            if (rainbowHue > 360f) rainbowHue -= 360f;

            // Current rainbow colour (high saturation, moderate value so it stays "spacey")
            Color rainbow = HsvToColor(rainbowHue, 0.7f, 0.8f);

            Vector3 centre = Position + Offset;

            // ── Update void (nebula) layers ─────────────────────
            for (int i = 0; i < VOID_LAYERS; i++)
            {
                if (voidBillboards[i] == null) continue;

                layerPhases[i] += MathHelper.ToRadians(layerRotSpeeds[i]) * dt;
                float angle = layerPhases[i];
                float s = Radius * layerScales[i] * pulse;

                // Per-layer hue offset so each layer shows a different rainbow band
                float layerHue = (rainbowHue + i * 72f) % 360f;  // 72° apart = 5 colours
                Color layerRainbow = HsvToColor(layerHue, 0.6f, 0.65f);

                float t = (float)i / VOID_LAYERS;
                Color baseColor = Color.Lerp(DeepSpaceColor, NebulaColor, t);
                Color finalColor = Color.Lerp(baseColor, layerRainbow, RainbowIntensity);
                byte a = (byte)((100 - i * 20) * alpha);

                Vector3 layerOffset = new Vector3(
                    (float)Math.Sin(angle * 0.3f) * 0.15f * (i + 1),
                    (float)Math.Cos(angle * 0.5f) * 0.08f * (i + 1),
                    (float)Math.Sin(angle * 0.7f) * 0.10f * (i + 1)
                );

                voidBillboards[i]!.Position = centre + layerOffset;
                voidBillboards[i]!.Scale = Vector2.One * s;
                voidBillboards[i]!.Color = new Color(finalColor.R, finalColor.G, finalColor.B, a);
            }

            // ── Update star particles (pink glow with rainbow twinkle) ──
            for (int i = 0; i < STAR_COUNT; i++)
            {
                if (starBillboards[i] == null) continue;

                starAngles[i] += MathHelper.ToRadians(starSpeeds[i]) * dt;
                float angle = starAngles[i];
                float r = Radius * starRadii[i] * pulse;

                // 3D orbit
                float tiltFactor = 0.4f + 0.3f * (float)Math.Sin(starPhases[i]);
                Vector3 starPos = centre + new Vector3(
                    (float)Math.Cos(angle) * r,
                    (float)Math.Sin(angle) * r * tiltFactor,
                    (float)Math.Sin(angle * 0.7f + starPhases[i]) * r * 0.3f
                );
                starBillboards[i]!.Position = starPos;

                // Twinkle + per-star rainbow hue shift
                float twinkle = 0.5f + 0.5f * (float)Math.Sin(globalTime * 3.5f + starPhases[i]);
                float starHue = (rainbowHue + i * 10f) % 360f;
                Color starRainbow = HsvToColor(starHue, 0.5f, 1f);
                Color starBase = Color.Lerp(StarPinkColor, CoreColor, twinkle);
                Color starFinal = Color.Lerp(starBase, starRainbow, RainbowIntensity * 0.5f);
                starBillboards[i]!.Color = starFinal * (alpha * Calc.Random.Range(0.5f, 0.9f));
            }

            // ── Update glow stream layers ───────────────────────
            for (int i = 0; i < STREAM_LAYERS; i++)
            {
                if (streamBillboards[i] == null) continue;

                streamPhases[i] += dt * (0.8f + i * 0.3f);
                float sAngle = streamPhases[i];
                float t = (float)i / STREAM_LAYERS;

                // Gentle drift
                Vector3 streamOffset = new Vector3(
                    (float)Math.Sin(sAngle * 0.4f) * 0.3f,
                    (float)Math.Cos(sAngle * 0.6f) * 0.2f,
                    (float)Math.Sin(sAngle * 0.2f) * 0.15f
                );
                streamBillboards[i]!.Position = centre + streamOffset;
                streamBillboards[i]!.Scale = Vector2.One * Radius * (1.2f + t * 0.4f + 0.05f * (float)Math.Sin(sAngle));

                // Rainbow-tinted faint glow
                float streamHue = (rainbowHue + 120f + i * 60f) % 360f;
                Color streamRainbow = HsvToColor(streamHue, 0.3f, 1f, 0.12f * alpha);
                streamBillboards[i]!.Color = streamRainbow;
            }

            // ── Core ────────────────────────────────────────────
            if (coreBillboard != null)
            {
                float coreScale = Radius * 0.3f * (1f + 0.1f * (float)Math.Sin(globalTime * 3f));
                coreBillboard.Position = centre;
                coreBillboard.Scale = Vector2.One * coreScale;
                // Core gets a subtle rainbow shimmer
                Color coreRainbow = Color.Lerp(CoreColor, rainbow, 0.15f);
                coreBillboard.Color = coreRainbow * (alpha * (0.75f + 0.25f * (float)Math.Sin(globalTime * 2f)));
            }
        }

        // ─────────────────────────────────────────────────────────
        // Cleanup
        // ─────────────────────────────────────────────────────────
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
    }
}
