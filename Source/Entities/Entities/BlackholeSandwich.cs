namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/BlackholeSandwich")]
    [Tracked]
    [HotReloadable]
    public class BlackholeSandwich : Entity
    {
        public static ParticleType P_Flare;
        public static ParticleType P_Rainbow;

        public enum SandwichMode
        {
            Hot,  // Moves up
            Cold  // Moves down
        }

        private SandwichMode mode;
        private float speed;
        private float width;
        private float height;
        private float topY;
        private float bottomY;
        private float particleTimer;
        private bool glitchy;
        private float glitchTimer;
        private Vector2 glitchOffset;
        private Color[] rainbowColors;
        private int colorIndex;
        private float rainbowTimer;
        private bool canSwitch;
        private string switchFlag;

        // Sandwich consists of two hazards
        private Entity topHazard;
        private Entity bottomHazard;

        public BlackholeSandwich(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            string modeStr = data.Attr("mode", "Hot");
            mode = (SandwichMode)Enum.Parse(typeof(SandwichMode), modeStr, true);
            
            speed = data.Float("speed", 80f);
            width = data.Width;
            height = data.Height;
            glitchy = data.Bool("glitchy", true);
            canSwitch = data.Bool("canSwitch", true);
            switchFlag = data.Attr("switchFlag", "");

            topY = Y;
            bottomY = Y + height;

            Depth = -50;
            
            InitializeRainbowColors();
            InitializeParticles();
            InitializeHazards();
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
                    Acceleration = new Vector2(0f, 10f),
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

        private void InitializeHazards()
        {
            // Create top and bottom hazards
            topHazard = new BlackholeSandwichHazard(this, true);
            bottomHazard = new BlackholeSandwichHazard(this, false);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(topHazard);
            scene.Add(bottomHazard);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            topHazard?.RemoveSelf();
            bottomHazard?.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();

            // Check for mode switch via flag
            if (canSwitch && !string.IsNullOrEmpty(switchFlag))
            {
                Level level = Scene as Level;
                if (level != null && level.Session.GetFlag(switchFlag))
                {
                    mode = mode == SandwichMode.Hot ? SandwichMode.Cold : SandwichMode.Hot;
                    level.Session.SetFlag(switchFlag, false);
                    
                    // Visual feedback
                    Audio.Play("event:/game/general/cassette_block_switch_2", Position);
                    level.Shake(0.2f);
                }
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
            particleTimer += Engine.DeltaTime;
            if (particleTimer >= 0.08f)
            {
                particleTimer = 0f;
                EmitParticles();
            }
        }

        private void EmitParticles()
        {
            Level level = Scene as Level;
            if (level == null) return;

            // Emit from top and bottom
            Vector2 topPos = new Vector2(X + width / 2f, topHazard.Y + 4f);
            Vector2 bottomPos = new Vector2(X + width / 2f, bottomHazard.Y + bottomHazard.Height - 4f);
            
            Color currentColor = rainbowColors[colorIndex];
            Color nextColor = rainbowColors[(colorIndex + 1) % rainbowColors.Length];
            Color particleColor = Color.Lerp(currentColor, nextColor, rainbowTimer);
            
            ParticleType rainbow = new ParticleType(P_Rainbow)
            {
                Color = particleColor,
                Color2 = Color.Black
            };

            level.ParticlesFG.Emit(rainbow, 1, topPos, Vector2.One * 8f);
            level.ParticlesFG.Emit(rainbow, 1, bottomPos, Vector2.One * 8f);
            
            if (glitchy && Calc.Random.Chance(0.3f))
            {
                level.ParticlesFG.Emit(P_Flare, 1, topPos + glitchOffset, Vector2.One * 4f);
                level.ParticlesFG.Emit(P_Flare, 1, bottomPos - glitchOffset, Vector2.One * 4f);
            }
        }

        public SandwichMode GetMode()
        {
            return mode;
        }

        public float GetSpeed()
        {
            return speed;
        }

        public Vector2 GetGlitchOffset()
        {
            return glitchOffset;
        }

        public Color GetCurrentRainbowColor()
        {
            Color currentColor = rainbowColors[colorIndex];
            Color nextColor = rainbowColors[(colorIndex + 1) % rainbowColors.Length];
            return Color.Lerp(currentColor, nextColor, rainbowTimer);
        }

        public float GetGlitchTimer()
        {
            return glitchTimer;
        }

        public float GetWidth()
        {
            return width;
        }

        public float GetTopY()
        {
            return topY;
        }

        public float GetBottomY()
        {
            return bottomY;
        }

        // Inner class for sandwich hazards
        private class BlackholeSandwichHazard : Entity
        {
            private BlackholeSandwich parent;
            private bool isTop;
            private float hazardHeight = 16f;

            public BlackholeSandwichHazard(BlackholeSandwich parent, bool isTop)
            {
                this.parent = parent;
                this.isTop = isTop;
                
                X = parent.X;
                Y = isTop ? parent.GetTopY() : parent.GetBottomY() - hazardHeight;
                
                Collider = new Hitbox(parent.GetWidth(), hazardHeight);
                Add(new PlayerCollider(OnPlayer));
                
                Depth = -50;
            }

            public override void Update()
            {
                base.Update();

                // Move based on mode
                float moveAmount = parent.GetSpeed() * Engine.DeltaTime;
                BlackholeSandwich.SandwichMode mode = parent.GetMode();

                if (isTop)
                {
                    // Top hazard
                    if (mode == SandwichMode.Hot)
                    {
                        Y -= moveAmount; // Move up when hot
                        if (Y < parent.GetTopY() - 200f)
                        {
                            Y = parent.GetTopY();
                        }
                    }
                    else
                    {
                        Y += moveAmount; // Move down when cold
                        if (Y > parent.GetBottomY() - hazardHeight - 32f)
                        {
                            Y = parent.GetBottomY() - hazardHeight - 32f;
                        }
                    }
                }
                else
                {
                    // Bottom hazard
                    if (mode == SandwichMode.Hot)
                    {
                        Y += moveAmount; // Move down when hot
                        if (Y > parent.GetBottomY() + 200f)
                        {
                            Y = parent.GetBottomY() - hazardHeight;
                        }
                    }
                    else
                    {
                        Y -= moveAmount; // Move up when cold
                        if (Y < parent.GetTopY() + 32f)
                        {
                            Y = parent.GetTopY() + 32f;
                        }
                    }
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
                    Color rainbowColor = parent.GetCurrentRainbowColor();
                    for (int i = 0; i < 15; i++)
                    {
                        ParticleType explosion = new ParticleType(P_Rainbow)
                        {
                            Color = rainbowColor,
                            Color2 = Color.Black
                        };
                        level.ParticlesFG.Emit(explosion, Center, (float)Math.PI * 2f);
                    }
                }
            }

            public override void Render()
            {
                Vector2 glitchOffset = parent.GetGlitchOffset();
                Vector2 renderPos = Position + glitchOffset;
                
                // Draw black hole core
                Draw.Rect(renderPos, Width, Height, Color.Black * 0.8f);
                
                // Draw rainbow border effect
                Color borderColor = parent.GetCurrentRainbowColor();
                
                // Pulsing effect
                float pulse = (float)Math.Sin(parent.GetGlitchTimer() * 8f) * 0.3f + 0.7f;
                borderColor *= pulse;
                
                // Draw glitchy borders
                float borderWidth = 2f;
                Draw.Rect(renderPos, Width, borderWidth, borderColor); // Top
                Draw.Rect(renderPos + new Vector2(0, Height - borderWidth), Width, borderWidth, borderColor); // Bottom
                Draw.Rect(renderPos, borderWidth, Height, borderColor); // Left
                Draw.Rect(renderPos + new Vector2(Width - borderWidth, 0), borderWidth, Height, borderColor); // Right
                
                // Inner glow
                for (int i = 1; i <= 2; i++)
                {
                    float glowAlpha = (1f - (i / 2f)) * 0.3f * pulse;
                    float inset = i * 2f;
                    Draw.Rect(renderPos + new Vector2(inset, inset), 
                        Width - inset * 2f, Height - inset * 2f, borderColor * glowAlpha);
                }
            }
        }
    }
}





