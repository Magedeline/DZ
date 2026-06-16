namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/BlackholeFlareSideway")]
    [Tracked]
    [HotReloadable]
    public class BlackholeFlareSideway : Entity
    {
        public static ParticleType P_Flare;
        public static ParticleType P_Rainbow;

        public enum Directions
        {
            Left,
            Right
        }

        private Directions direction;
        private float speed;
        private float width;
        private float height;
        private float particleTimer;
        private bool glitchy;
        private float glitchTimer;
        private Vector2 glitchOffset;
        private Color[] rainbowColors;
        private int colorIndex;
        private float rainbowTimer;

        public BlackholeFlareSideway(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            string dirStr = data.Attr("direction", "Right");
            direction = (Directions)Enum.Parse(typeof(Directions), dirStr, true);
            
            speed = data.Float("speed", 100f);
            width = data.Width;
            height = data.Height;
            glitchy = data.Bool("glitchy", true);

            Collider = new Hitbox(width, height);
            Add(new PlayerCollider(OnPlayer));
            
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

        public override void Update()
        {
            base.Update();

            // Move sideways
            float moveAmount = speed * Engine.DeltaTime;
            if (direction == Directions.Left)
            {
                X -= moveAmount;
            }
            else
            {
                X += moveAmount;
            }

            // Glitchy offset
            if (glitchy)
            {
                glitchTimer += Engine.DeltaTime;
                if (Scene.OnInterval(0.05f))
                {
                    glitchOffset = new Vector2(
                        Calc.Random.Range(-2f, 2f),
                        Calc.Random.Range(-2f, 2f)
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

            // Remove if off screen
            Level level = Scene as Level;
            if (level != null)
            {
                if (X < level.Bounds.Left - width - 64f || X > level.Bounds.Right + 64f)
                {
                    RemoveSelf();
                }
            }
        }

        private void EmitParticles()
        {
            Level level = Scene as Level;
            if (level == null) return;

            Vector2 particlePos = Position + new Vector2(width / 2f, height / 2f);
            
            // Black hole particles
            Color currentColor = rainbowColors[colorIndex];
            Color nextColor = rainbowColors[(colorIndex + 1) % rainbowColors.Length];
            Color particleColor = Color.Lerp(currentColor, nextColor, rainbowTimer);
            
            ParticleType rainbow = new ParticleType(P_Rainbow)
            {
                Color = particleColor,
                Color2 = Color.Black
            };

            level.ParticlesFG.Emit(rainbow, 1, particlePos, Vector2.One * 8f);
            
            if (glitchy && Calc.Random.Chance(0.3f))
            {
                level.ParticlesFG.Emit(P_Flare, 1, particlePos + glitchOffset, Vector2.One * 4f);
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            // Kill player on contact
            Vector2 direction = (player.Center - Center).SafeNormalize();
            player.Die(direction * 100f);
            
            // Visual effect
            Level level = Scene as Level;
            if (level != null)
            {
                level.Shake(0.3f);
                Audio.Play("event:/game/general/thing_booped", Position);
                
                // Rainbow explosion
                for (int i = 0; i < 15; i++)
                {
                    Color explosionColor = rainbowColors[Calc.Random.Next(rainbowColors.Length)];
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
            Vector2 renderPos = Position + glitchOffset;
            
            // Draw black hole core
            Draw.Rect(renderPos, width, height, Color.Black * 0.8f);
            
            // Draw rainbow border effect
            Color currentColor = rainbowColors[colorIndex];
            Color nextColor = rainbowColors[(colorIndex + 1) % rainbowColors.Length];
            Color borderColor = Color.Lerp(currentColor, nextColor, rainbowTimer);
            
            // Pulsing effect
            float pulse = (float)Math.Sin(glitchTimer * 8f) * 0.3f + 0.7f;
            borderColor *= pulse;
            
            // Draw glitchy borders
            float borderWidth = 2f;
            Draw.Rect(renderPos, width, borderWidth, borderColor); // Top
            Draw.Rect(renderPos + new Vector2(0, height - borderWidth), width, borderWidth, borderColor); // Bottom
            Draw.Rect(renderPos, borderWidth, height, borderColor); // Left
            Draw.Rect(renderPos + new Vector2(width - borderWidth, 0), borderWidth, height, borderColor); // Right
            
            // Inner glow
            for (int i = 1; i <= 3; i++)
            {
                float glowAlpha = (1f - (i / 3f)) * 0.3f * pulse;
                Draw.Rect(renderPos + Vector2.One * i * 2f, width - i * 4f, height - i * 4f, borderColor * glowAlpha);
            }
        }
    }
}





