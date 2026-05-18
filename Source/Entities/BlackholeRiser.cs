namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/BlackholeRiser")]
    [Tracked]
    [HotReloadable]
    public class BlackholeRiser : Entity
    {
        public static ParticleType P_Flare;
        public static ParticleType P_Rainbow;

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

        public BlackholeRiser(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            speed = data.Float("speed", 120f);
            width = data.Width;
            maxHeight = data.Float("maxHeight", 200f);
            riseDelay = data.Float("riseDelay", 1f);
            glitchy = data.Bool("glitchy", true);
            looping = data.Bool("looping", true);

            currentHeight = 0f;
            rising = false;
            riseTimer = riseDelay;

            Depth = -50;
            
            InitializeRainbowColors();
            InitializeParticles();
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
            base.Update();

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

            // Update collider
            Collider = new Hitbox(width, currentHeight, 0f, -currentHeight);
            if (currentHeight > 0f)
            {
                Add(new PlayerCollider(OnPlayer));
            }

            // Glitchy offset
            if (glitchy)
            {
                glitchTimer += Engine.DeltaTime;
                if (Scene.OnInterval(0.05f))
                {
                    glitchOffset = new Vector2(
                        Calc.Random.Range(-3f, 3f),
                        Calc.Random.Range(-3f, 3f)
                    );
                }
            }

            // Rainbow color cycling
            rainbowTimer += Engine.DeltaTime * 5f;
            if (rainbowTimer >= 1f)
            {
                rainbowTimer = 0f;
                colorIndex = (colorIndex + 1) % rainbowColors.Length;
            }

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

        private void EmitParticles()
        {
            Level level = Scene as Level;
            if (level == null) return;

            Vector2 topPos = new Vector2(X + width / 2f, Y - currentHeight);
            
            Color currentColor = rainbowColors[colorIndex];
            Color nextColor = rainbowColors[(colorIndex + 1) % rainbowColors.Length];
            Color particleColor = Color.Lerp(currentColor, nextColor, rainbowTimer);
            
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
                
                // Rainbow explosion
                Color explosionColor = rainbowColors[colorIndex];
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
            
            // Draw black hole core (rising column)
            Draw.Rect(renderPos.X, renderPos.Y - currentHeight, width, currentHeight, Color.Black * 0.8f);
            
            // Draw rainbow border effect
            Color currentColor = rainbowColors[colorIndex];
            Color nextColor = rainbowColors[(colorIndex + 1) % rainbowColors.Length];
            Color borderColor = Color.Lerp(currentColor, nextColor, rainbowTimer);
            
            // Pulsing effect
            float pulse = (float)Math.Sin(glitchTimer * 8f) * 0.3f + 0.7f;
            borderColor *= pulse;
            
            // Draw glitchy borders
            float borderWidth = 2f;
            
            // Top
            Draw.Rect(renderPos.X, renderPos.Y - currentHeight, width, borderWidth, borderColor);
            // Left
            Draw.Rect(renderPos.X, renderPos.Y - currentHeight, borderWidth, currentHeight, borderColor);
            // Right
            Draw.Rect(renderPos.X + width - borderWidth, renderPos.Y - currentHeight, borderWidth, currentHeight, borderColor);
            
            // Inner glow (vertical stripes)
            for (int i = 1; i <= 3; i++)
            {
                float glowAlpha = (1f - (i / 3f)) * 0.3f * pulse;
                float inset = i * 2f;
                Draw.Rect(renderPos.X + inset, renderPos.Y - currentHeight + inset, 
                    width - inset * 2f, currentHeight - inset, borderColor * glowAlpha);
            }

            // Warning indicator at base when about to rise
            if (!rising && riseTimer < 0.5f)
            {
                float warningAlpha = (0.5f - riseTimer) * 2f;
                Draw.Rect(renderPos.X, renderPos.Y - 4f, width, 4f, borderColor * warningAlpha);
            }
        }
    }
}




