#nullable enable

using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

/// <summary>
/// High-resolution snow particle effect entity.
/// Displays animated snow particles with parallax scrolling.
/// </summary>
public class MaggyHiresSnow : Entity
{
    private Particle[] particles;
    private readonly int particleCount;
    private readonly float speed;
    private float timer;
    public float Alpha { get; set; } = 1f;

    public MaggyHiresSnow(int particleCount = 100, float speed = 20f, float opacity = 1f)
        : base(Vector2.Zero)
    {
        this.particleCount = particleCount;
        this.speed = speed;
        Alpha = opacity;
        particles = new Particle[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            particles[i] = new Particle
            {
                Position = new Vector2(
                    Calc.Random.Next((int)(-Engine.Width * 0.5f), (int)(Engine.Width * 1.5f)),
                    Calc.Random.Next(-50, Engine.Height)
                ),
                Velocity = new Vector2(
                    Calc.Random.Range(-speed * 0.5f, speed * 0.5f),
                    Calc.Random.Range(speed * 0.5f, speed * 1.5f)
                ),
                Depth = -1000,
            };
        }
    }

    public override void Update()
    {
        base.Update();

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Position += particles[i].Velocity;

            // Wrap around screen edges
            if (particles[i].Position.Y > Engine.Height + 50)
                particles[i].Position.Y = -50;

            if (particles[i].Position.X > Engine.Width + 50)
                particles[i].Position.X = -50;
            else if (particles[i].Position.X < -50)
                particles[i].Position.X = Engine.Width + 50;
        }

        timer += Engine.DeltaTime;
    }

    public override void Render()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            Draw.Pixel.Draw(particles[i].Position, Vector2.Zero,
                Color.White * (Alpha * 0.8f), Vector2.One * 2f);
        }
    }

    private struct Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public int Depth;
    }
}
