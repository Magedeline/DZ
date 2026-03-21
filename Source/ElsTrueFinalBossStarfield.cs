using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace MaggyHelper.Entities
{
    public class ElsFinalBossStarfield : Backdrop
    {
        public float Alpha = 1f;
        private const int particleCount = 200;
        private Particle[] particles = new Particle[200];
        private VertexPositionColor[] verts = new VertexPositionColor[1206];
        private static readonly Color[] colors = new Color[4]
        {
            Calc.HexToColor("030c1b"),
            Calc.HexToColor("0b031b"),
            Calc.HexToColor("1b0319"),
            Calc.HexToColor("0f0301")
        };

        // Burst effect state
        private bool isBursting = false;
        private float burstTimer = 0f;
        private const float BURST_DURATION = 0.6f;
        private float burstIntensity = 0f;

        public ElsFinalBossStarfield()
        {
            UseSpritebatch = false;
            for (int index = 0; index < 200; ++index)
            {
                particles[index].Speed = Calc.Random.Range(500f, 1200f);
                particles[index].Direction = new Vector2(-1f, 0.0f);
                particles[index].DirectionApproach = Calc.Random.Range(0.25f, 4f);
                particles[index].Position.X = Calc.Random.Range(0, 384);
                particles[index].Position.Y = Calc.Random.Range(0, 244);
                particles[index].Color = Calc.Random.Choose(ElsFinalBossStarfield.colors);
            }
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
            for (int i = 0; i < particleCount; i++)
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
            
            for (int index = 0; index < 200; ++index)
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
            Color color1 = Color.Black * Alpha;
            verts[0].Color = color1;
            verts[0].Position = new Vector3(-10f, -10f, 0.0f);
            verts[1].Color = color1;
            verts[1].Position = new Vector3(330f, -10f, 0.0f);
            verts[2].Color = color1;
            verts[2].Position = new Vector3(330f, 190f, 0.0f);
            verts[3].Color = color1;
            verts[3].Position = new Vector3(-10f, -10f, 0.0f);
            verts[4].Color = color1;
            verts[4].Position = new Vector3(330f, 190f, 0.0f);
            verts[5].Color = color1;
            verts[5].Position = new Vector3(-10f, 190f, 0.0f);
            for (int index1 = 0; index1 < 200; ++index1)
            {
                int index2 = (index1 + 1) * 6;
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
                Color color2 = particles[index1].Color * Alpha;
                verts[index2].Color = color2;
                verts[index2].Position = new Vector3(vector2_2, 0.0f);
                verts[index2 + 1].Color = color2;
                verts[index2 + 1].Position = new Vector3(vector2_3, 0.0f);
                verts[index2 + 2].Color = color2;
                verts[index2 + 2].Position = new Vector3(vector2_4, 0.0f);
                verts[index2 + 3].Color = color2;
                verts[index2 + 3].Position = new Vector3(vector2_2, 0.0f);
                verts[index2 + 4].Color = color2;
                verts[index2 + 4].Position = new Vector3(vector2_4, 0.0f);
                verts[index2 + 5].Color = color2;
                verts[index2 + 5].Position = new Vector3(vector2_5, 0.0f);
            }
            GFX.DrawVertices(Matrix.Identity, verts, verts.Length);
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