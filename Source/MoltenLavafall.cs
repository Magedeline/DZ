using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace MaggyHelper
{
    // =============================================
    // MoltenLavafall - Hazardous lava cascade
    // (Waterfall-like entity for molten magma)
    // =============================================
    [CustomEntity("MaggyHelper/MoltenLavafall")]
    [Tracked]
    public class MoltenLavafall : Entity
    {
        // -- Color palette --
        private static readonly Color FallColor = Calc.HexToColor("CC3300") * 0.6f;
        private static readonly Color FallEdgeColor = Calc.HexToColor("FF6600") * 0.8f;
        private static readonly Color SplashColor = Calc.HexToColor("FF9900");
        private static readonly Color GlowColor = Calc.HexToColor("FFAA00") * 0.3f;

        private float width;
        private float height;
        private bool killsPlayer;
        private float flowSpeed;

        private ParticleType splashParticle;
        private ParticleType embersParticle;

        // Flow animation
        private float flowOffset;
        private readonly List<LavaLine> lavaLines = new();

        // Landing splash
        private MoltenLava landingPool;

        public MoltenLavafall(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            width = data.Width;
            if (width < 8f) width = 8f;
            height = data.Height;
            killsPlayer = data.Bool("killsPlayer", true);
            flowSpeed = data.Float("flowSpeed", 60f);

            Collider = new Hitbox(width, height);
            Depth = -9998;

            Add(new DisplacementRenderHook(RenderDisplacement));

            splashParticle = new ParticleType
            {
                Color = Calc.HexToColor("FF6600"),
                Color2 = Calc.HexToColor("FFCC00"),
                Size = 2f,
                SizeRange = 1f,
                SpeedMin = 10f,
                SpeedMax = 30f,
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                DirectionRange = (float)Math.PI * 0.3f,
                Direction = -(float)Math.PI / 2f
            };

            embersParticle = new ParticleType
            {
                Color = Calc.HexToColor("FF9900"),
                Color2 = Calc.HexToColor("FFAA00"),
                Size = 1f,
                SizeRange = 0.5f,
                SpeedMin = 5f,
                SpeedMax = 15f,
                LifeMin = 0.5f,
                LifeMax = 1.0f,
                DirectionRange = (float)Math.PI * 0.5f,
                Direction = -(float)Math.PI / 2f
            };

            // Create visual flow lines
            int lineCount = Math.Max(2, (int)(width / 4f));
            for (int i = 0; i < lineCount; i++)
            {
                lavaLines.Add(new LavaLine
                {
                    XOffset = Calc.Random.NextFloat(width - 2f) + 1f,
                    Speed = 0.7f + Calc.Random.NextFloat(0.6f),
                    Width = 1f + Calc.Random.NextFloat(2f),
                    Phase = Calc.Random.NextFloat((float)Math.PI * 2f),
                    Brightness = 0.5f + Calc.Random.NextFloat(0.5f)
                });
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Check if there's a MoltenLava pool below us to create splashes
            landingPool = Scene.CollideFirst<MoltenLava>(
                new Rectangle((int)X, (int)(Y + height), (int)width, 4));
        }

        public override void Update()
        {
            base.Update();

            float dt = Engine.DeltaTime;
            flowOffset += flowSpeed * dt;
            if (flowOffset > 16f) flowOffset -= 16f;

            // Kill player on contact
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && killsPlayer && CollideCheck(player))
            {
                player.Die((player.Center - Center).SafeNormalize());
            }

            // Splash particles at the bottom
            if (Scene.OnInterval(0.15f))
            {
                float sx = X + Calc.Random.NextFloat(width);
                SceneAs<Level>().Particles.Emit(splashParticle, new Vector2(sx, Y + height));
            }

            // Embers drifting off the sides
            if (Scene.OnInterval(0.3f))
            {
                float ex = Calc.Random.Choose(X - 2f, X + width + 2f);
                float ey = Y + Calc.Random.NextFloat(height);
                SceneAs<Level>().Particles.Emit(embersParticle, new Vector2(ex, ey));
            }

            // Ripple landing pool surface
            if (landingPool?.TopSurface != null && Scene.OnInterval(0.08f))
            {
                float rx = X + Calc.Random.NextFloat(width);
                landingPool.TopSurface.DoRipple(new Vector2(rx, Y + height), 0.3f);
            }
        }

        public override void Render()
        {
            // Main body fill
            for (float y = 0; y < height; y += 4f)
            {
                float segHeight = Math.Min(4f, height - y);
                float waveX = (float)Math.Sin((Y + y + flowOffset) * 0.1f) * 1.5f;

                // Fill
                Draw.Rect(X + waveX, Y + y, width, segHeight, FallColor);

                // Bright edge lines on left and right
                Draw.Rect(X + waveX, Y + y, 1f, segHeight, FallEdgeColor);
                Draw.Rect(X + waveX + width - 1f, Y + y, 1f, segHeight, FallEdgeColor);
            }

            // Animated lava flow streaks
            for (int i = 0; i < lavaLines.Count; i++)
            {
                var line = lavaLines[i];
                float yOff = (flowOffset * line.Speed + line.Phase * 100f) % height;

                for (float seg = 0; seg < height; seg += 8f)
                {
                    float sy = (seg + yOff) % height;
                    float segLen = Math.Min(6f, height - sy);
                    if (segLen <= 0) continue;

                    float waveX = (float)Math.Sin((Y + sy + flowOffset) * 0.1f) * 1.5f;
                    float flicker = (float)Math.Sin(Scene.TimeActive * 3f + line.Phase) * 0.15f;

                    Color streakColor = Color.Lerp(FallColor, FallEdgeColor, line.Brightness + flicker);
                    Draw.Rect(X + waveX + line.XOffset, Y + sy, line.Width, segLen, streakColor * 0.6f);
                }
            }

            // Glow at the top (source)
            Draw.Rect(X - 1f, Y, width + 2f, 3f, GlowColor);

            // Splash pool indicator at the bottom
            float splashAlpha = 0.3f + (float)Math.Sin(Scene.TimeActive * 4f) * 0.1f;
            Draw.Rect(X - 2f, Y + height - 2f, width + 4f, 4f, SplashColor * splashAlpha);
        }

        private void RenderDisplacement()
        {
            // Heat shimmer for lavafall
            Draw.Rect(X - 2f, Y, width + 4f, height, new Color(0.5f, 0.5f, 0.25f, 1f));
        }

        private class LavaLine
        {
            public float XOffset;
            public float Speed;
            public float Width;
            public float Phase;
            public float Brightness;
        }
    }
}
