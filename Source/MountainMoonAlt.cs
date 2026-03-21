#nullable enable

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Renders an alternative moon (moon.obj + moon_alt.png) above the normal
    /// overworld moon in the 3D mountain scene.  Pairs with
    /// <see cref="AstralBirthVoidOverlay"/> which draws the void halo on top.
    /// 
    /// Assets expected:
    ///   Mountain/Maggy/Celeste/moon.obj   – OBJ mesh
    ///   Graphics/Atlases/Mountain/moon_alt – texture (packed by Everest)
    ///   Graphics/Atlases/Mountain/void     – void halo texture
    /// </summary>
    [HotReloadable]
    public class MountainMoonAlt : Entity
    {
        // ── References ──────────────────────────────────────────
        private readonly MountainRenderer renderer;

        // ── 3D mesh / texture ───────────────────────────────────
        private ObjModel? moonModel;
        private VirtualRenderTarget? moonRt;

        // ── Billboard fallback (used when OBJ fails to load) ───
        private Billboard? moonBillboard;

        // ── Positioning ─────────────────────────────────────────
        /// <summary>Offset *relative* to the camera target so it floats above in the sky.</summary>
        public Vector3 Offset { get; set; } = new Vector3(0f, 1.5f, 0f);

        /// <summary>Additional world-space position adjustment.</summary>
        public new Vector3 Position { get; set; } = Vector3.Zero;

        /// <summary>Scale multiplier for the alt-moon mesh / billboard.</summary>
        public float MoonScale { get; set; } = 1.25f;

        // ── Animation ───────────────────────────────────────────
        private float rotationAngle;
        private float bobPhase;

        /// <summary>Degrees-per-second rotation around the Y axis.</summary>
        public float RotationSpeed { get; set; } = 8f;

        /// <summary>How far the moon bobs vertically (world units).</summary>
        public float BobAmplitude { get; set; } = 0.15f;

        /// <summary>Bob oscillation speed.</summary>
        public float BobFrequency { get; set; } = 1.2f;

        // ── Alpha / visibility ──────────────────────────────────
        private float alpha;
        public bool Show { get; set; } = true;

        /// <summary>Tint colour blended onto the moon texture.</summary>
        public Color Tint { get; set; } = new Color(200, 180, 255, 255); // soft lavender glow

        // ── Glow ring around the moon ───────────────────────────
        private Billboard? glowRing;
        private Billboard? outerHalo;
        private float glowPulsePhase;

        // ── Rainbow colour cycling ──────────────────────────────
        private float rainbowHue;

        /// <summary>Speed of the rainbow hue cycle (full rotations per second).</summary>
        public float RainbowSpeed { get; set; } = 0.08f;

        /// <summary>How much rainbow tint mixes into the glow (0..1).</summary>
        public float RainbowIntensity { get; set; } = 0.4f;

        // ── Rainbow helper ──────────────────────────────────────
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
        // Construction
        // ─────────────────────────────────────────────────────────
        public MountainMoonAlt(MountainRenderer mountainRenderer)
        {
            renderer = mountainRenderer ?? throw new ArgumentNullException(nameof(mountainRenderer));
            Depth = -9500; // behind marker, in front of skybox
        }

        // ─────────────────────────────────────────────────────────
        // Load / setup
        // ─────────────────────────────────────────────────────────
        public void LoadContent()
        {
            try
            {
                // ── Try loading the OBJ model ───────────────────
                string objPath = "Mountain/Maggy/Celeste/moon";
                if (Everest.Content.TryGet(objPath, out var objAsset))
                {
                    moonModel = ObjModel.Create(objPath);
                    IngesteLogger.Info("MountainMoonAlt: moon.obj loaded");
                }
                else
                {
                    IngesteLogger.Warn("MountainMoonAlt: moon.obj not found – falling back to billboard");
                }

                // ── Always prepare a billboard as fallback / halo ──
                if (MTN.Mountain.Has("moon_alt"))
                {
                    var tex = MTN.Mountain["moon_alt"];
                    moonBillboard = new Billboard(tex, Vector3.Zero)
                    {
                        Scale = Vector2.One * MoonScale,
                        Color = Tint
                    };
                    Add(moonBillboard);
                    IngesteLogger.Info("MountainMoonAlt: moon_alt billboard created");
                }
                else
                {
                    IngesteLogger.Warn("MountainMoonAlt: moon_alt texture missing from MTN.Mountain atlas");
                }

                // ── Glow ring billboard ─────────────────────────
                if (MTN.Mountain.Has("moon_alt"))
                {
                    var glowTex = MTN.Mountain["moon_alt"];
                    glowRing = new Billboard(glowTex, Vector3.Zero)
                    {
                        Scale = Vector2.One * MoonScale * 1.4f,
                        Color = new Color(Tint.R, Tint.G, Tint.B, (byte)30)
                    };
                    Add(glowRing);
                }

                // ── Outer rainbow halo (uses moon_alt.png for a wider aura) ─
                MTexture? haloTex = MTN.Mountain.Has("moon_alt") ? MTN.Mountain["moon_alt"]
                                  : null;
                if (haloTex != null)
                {
                    outerHalo = new Billboard(haloTex, Vector3.Zero)
                    {
                        Scale = Vector2.One * MoonScale * 1.8f,
                        Color = new Color(60, 20, 100, 12)
                    };
                    Add(outerHalo);
                }
            }
            catch (Exception ex)
            {
                IngesteLogger.Warn($"MountainMoonAlt: LoadContent failed – {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────
        // Update
        // ─────────────────────────────────────────────────────────
        public override void Update()
        {
            base.Update();

            float dt = Engine.DeltaTime;

            // Fade in / out
            alpha = Calc.Approach(alpha, Show ? 1f : 0f, dt * 3f);

            // Rotation (continuous, slow spin)
            rotationAngle += MathHelper.ToRadians(RotationSpeed) * dt;
            if (rotationAngle > MathHelper.TwoPi) rotationAngle -= MathHelper.TwoPi;

            // Gentle bob
            bobPhase += dt * BobFrequency;
            float bobOffset = (float)Math.Sin(bobPhase * MathHelper.TwoPi) * BobAmplitude;

            // Glow pulse
            glowPulsePhase += dt * 0.8f;
            float glowAlpha = 0.3f + 0.15f * (float)Math.Sin(glowPulsePhase * MathHelper.TwoPi);

            // Rainbow hue cycle
            rainbowHue += dt * RainbowSpeed * 360f;
            if (rainbowHue > 360f) rainbowHue -= 360f;
            Color rainbow = HsvToColor(rainbowHue, 0.65f, 0.9f);

            // Compute final 3D position (base + offset + bob)
            Vector3 worldPos = Position + Offset + new Vector3(0f, bobOffset, 0f);

            // ── Update Billboard ────────────────────────────────
            if (moonBillboard != null)
            {
                moonBillboard.Position = worldPos;
                moonBillboard.Scale = Vector2.One * MoonScale;
                // Subtle rainbow shimmer on the moon itself
                Color moonTint = Color.Lerp(Tint, rainbow, RainbowIntensity * 0.3f);
                moonBillboard.Color = new Color(moonTint.R, moonTint.G, moonTint.B, (byte)(alpha * 255));
            }

            if (glowRing != null)
            {
                glowRing.Position = worldPos;
                glowRing.Scale = Vector2.One * MoonScale * (1.5f + 0.1f * (float)Math.Sin(glowPulsePhase * MathHelper.TwoPi));
                // Rainbow glow ring
                Color glowTint = Color.Lerp(Tint, rainbow, RainbowIntensity);
                glowRing.Color = new Color(glowTint.R, glowTint.G, glowTint.B, (byte)(glowAlpha * alpha * 255));
            }

            // ── Outer halo with offset rainbow hue ──────────────
            if (outerHalo != null)
            {
                outerHalo.Position = worldPos;
                float haloScale = MoonScale * (2.3f + 0.2f * (float)Math.Sin(glowPulsePhase * MathHelper.TwoPi * 0.5f));
                outerHalo.Scale = Vector2.One * haloScale;
                // Offset hue by 180° for complementary color
                Color haloRainbow = HsvToColor((rainbowHue + 180f) % 360f, 0.4f, 0.7f, 0.1f * alpha);
                outerHalo.Color = haloRainbow;
            }
        }

        // ─────────────────────────────────────────────────────────
        // Render – draw OBJ model when available
        // ─────────────────────────────────────────────────────────
        public override void Render()
        {
            base.Render();

            if (alpha <= 0.01f) return;

            if (moonModel != null)
            {
                try
                {
                    RenderObjMoon();
                }
                catch (Exception ex)
                {
                    IngesteLogger.Warn($"MountainMoonAlt: OBJ render error – {ex.Message}");
                    // Billboard serves as visual fallback automatically
                }
            }
        }

        private void RenderObjMoon()
        {
            if (moonModel == null) return;

            float bobOffset = (float)Math.Sin(bobPhase * MathHelper.TwoPi) * BobAmplitude;
            Vector3 worldPos = Position + Offset + new Vector3(0f, bobOffset, 0f);

            Matrix world =
                Matrix.CreateScale(MoonScale * 0.04f) *            // OBJ models are usually large
                Matrix.CreateRotationY(rotationAngle) *
                Matrix.CreateTranslation(worldPos);

            // MountainCamera only exposes Position + Target, so build matrices manually
            var cam = renderer.Model.Camera;
            Matrix view = Matrix.CreateLookAt(cam.Position, cam.Target, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(75f), Engine.ViewWidth / (float)Engine.ViewHeight, 0.1f, 800f);

            var device = Engine.Graphics.GraphicsDevice;
            var oldRaster = device.RasterizerState;
            var oldDepth = device.DepthStencilState;
            var oldBlend = device.BlendState;

            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;
            device.BlendState = BlendState.AlphaBlend;

            try
            {
                // Bind moon_alt texture if we have it
                MTexture? tex = MTN.Mountain.Has("moon_alt") ? MTN.Mountain["moon_alt"] : null;
                if (tex != null)
                {
                    device.Textures[0] = tex.Texture.Texture_Safe;
                }

                var effect = new BasicEffect(device)
                {
                    World = world,
                    View = view,
                    Projection = projection,
                    TextureEnabled = tex != null,
                    DiffuseColor = new Vector3(Tint.R / 255f, Tint.G / 255f, Tint.B / 255f),
                    Alpha = alpha,
                    LightingEnabled = true,
                    AmbientLightColor = new Vector3(0.5f, 0.4f, 0.6f),
                };
                effect.DirectionalLight0.Enabled = true;
                effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1f, -0.5f, -1f));
                // Rainbow-tinted directional light
                Color rainbowLight = HsvToColor(rainbowHue, 0.5f, 0.9f);
                effect.DirectionalLight0.DiffuseColor = new Vector3(
                    rainbowLight.R / 255f, rainbowLight.G / 255f, rainbowLight.B / 255f);

                moonModel.Draw(effect);
                effect.Dispose();
            }
            finally
            {
                device.RasterizerState = oldRaster;
                device.DepthStencilState = oldDepth;
                device.BlendState = oldBlend;
            }
        }

        // ─────────────────────────────────────────────────────────
        // Cleanup
        // ─────────────────────────────────────────────────────────
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            moonRt?.Dispose();
            moonRt = null;
        }
    }
}
