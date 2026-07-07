namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/BlackholeRiser")]
    [Tracked]
    [HotReloadable]
    public class BlackholeRiser : Entity
    {
        public enum RiserMode
        {
            Pure,
            Black,
            Rainbow,
            Distortion
        }

        public static ParticleType P_Flare;
        public static ParticleType P_Rainbow;

        private RiserMode mode;
        private float speed;
        private float width;
        private float maxHeight;
        private float currentHeight;
        private float particleTimer;
        private bool glitchy;
        private float glitchTimer;
        private Vector2 glitchOffset;
        private Color[] rainbowColors;
        private int colorIndex;
        private float rainbowTimer;
        private bool rising;
        private float riseDelay;
        private float riseTimer;
        private bool looping;
        private Sprite lavaSprite;

        public BlackholeRiser(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            speed = data.Float("speed", 80f);
            width = data.Width;
            maxHeight = data.Float("maxHeight", 200f);
            riseDelay = data.Float("riseDelay", 1f);
            glitchy = data.Bool("glitchy", true);
            looping = data.Bool("looping", true);

            // New mode option. If omitted, preserve the legacy glitchy appearance.
            string modeStr = data.Attr("mode", "");
            if (Enum.TryParse<RiserMode>(modeStr, true, out RiserMode parsedMode))
            {
                mode = parsedMode;
            }
            else
            {
                mode = glitchy ? RiserMode.Rainbow : RiserMode.Pure;
            }

            currentHeight = 0f;
            rising = false;
            riseTimer = riseDelay;

            Depth = -50;

            Collider = new Hitbox(width, 0f, 0f, 0f);
            Add(new PlayerCollider(OnPlayer));

            InitializeRainbowColors();
            InitializeParticles();
            InitializeLavaSprite();
        }

        private void InitializeRainbowColors()
        {
            rainbowColors = new Color[7]
            {
                Calc.HexToColor("FF0000"), // Red
                Calc.HexToColor("FF7F00"), // Orange
                Calc.HexToColor("FFFF00"), // Yellow
                Calc.HexToColor("00FF00"), // Green
                Calc.HexToColor("0000FF"), // Blue
                Calc.HexToColor("4B0082"), // Indigo
                Calc.HexToColor("9400D3")  // Violet
            };
        }

        private void InitializeLavaSprite()
        {
            lavaSprite = GFX.SpriteBank.Create("lava_bubble");
            string anim = lavaSprite.Has("idle") ? "idle"
                : lavaSprite.Has("rising") ? "rising"
                : lavaSprite.Has("forming") ? "forming" : null;
            if (anim != null)
                lavaSprite.Play(anim);

            lavaSprite.Position = new Vector2(width / 2f - lavaSprite.Width / 2f, -lavaSprite.Height);
            lavaSprite.Visible = false;
            Add(lavaSprite);
        }

        private static void InitializeParticles()
        {
            if (P_Flare == null)
            {
                P_Flare = new ParticleType
                {
                    Source = GFX.Game["particles/blob"],
                    Color = Color.Purple,
                    Color2 = Color.Black,
                    ColorMode = ParticleType.ColorModes.Blink,
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 0.6f,
                    LifeMax = 1.2f,
                    Size = 0.8f,
                    SizeRange = 0.4f,
                    SpeedMin = 10f,
                    SpeedMax = 30f,
                    SpeedMultiplier = 0.5f,
                    DirectionRange = (float)Math.PI / 4f,
                    Acceleration = new Vector2(0f, -20f),
                    SpinMin = -2f,
                    SpinMax = 2f
                };
            }

            if (P_Rainbow == null)
            {
                P_Rainbow = new ParticleType
                {
                    Source = GFX.Game["particles/blob"],
                    Color = Color.White,
                    Color2 = Color.Cyan,
                    ColorMode = ParticleType.ColorModes.Choose,
                    FadeMode = ParticleType.FadeModes.InAndOut,
                    LifeMin = 0.4f,
                    LifeMax = 0.8f,
                    Size = 1f,
                    SizeRange = 0.5f,
                    SpeedMin = 20f,
                    SpeedMax = 50f,
                    DirectionRange = (float)Math.PI * 2f,
                    SpinMin = -3f,
                    SpinMax = 3f
                };
            }
        }

        public override void Update()
        {
            // Handle rise delay
            if (!rising && riseTimer > 0f)
            {
                riseTimer -= Engine.DeltaTime;
                if (riseTimer <= 0f)
                {
                    rising = true;
                    Audio.Play("event:/game/general/thing_booped", Position);
                }
            }

            // Rise up
            if (rising)
            {
                currentHeight = Calc.Approach(currentHeight, maxHeight, speed * Engine.DeltaTime);

                if (currentHeight >= maxHeight)
                {
                    if (looping)
                    {
                        // Reset for next rise
                        currentHeight = 0f;
                        rising = false;
                        riseTimer = riseDelay;
                    }
                }
            }

            // Update collider before components are updated so the PlayerCollider uses the current hitbox
            Collider = new Hitbox(width, currentHeight, 0f, -currentHeight);

            // Glitchy offset
            float glitchAmp = GetGlitchAmplitude();
            if (glitchAmp > 0f)
            {
                glitchTimer += Engine.DeltaTime;
                if (Scene.OnInterval(0.05f))
                {
                    glitchOffset = new Vector2(
                        Calc.Random.Range(-glitchAmp, glitchAmp),
                        Calc.Random.Range(-glitchAmp, glitchAmp)
                    );
                }
            }
            else
            {
                glitchOffset = Vector2.Zero;
            }

            // Rainbow color cycling
            rainbowTimer += Engine.DeltaTime * 5f;
            if (rainbowTimer >= 1f)
            {
                rainbowTimer = 0f;
                colorIndex = (colorIndex + 1) % rainbowColors.Length;
            }

            // Position the lava bubble sprite at the top of the rising column
            if (lavaSprite != null)
            {
                lavaSprite.Visible = currentHeight > 0f;
                if (lavaSprite.Visible)
                {
                    lavaSprite.Position = new Vector2(
                        width / 2f - lavaSprite.Width / 2f,
                        -currentHeight - lavaSprite.Height / 2f
                    ) + glitchOffset;
                }
            }

            base.Update();

            // Emit particles
            if (currentHeight > 0f)
            {
                particleTimer += Engine.DeltaTime;
                if (particleTimer >= 0.08f)
                {
                    particleTimer = 0f;
                    EmitParticles();
                }
            }
        }

        private float GetGlitchAmplitude()
        {
            switch (mode)
            {
                case RiserMode.Pure:
                    return 0f;
                case RiserMode.Distortion:
                    return 6f;
                case RiserMode.Black:
                case RiserMode.Rainbow:
                default:
                    return glitchy ? 3f : 0f;
            }
        }

        private void EmitParticles()
        {
            Level level = Scene as Level;
            if (level == null) return;

            if (mode == RiserMode.Pure)
                return;

            Vector2 topPos = new Vector2(X + width / 2f, Y - currentHeight);

            Color particleColor;
            if (mode == RiserMode.Rainbow || mode == RiserMode.Distortion)
            {
                Color currentColor = rainbowColors[colorIndex];
                Color nextColor = rainbowColors[(colorIndex + 1) % rainbowColors.Length];
                particleColor = Color.Lerp(currentColor, nextColor, rainbowTimer);
            }
            else
            {
                particleColor = Color.Purple;
            }

            ParticleType rainbow = new ParticleType(P_Rainbow)
            {
                Color = particleColor,
                Color2 = Color.Black
            };

            level.ParticlesFG.Emit(rainbow, 1, topPos, Vector2.One * 8f);

            if (glitchy && Calc.Random.Chance(0.3f))
            {
                level.ParticlesFG.Emit(P_Flare, 1, topPos + glitchOffset, Vector2.One * 4f);
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            Vector2 direction = (player.Center - Center).SafeNormalize();
            player.Die(direction * 100f);

            Level level = Scene as Level;
            if (level != null)
            {
                level.Shake(0.3f);
                Audio.Play("event:/game/general/thing_booped", Position);

                // Explosion effect matching the current mode
                Color explosionColor;
                if (mode == RiserMode.Rainbow || mode == RiserMode.Distortion)
                {
                    explosionColor = rainbowColors[colorIndex];
                }
                else if (mode == RiserMode.Black)
                {
                    explosionColor = Color.Purple;
                }
                else
                {
                    explosionColor = Color.Black;
                }

                for (int i = 0; i < 15; i++)
                {
                    ParticleType explosion = new ParticleType(P_Rainbow)
                    {
                        Color = explosionColor,
                        Color2 = Color.Black
                    };
                    level.ParticlesFG.Emit(explosion, Center, (float)Math.PI * 2f);
                }
            }
        }

        public override void Render()
        {
            if (currentHeight <= 0f) return;

            Vector2 renderPos = Position + glitchOffset;

            Color currentColor = rainbowColors[colorIndex];
            Color nextColor = rainbowColors[(colorIndex + 1) % rainbowColors.Length];
            Color rainbowBorder = Color.Lerp(currentColor, nextColor, rainbowTimer);

            float pulse = (float)Math.Sin(glitchTimer * 8f) * 0.3f + 0.7f;

            Color coreColor;
            Color borderColor;
            switch (mode)
            {
                case RiserMode.Pure:
                    coreColor = Color.Black * 0.95f;
                    borderColor = Color.DarkGray * 0.5f;
                    break;
                case RiserMode.Black:
                    coreColor = Color.Black * 0.85f;
                    borderColor = Color.Purple * pulse;
                    break;
                case RiserMode.Rainbow:
                case RiserMode.Distortion:
                default:
                    coreColor = Color.Black * 0.8f;
                    borderColor = rainbowBorder * pulse;
                    break;
            }

            // Draw core (rising column)
            Draw.Rect(renderPos.X, renderPos.Y - currentHeight, width, currentHeight, coreColor);

            // Draw border effect
            float borderWidth = 2f;

            // Top
            Draw.Rect(renderPos.X, renderPos.Y - currentHeight, width, borderWidth, borderColor);
            // Left
            Draw.Rect(renderPos.X, renderPos.Y - currentHeight, borderWidth, currentHeight, borderColor);
            // Right
            Draw.Rect(renderPos.X + width - borderWidth, renderPos.Y - currentHeight, borderWidth, currentHeight, borderColor);

            // Inner glow
            if (mode == RiserMode.Rainbow || mode == RiserMode.Distortion)
            {
                for (int i = 1; i <= 3; i++)
                {
                    float glowAlpha = (1f - (i / 3f)) * 0.3f * pulse;
                    float inset = i * 2f;
                    Draw.Rect(renderPos.X + inset, renderPos.Y - currentHeight + inset,
                        width - inset * 2f, currentHeight - inset, borderColor * glowAlpha);
                }
            }
            else if (mode == RiserMode.Black)
            {
                for (int i = 1; i <= 2; i++)
                {
                    float glowAlpha = (1f - (i / 2f)) * 0.15f * pulse;
                    float inset = i * 2f;
                    Draw.Rect(renderPos.X + inset, renderPos.Y - currentHeight + inset,
                        width - inset * 2f, currentHeight - inset, borderColor * glowAlpha);
                }
            }

            // Warning indicator at base when about to rise
            if (!rising && riseTimer < 0.5f)
            {
                float warningAlpha = (0.5f - riseTimer) * 2f;
                Color warningColor = mode == RiserMode.Pure ? Color.DarkGray : borderColor;
                Draw.Rect(renderPos.X, renderPos.Y - 4f, width, 4f, warningColor * warningAlpha);
            }

            // Draw lava bubble sprite on top of the column
            base.Render();
        }
    }
}
