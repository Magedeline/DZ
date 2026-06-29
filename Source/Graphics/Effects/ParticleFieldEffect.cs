using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Particle field backdrop effect that renders floating particles.
    /// </summary>
    [CustomBackdrop("DZ/ParticleFieldEffect")]
    [HotReloadable]
    public class ParticleFieldEffect : Backdrop
    {
        private struct FieldParticle
        {
            public Vector2 Position;
            public float Speed;
            public float Size;
            public float Alpha;
            public float Phase;
        }

        private const int MAX_PARTICLES = 100;

        private string particleType;
        private float density;
        private bool active;
        private float time;
        private float visibleFade;
        private readonly FieldParticle[] particles;

        public ParticleFieldEffect()
        {
            particleType = "sparkle";
            density = 1f;
            active = true;
            time = 0f;
            visibleFade = 0f;
            particles = new FieldParticle[MAX_PARTICLES];
            InitializeParticles();
        }

        public ParticleFieldEffect(BinaryPacker.Element data) : this()
        {
            particleType = data.Attr("particleType", "sparkle");
            density = data.AttrFloat("density", 1f);
            active = data.AttrBool("active", true);
        }

        private void InitializeParticles()
        {
            for (int i = 0; i < MAX_PARTICLES; i++)
            {
                particles[i] = new FieldParticle
                {
                    Position = new Vector2(Calc.Random.Range(0f, 320f), Calc.Random.Range(0f, 180f)),
                    Speed = Calc.Random.Range(5f, 20f),
                    Size = Calc.Random.Range(1f, 2.5f),
                    Alpha = Calc.Random.Range(0.3f, 0.8f),
                    Phase = Calc.Random.NextFloat() * MathHelper.TwoPi
                };
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update(Scene scene)
        {
            base.Update(scene);

            Level level = scene as Level;
            visibleFade = Calc.Approach(visibleFade, (IsVisible(level) && active) ? 1f : 0f, Engine.DeltaTime * 2f);
            time += Engine.DeltaTime;

            for (int i = 0; i < MAX_PARTICLES; i++)
            {
                ref FieldParticle p = ref particles[i];
                p.Position.Y -= p.Speed * Engine.DeltaTime;
                if (p.Position.Y < -5f)
                {
                    p.Position.Y = 185f;
                    p.Position.X = Calc.Random.Range(0f, 320f);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render(Scene scene)
        {
            if (visibleFade <= 0f)
                return;

            int count = (int)(MAX_PARTICLES * density);
            count = Math.Min(count, MAX_PARTICLES);

            for (int i = 0; i < count; i++)
            {
                ref FieldParticle p = ref particles[i];
                float twinkle = 0.5f + 0.5f * (float)Math.Sin(time * 3f + p.Phase);
                Color color = Color.White * (p.Alpha * twinkle * visibleFade);
                Draw.Rect(p.Position.X, p.Position.Y, p.Size, p.Size, color);
            }
        }
    }
}
