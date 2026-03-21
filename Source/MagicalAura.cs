namespace MaggyHelper.Effects
{
    /// <summary>
    /// Magical Aura effect that creates floating magical particles in the background
    /// </summary>
    [HotReloadable]
    public class MagicalAura : Backdrop
    {
        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Alpha;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public Color Color;
            public float Life;
            public float MaxLife;
        }

        private Particle[] particles;
        private string colorType;
        private float intensity;
        private float speed;
        private int particleCount;
        private Color[] colors;
        private Random random;

        public MagicalAura(string colorType = "purple", float intensity = 1.0f, float speed = 1.0f, int particleCount = 50)
        {
            this.colorType = colorType;
            this.intensity = Math.Max(0.1f, Math.Min(3.0f, intensity));
            this.speed = Math.Max(0.1f, Math.Min(5.0f, speed));
            this.particleCount = Math.Max(10, Math.Min(200, particleCount));

            random = new Random();
            particles = new Particle[this.particleCount];

            setupColors();
            initializeParticles();
        }

        private void setupColors()
        {
            switch (colorType.ToLower())
            {
                case "purple":
                    colors = new Color[] {
                        Color.Purple * 0.8f,
                        Color.MediumPurple * 0.9f,
                        Color.Violet * 0.7f
                    };
                    break;
                case "blue":
                    colors = new Color[] {
                        Color.CornflowerBlue * 0.8f,
                        Color.SkyBlue * 0.9f,
                        Color.LightBlue * 0.7f
                    };
                    break;
                case "green":
                    colors = new Color[] {
                        Color.LimeGreen * 0.8f,
                        Color.ForestGreen * 0.9f,
                        Color.SpringGreen * 0.7f
                    };
                    break;
                case "red":
                    colors = new Color[] {
                        Color.Red * 0.8f,
                        Color.Crimson * 0.9f,
                        Color.Orange * 0.7f
                    };
                    break;
                case "gold":
                    colors = new Color[] {
                        Color.Gold * 0.8f,
                        Color.Yellow * 0.9f,
                        Color.Orange * 0.7f
                    };
                    break;
                case "rainbow":
                default:
                    colors = new Color[] {
                        Color.Purple * 0.8f,
                        Color.Blue * 0.8f,
                        Color.Green * 0.8f,
                        Color.Yellow * 0.8f,
                        Color.Red * 0.8f
                    };
                    break;
            }
        }

        private void initializeParticles()
        {
            for (int i = 0; i < particleCount; i++)
            {
                particles[i] = createParticle();
            }
        }

        private Particle createParticle()
        {
            Particle particle = new Particle
            {
                Position = new Vector2(
                    (float)(random.NextDouble() * 2000 - 1000),
                    (float)(random.NextDouble() * 1200 - 600)
                ),
                Velocity = new Vector2(
                    (float)((random.NextDouble() - 0.5) * 30 * speed),
                    (float)((random.NextDouble() - 0.5) * 20 * speed)
                ),
                Alpha = (float)(random.NextDouble() * 0.8 + 0.2) * intensity,
                Scale = (float)(random.NextDouble() * 0.5 + 0.3) * intensity,
                Rotation = (float)(random.NextDouble() * Math.PI * 2),
                RotationSpeed = (float)((random.NextDouble() - 0.5) * 2 * speed),
                Color = colors[random.Next(colors.Length)],
                MaxLife = (float)(random.NextDouble() * 5 + 3),
            };
            particle.Life = particle.MaxLife;
            return particle;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            for (int i = particles.Length - 1; i >= 0; i--)
            {
                var particle = particles[i];

                // Update position
                particle.Position += particle.Velocity * Engine.DeltaTime;
                particle.Rotation += particle.RotationSpeed * Engine.DeltaTime;

                // Update life
                particle.Life -= Engine.DeltaTime;

                // Fade out as life decreases
                particle.Alpha = (particle.Life / particle.MaxLife) * intensity * 0.8f;

                // Wrap around screen edges
                if (particle.Position.X < -1000) particle.Position.X = 1000;
                if (particle.Position.X > 1000) particle.Position.X = -1000;
                if (particle.Position.Y < -600) particle.Position.Y = 600;
                if (particle.Position.Y > 600) particle.Position.Y = -600;

                // Reset dead particles in-place (no RemoveAt/Add)
                if (particle.Life <= 0)
                {
                    particles[i] = createParticle();
                }
                else
                {
                    particles[i] = particle;
                }
            }
        }

        public override void Render(Scene scene)
        {
            var camera = (scene as Level)?.Camera ?? new Camera();

            for (int i = 0; i < particles.Length; i++)
            {
                var particle = particles[i];
                Vector2 screenPos = particle.Position - camera.Position;

                // Only render particles visible on screen
                if (screenPos.X > -100 && screenPos.X < 420 && screenPos.Y > -100 && screenPos.Y < 340)
                {
                    Draw.Circle(
                        screenPos,
                        3f * particle.Scale,
                        particle.Color * particle.Alpha,
                        8
                    );

                    // Add sparkle effect
                    if (particle.Alpha > 0.5f)
                    {
                        Draw.Line(
                            screenPos + new Vector2(-4 * particle.Scale, 0),
                            screenPos + new Vector2(4 * particle.Scale, 0),
                            particle.Color * (particle.Alpha * 0.5f)
                        );
                        Draw.Line(
                            screenPos + new Vector2(0, -4 * particle.Scale),
                            screenPos + new Vector2(0, 4 * particle.Scale),
                            particle.Color * (particle.Alpha * 0.5f)
                        );
                    }
                }
            }
        }
    }
}




