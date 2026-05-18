using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Entities
{
    public class ElsFinalBossStarfield : Backdrop
    {
        public float Alpha = 1f;
        private const int ParticleCount = 200;
        private const int BackgroundVertexCount = 12;
        private readonly Particle[] particles = new Particle[ParticleCount];
        private VertexPositionColor[] verts = new VertexPositionColor[BackgroundVertexCount + ParticleCount * 6];
        private static readonly Color[] DefaultColors = new Color[4]
        {
            Calc.HexToColor("030c1b"),
            Calc.HexToColor("0b031b"),
            Calc.HexToColor("1b0319"),
            Calc.HexToColor("0f0301")
        };
        private static readonly Color[] PinkColors = new Color[5]
        {
            Calc.HexToColor("2a0c24"),
            Calc.HexToColor("44102f"),
            Calc.HexToColor("5f143a"),
            Calc.HexToColor("8d2b62"),
            Calc.HexToColor("ffb8e7")
        };
        private static readonly Color[] SoulBlackColors = new Color[5]
        {
            Calc.HexToColor("050507"),
            Calc.HexToColor("11080d"),
            Calc.HexToColor("1b0914"),
            Calc.HexToColor("2c0f1e"),
            Calc.HexToColor("4e1b33")
        };
        private static readonly Color[] StellarrussColors = new Color[7]
        {
            Calc.HexToColor("ffffff"),
            Calc.HexToColor("ff6cce"),
            Calc.HexToColor("ff8c42"),
            Calc.HexToColor("ffe66d"),
            Calc.HexToColor("62ffcf"),
            Calc.HexToColor("6ec7ff"),
            Calc.HexToColor("b68cff")
        };
        private Color[] activePalette = DefaultColors;
        private Color backgroundBaseColor = Color.Black;
        private Color backgroundOverlayColor = Color.Transparent;
        private float backgroundOverlayPulseMin = 0f;
        private float backgroundOverlayPulseRange = 0f;
        private bool rainbowPulseMode = false;

        // Burst effect state
        private bool isBursting = false;
        private float burstTimer = 0f;
        private const float BURST_DURATION = 0.6f;
        private float burstIntensity = 0f;

        public ElsFinalBossStarfield()
        {
            UseSpritebatch = false;
            for (int index = 0; index < ParticleCount; ++index)
            {
                particles[index].Speed = Calc.Random.Range(500f, 1200f);
                particles[index].Direction = new Vector2(-1f, 0.0f);
                particles[index].DirectionApproach = Calc.Random.Range(0.25f, 4f);
                particles[index].Position.X = Calc.Random.Range(0, 384);
                particles[index].Position.Y = Calc.Random.Range(0, 244);
                particles[index].Color = Calc.Random.Choose(activePalette);
            }
        }

        public void SetSiamoTier(SiamoZeroFinalBoss.SiamoZeroTier tier)
        {
            switch (tier)
            {
                case SiamoZeroFinalBoss.SiamoZeroTier.Pink:
                    activePalette = PinkColors;
                    backgroundBaseColor = Calc.HexToColor("09030b");
                    backgroundOverlayColor = Calc.HexToColor("ffe8f7");
                    backgroundOverlayPulseMin = 0.03f;
                    backgroundOverlayPulseRange = 0.05f;
                    rainbowPulseMode = false;
                    break;

                case SiamoZeroFinalBoss.SiamoZeroTier.Stellarruss:
                    activePalette = StellarrussColors;
                    backgroundBaseColor = Color.Black;
                    backgroundOverlayColor = Color.White;
                    backgroundOverlayPulseMin = 0.08f;
                    backgroundOverlayPulseRange = 0.08f;
                    rainbowPulseMode = true;
                    break;

                default:
                    activePalette = SoulBlackColors;
                    backgroundBaseColor = Calc.HexToColor("020203");
                    backgroundOverlayColor = Calc.HexToColor("6b1630");
                    backgroundOverlayPulseMin = 0.01f;
                    backgroundOverlayPulseRange = 0.03f;
                    rainbowPulseMode = false;
                    break;
            }

            RecolorParticles();
        }

        private void RecolorParticles()
        {
            for (int i = 0; i < ParticleCount; i++)
                particles[i].Color = Calc.Random.Choose(activePalette);
        }

        /// <summary>
        /// Triggers a visual-only burst effect (particles explode outward from center).
        /// Does NOT apply any shockwave or pushback to the player.
        /// </summary>
        public void TriggerBurst()
        {
            isBursting = true;
            burstTimer = BURST_DURATION;
            burstIntensity = 1f;
            
            // Boost all particles outward from center for burst
            Vector2 center = new Vector2(192f, 122f);
            for (int i = 0; i < ParticleCount; i++)
            {
                Vector2 awayFromCenter = (particles[i].Position - center);
                if (awayFromCenter.LengthSquared() > 0f)
                    awayFromCenter.Normalize();
                else
                    awayFromCenter = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, 1f);
                
                // Push particles outward rapidly
                particles[i].Direction = awayFromCenter;
                particles[i].Speed = Calc.Random.Range(800f, 1800f);
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (!Visible || Alpha <= 0.0)
                return;
            Level level = scene as Level;
            Vector2 center = new Vector2(192f, 122f); // Center of the starfield area
            
            // Handle burst timer decay
            if (isBursting)
            {
                burstTimer -= Engine.DeltaTime;
                burstIntensity = Math.Max(0f, burstTimer / BURST_DURATION);
                
                if (burstTimer <= 0f)
                {
                    isBursting = false;
                    burstTimer = 0f;
                    burstIntensity = 0f;
                }
            }
            
            for (int index = 0; index < ParticleCount; ++index)
            {
                particles[index].Position += particles[index].Direction * particles[index].Speed * Engine.DeltaTime;
                
                // During burst, particles fly outward; afterward, gradually return to normal swirl
                if (!isBursting)
                {
                    Vector2 toCenter = center - particles[index].Position;
                    if (toCenter.Length() > 0f)
                    {
                        float targetAngle = toCenter.Angle();
                        float angleRadians = Calc.AngleApproach(particles[index].Direction.Angle(), targetAngle, particles[index].DirectionApproach * Engine.DeltaTime);
                        particles[index].Direction = Calc.AngleToVector(angleRadians, 1f);
                    }
                    
                    // Gradually restore normal speed after burst
                    particles[index].Speed = Calc.Approach(particles[index].Speed, Calc.Random.Range(500f, 1200f), 400f * Engine.DeltaTime);
                }
                else
                {
                    // During burst, slow down particles gradually for the explosion-then-fade look
                    particles[index].Speed *= (1f - 1.5f * Engine.DeltaTime);
                }
            }
        }

        public override void Render(Scene scene)
        {
            Vector2 position1 = (scene as Level).Camera.Position;
            int vertexIndex = 0;
            AddBackgroundQuad(ref vertexIndex, backgroundBaseColor * Alpha);

            float overlayAlpha = backgroundOverlayPulseMin;
            if (backgroundOverlayPulseRange > 0f)
                overlayAlpha += ((float)Math.Sin(scene.TimeActive * 1.6f) * 0.5f + 0.5f) * backgroundOverlayPulseRange;

            if (overlayAlpha > 0.001f)
                AddBackgroundQuad(ref vertexIndex, backgroundOverlayColor * overlayAlpha * Alpha);

            for (int index1 = 0; index1 < ParticleCount; ++index1)
            {
                float num1 = Calc.ClampedMap(particles[index1].Speed, 0.0f, 1200f, 1f, 64f);
                float num2 = Calc.ClampedMap(particles[index1].Speed, 0.0f, 1200f, 3f, 0.6f);
                Vector2 direction = particles[index1].Direction;
                Vector2 vector2_1 = direction.Perpendicular();
                Vector2 position2 = particles[index1].Position;
                position2.X = Mod(position2.X - position1.X * 0.9f, 384f) - 32f;
                position2.Y = Mod(position2.Y - position1.Y * 0.9f, 244f) - 32f;
                Vector2 vector2_2 = position2 - direction * num1 * 0.5f - vector2_1 * num2;
                Vector2 vector2_3 = position2 + direction * num1 * 1f - vector2_1 * num2;
                Vector2 vector2_4 = position2 + direction * num1 * 0.5f + vector2_1 * num2;
                Vector2 vector2_5 = position2 - direction * num1 * 1f + vector2_1 * num2;
                Color color2 = GetParticleColor(index1, scene) * Alpha;

                verts[vertexIndex].Color = color2;
                verts[vertexIndex].Position = new Vector3(vector2_2, 0.0f);
                vertexIndex++;
                verts[vertexIndex].Color = color2;
                verts[vertexIndex].Position = new Vector3(vector2_3, 0.0f);
                vertexIndex++;
                verts[vertexIndex].Color = color2;
                verts[vertexIndex].Position = new Vector3(vector2_4, 0.0f);
                vertexIndex++;
                verts[vertexIndex].Color = color2;
                verts[vertexIndex].Position = new Vector3(vector2_2, 0.0f);
                vertexIndex++;
                verts[vertexIndex].Color = color2;
                verts[vertexIndex].Position = new Vector3(vector2_4, 0.0f);
                vertexIndex++;
                verts[vertexIndex].Color = color2;
                verts[vertexIndex].Position = new Vector3(vector2_5, 0.0f);
                vertexIndex++;
            }

            GFX.DrawVertices(Matrix.Identity, verts, vertexIndex);
        }

        private Color GetParticleColor(int index, Scene scene)
        {
            if (!rainbowPulseMode)
                return particles[index].Color;

            float offset = scene.TimeActive * 0.6f + index * 0.11f + burstIntensity * 0.35f;
            return SampleStellarrussColor(offset);
        }

        private static Color SampleStellarrussColor(float offset)
        {
            int count = StellarrussColors.Length;
            float wrapped = offset % count;
            if (wrapped < 0f)
                wrapped += count;

            int left = (int)Math.Floor(wrapped) % count;
            int right = (left + 1) % count;
            float t = wrapped - (float)Math.Floor(wrapped);
            return Color.Lerp(StellarrussColors[left], StellarrussColors[right], t);
        }

        private void AddBackgroundQuad(ref int vertexIndex, Color color)
        {
            verts[vertexIndex].Color = color;
            verts[vertexIndex].Position = new Vector3(-10f, -10f, 0.0f);
            vertexIndex++;
            verts[vertexIndex].Color = color;
            verts[vertexIndex].Position = new Vector3(330f, -10f, 0.0f);
            vertexIndex++;
            verts[vertexIndex].Color = color;
            verts[vertexIndex].Position = new Vector3(330f, 190f, 0.0f);
            vertexIndex++;
            verts[vertexIndex].Color = color;
            verts[vertexIndex].Position = new Vector3(-10f, -10f, 0.0f);
            vertexIndex++;
            verts[vertexIndex].Color = color;
            verts[vertexIndex].Position = new Vector3(330f, 190f, 0.0f);
            vertexIndex++;
            verts[vertexIndex].Color = color;
            verts[vertexIndex].Position = new Vector3(-10f, 190f, 0.0f);
            vertexIndex++;
        }

        private float Mod(float x, float m) => (x % m + m) % m;

        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Direction;
            public float Speed;
            public Color Color;
            public float DirectionApproach;
        }
    }
}
