using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Rainbow space dust effect that can be used for both foreground and background stylegrounds.
    /// Features colorful particles that cycle through the rainbow spectrum with wind-based movement.
    /// </summary>
    [CustomBackdrop("DZ/RainbowSpaceDust")]
    [HotReloadable]
    public class RainbowSpaceDust : Backdrop
    {
        private struct DustParticle
        {
            public Vector2 Position;
            public float Size;
            public float Speed;
            public float RainbowOffset;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public float Alpha;
        }

        private const int PARTICLE_COUNT = 200;
        private const int RAINBOW_COLOR_COUNT = 16;
        private const float LOOP_WIDTH = 640f;
        private const float LOOP_HEIGHT = 360f;

        private readonly DustParticle[] particles;
        private readonly Color[] rainbowColors;
        private float rainbowTime;
        private float rainbowSpeed = 2f;

        public Vector2 CameraOffset = Vector2.Zero;
        public float Alpha = 1f;
        public float Scale = 1f;
        public float SpeedMultiplier = 1f;
        public bool UseWind = true;
        public bool IsForeground = false;

        private float visibleFade = 1f;

        public RainbowSpaceDust()
        {
            Color = Color.White;
            particles = new DustParticle[PARTICLE_COUNT];
            rainbowColors = new Color[RAINBOW_COLOR_COUNT];
            
            InitializeRainbowColors();
            InitializeParticles();
        }

        public RainbowSpaceDust(BinaryPacker.Element data) : this()
        {
            if (data.HasAttr("alpha"))
                Alpha = MathHelper.Clamp(data.AttrFloat("alpha", 1f), 0f, 1f);
            
            if (data.HasAttr("scale"))
                Scale = data.AttrFloat("scale", 1f);
            
            if (data.HasAttr("speed"))
                SpeedMultiplier = data.AttrFloat("speed", 1f);
            
            if (data.HasAttr("rainbowSpeed"))
                rainbowSpeed = data.AttrFloat("rainbowSpeed", 2f);
            
            if (data.HasAttr("useWind"))
                UseWind = data.AttrBool("useWind", true);
            
            if (data.HasAttr("isForeground"))
                IsForeground = data.AttrBool("isForeground", false);
        }

        private void InitializeRainbowColors()
        {
            for (int i = 0; i < RAINBOW_COLOR_COUNT; i++)
            {
                float hue = (float)i / RAINBOW_COLOR_COUNT;
                rainbowColors[i] = Calc.HsvToColor(hue, 0.8f, 1f);
            }
        }

        private void InitializeParticles()
        {
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                particles[i] = new DustParticle
                {
                    Position = new Vector2(
                        Calc.Random.Range(0f, LOOP_WIDTH),
                        Calc.Random.Range(0f, LOOP_HEIGHT)
                    ),
                    Size = Calc.Random.Range(1f, 3f),
                    Speed = Calc.Random.Range(10f, 30f),
                    RainbowOffset = Calc.Random.NextFloat() * MathHelper.TwoPi,
                    TwinklePhase = Calc.Random.NextFloat() * MathHelper.TwoPi,
                    TwinkleSpeed = Calc.Random.Range(2f, 5f),
                    Alpha = Calc.Random.Range(0.3f, 0.8f)
                };
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update(Scene scene)
        {
            base.Update(scene);
            
            Level level = scene as Level;
            visibleFade = Calc.Approach(visibleFade, IsVisible(level) ? 1 : 0, Engine.DeltaTime * 2f);
            
            rainbowTime += Engine.DeltaTime * rainbowSpeed;

            Vector2 movement = Vector2.Zero;
            
            if (UseWind && level != null)
            {
                movement = level.Wind * 2f;
            }
            else
            {
                // Default gentle drift if no wind
                movement = new Vector2(10f, 5f);
            }

            movement *= SpeedMultiplier;

            for (int i = 0; i < particles.Length; i++)
            {
                ref DustParticle particle = ref particles[i];
                
                // Update twinkle
                particle.TwinklePhase += Engine.DeltaTime * particle.TwinkleSpeed;
                
                // Move particle
                particle.Position += movement * Engine.DeltaTime * (particle.Speed / 20f);
                
                // Wrap around
                particle.Position.X %= LOOP_WIDTH;
                if (particle.Position.X < 0f)
                    particle.Position.X += LOOP_WIDTH;
                
                particle.Position.Y %= LOOP_HEIGHT;
                if (particle.Position.Y < 0f)
                    particle.Position.Y += LOOP_HEIGHT;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render(Scene scene)
        {
            if (Alpha <= 0f)
                return;

            Level level = scene as Level;
            float combinedAlpha = Alpha * visibleFade;

            for (int i = 0; i < particles.Length; i++)
            {
                ref DustParticle particle = ref particles[i];
                
                // Calculate screen position
                Vector2 screenPos = particle.Position;
                screenPos.X -= level.Camera.X + CameraOffset.X;
                screenPos.X %= LOOP_WIDTH;
                if (screenPos.X < 0f)
                    screenPos.X += LOOP_WIDTH;
                
                screenPos.Y -= level.Camera.Y + CameraOffset.Y;
                screenPos.Y %= LOOP_HEIGHT;
                if (screenPos.Y < 0f)
                    screenPos.Y += LOOP_HEIGHT;

                // Calculate rainbow color
                float rainbowIndex = (rainbowTime + particle.RainbowOffset) % MathHelper.TwoPi;
                int colorIndex = (int)(rainbowIndex / (MathHelper.TwoPi / RAINBOW_COLOR_COUNT));
                colorIndex = colorIndex % RAINBOW_COLOR_COUNT;
                
                Color particleColor = rainbowColors[colorIndex];
                
                // Apply twinkle effect
                float twinkle = 0.5f + 0.5f * (float)Math.Sin(particle.TwinklePhase);
                particleColor *= particle.Alpha * twinkle * combinedAlpha;

                // Draw particle
                float size = particle.Size * Scale;
                Draw.Circle(screenPos, size, particleColor, 8);
            }
        }
    }
}