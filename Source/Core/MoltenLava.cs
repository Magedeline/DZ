using Celeste.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    // =============================================
    // MoltenLava - Hazardous lava pool (Water-like)
    // =============================================
    [CustomEntity("MaggyHelper/MoltenLava")]
    [Tracked]
    public class MoltenLava : Entity
    {
        // -- Lava color palette --
        public static readonly Color FillColor = Calc.HexToColor("CC3300") * 0.55f;
        public static readonly Color SurfaceColor = Calc.HexToColor("FF6600") * 0.9f;
        public static readonly Color RayTopColor = Calc.HexToColor("FFAA00") * 0.5f;
        public static readonly Color DeepColor = Calc.HexToColor("880000") * 0.6f;

        public Surface TopSurface;
        public Surface BottomSurface;

        private readonly bool hasTop;
        private readonly bool hasBottom;
        private bool[,] grid;

        private float bubbleTimer;
        private ParticleType bubbleParticle;
        private ParticleType embersParticle;

        private bool killsPlayer;
        private float damageGracePeriod;
        private float graceTimer;

        public MoltenLava(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            float width = data.Width;
            float height = data.Height;
            hasTop = data.Bool("hasTop", true);
            hasBottom = data.Bool("hasBottom", false);
            killsPlayer = data.Bool("killsPlayer", true);
            damageGracePeriod = data.Float("damageGracePeriod", 0f);
            graceTimer = 0f;

            Collider = new Hitbox(width, height);
            Depth = -9999;

            if (hasTop)
                TopSurface = new Surface(Position + new Vector2(8f, 0f), new Vector2(-8f, 0f), (int)width - 16, SurfaceColor);
            if (hasBottom)
                BottomSurface = new Surface(Position + new Vector2(8f, height), new Vector2(-8f, height), (int)width - 16, SurfaceColor);

            Add(new DisplacementRenderHook(RenderDisplacement));

            bubbleParticle = new ParticleType
            {
                Color = Calc.HexToColor("FF9900"),
                Color2 = Calc.HexToColor("FFCC00"),
                Size = 2f,
                SizeRange = 1f,
                SpeedMin = 8f,
                SpeedMax = 20f,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                DirectionRange = (float)Math.PI * 0.25f,
                Direction = -(float)Math.PI / 2f
            };

            embersParticle = new ParticleType
            {
                Color = Calc.HexToColor("FF6600"),
                Color2 = Calc.HexToColor("FFAA00"),
                Size = 1f,
                SizeRange = 1f,
                SpeedMin = 15f,
                SpeedMax = 40f,
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                DirectionRange = (float)Math.PI * 0.15f,
                Direction = -(float)Math.PI / 2f
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            int tilesW = (int)Width / 8;
            int tilesH = (int)Height / 8;
            grid = new bool[tilesW, tilesH];

            for (int tx = 0; tx < tilesW; tx++)
            {
                for (int ty = 0; ty < tilesH; ty++)
                {
                    grid[tx, ty] = !Scene.CollideCheck<Solid>(
                        new Rectangle((int)X + tx * 8, (int)Y + ty * 8, 8, 8));
                }
            }
        }

        public override void Update()
        {
            base.Update();

            TopSurface?.Update();
            BottomSurface?.Update();

            // Kill the player on contact
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                if (killsPlayer)
                {
                    if (damageGracePeriod > 0f)
                    {
                        graceTimer += Engine.DeltaTime;
                        if (graceTimer >= damageGracePeriod)
                        {
                            player.Die((player.Center - Center).SafeNormalize());
                            graceTimer = 0f;
                        }
                    }
                    else
                    {
                        player.Die((player.Center - Center).SafeNormalize());
                    }
                }

                // Surface ripple from player
                if (TopSurface != null)
                {
                    TopSurface.DoRipple(player.Position, 1f);
                }
            }
            else
            {
                graceTimer = 0f;
            }

            // Bubble particles
            bubbleTimer -= Engine.DeltaTime;
            if (bubbleTimer <= 0f)
            {
                bubbleTimer = 0.1f + Calc.Random.NextFloat(0.3f);
                float bx = X + Calc.Random.NextFloat(Width);
                float by = Y + Calc.Random.NextFloat(Height * 0.6f) + Height * 0.3f;
                SceneAs<Level>().Particles.Emit(bubbleParticle, new Vector2(bx, by));
            }

            // Ember particles rising from surface
            if (hasTop && Scene.OnInterval(0.25f))
            {
                float ex = X + 4f + Calc.Random.NextFloat(Width - 8f);
                SceneAs<Level>().Particles.Emit(embersParticle, new Vector2(ex, Y));
            }
        }

        public override void Render()
        {
            // Draw the lava fill
            if (grid != null)
            {
                int tilesW = grid.GetLength(0);
                int tilesH = grid.GetLength(1);
                for (int tx = 0; tx < tilesW; tx++)
                {
                    for (int ty = 0; ty < tilesH; ty++)
                    {
                        if (grid[tx, ty])
                        {
                            // Depth gradient: darker at bottom
                            float depthFactor = (float)ty / tilesH;
                            Color tileColor = Color.Lerp(FillColor, DeepColor, depthFactor * 0.5f);

                            // Animated glow flicker
                            float flicker = (float)Math.Sin(Scene.TimeActive * 2f + tx * 0.7f + ty * 0.5f) * 0.08f;
                            float brightness = Calc.Clamp(1f + flicker, 0.92f, 1.06f);
                            tileColor *= brightness;

                            Draw.Rect(X + tx * 8, Y + ty * 8, 8f, 8f, tileColor);
                        }
                    }
                }
            }
            else
            {
                Draw.Rect(X, Y, Width, Height, FillColor);
            }

            // Draw rendered surfaces (only flush renderer state when needed)
            if (TopSurface != null || BottomSurface != null)
            {
                GameplayRenderer.End();

                TopSurface.Render(Scene);
                BottomSurface.Render(Scene);

                GameplayRenderer.Begin();
            }
        }

        private void RenderDisplacement()
        {
            // Heat shimmer displacement
            Draw.Rect(X, Y, Width, Height, new Color(0.5f, 0.5f, 0.3f, 1f));
        }

        // ======================
        // Surface (ripple sim)
        // ======================
        public class Surface
        {
            private const int RippleResolution = 2;
            private readonly Vector2 worldPos;
            private readonly Vector2 localOffset;
            private readonly int surfaceWidth;
            private readonly Color color;

            private readonly float[] heights;
            private readonly float[] speeds;
            private readonly float[] diffusion;
            private readonly List<Ripple> ripples = new();
            private readonly List<Ray> rays = new();

            private VertexPositionColor[] verts;

            public Surface(Vector2 worldPos, Vector2 localOffset, int width, Color color)
            {
                this.worldPos = worldPos;
                this.localOffset = localOffset;
                this.surfaceWidth = width;
                this.color = color;

                int count = width / RippleResolution;
                heights = new float[count + 1];
                speeds = new float[count + 1];
                diffusion = new float[count + 1];
                verts = new VertexPositionColor[(count + 1) * 2];

                for (int i = 0; i < 3; i++)
                {
                    rays.Add(new Ray
                    {
                        Position = Calc.Random.NextFloat(width),
                        Width = 4f + Calc.Random.NextFloat(8f),
                        Length = 12f + Calc.Random.NextFloat(24f),
                        Duration = 2f + Calc.Random.NextFloat(4f),
                        Timer = Calc.Random.NextFloat(6f)
                    });
                }
            }

            public void Update()
            {
                float dt = Engine.DeltaTime;

                // Ripple physics
                for (int i = 0; i < heights.Length; i++)
                {
                    speeds[i] += (0f - heights[i]) * 16f * dt;
                    speeds[i] *= 0.96f;
                    heights[i] += speeds[i] * dt * 60f;
                }

                // Diffusion pass
                for (int i = 0; i < heights.Length; i++)
                {
                    float sum = heights[i] * 2f;
                    int count = 2;
                    if (i > 0) { sum += heights[i - 1]; count++; }
                    if (i < heights.Length - 1) { sum += heights[i + 1]; count++; }
                    diffusion[i] = sum / count;
                }
                Array.Copy(diffusion, heights, heights.Length);

                // Update ripples
                for (int i = ripples.Count - 1; i >= 0; i--)
                {
                    ripples[i].Percent += dt / ripples[i].Duration;
                    if (ripples[i].Percent >= 1f)
                    {
                        ripples.RemoveAt(i);
                        continue;
                    }

                    int idx = (int)((ripples[i].Position - worldPos.X) / RippleResolution);
                    if (idx >= 0 && idx < heights.Length)
                    {
                        float amplitude = ripples[i].Height * (1f - ripples[i].Percent);
                        heights[idx] = amplitude;
                    }
                }

                // Update rays
                for (int i = 0; i < rays.Count; i++)
                {
                    rays[i].Timer += dt;
                    if (rays[i].Timer >= rays[i].Duration)
                    {
                        rays[i].Timer = 0f;
                        rays[i].Position = Calc.Random.NextFloat(surfaceWidth);
                        rays[i].Width = 4f + Calc.Random.NextFloat(8f);
                        rays[i].Length = 12f + Calc.Random.NextFloat(24f);
                        rays[i].Duration = 2f + Calc.Random.NextFloat(4f);
                    }
                }
            }

            public void DoRipple(Vector2 position, float strength)
            {
                ripples.Add(new Ripple
                {
                    Position = position.X,
                    Speed = 0f,
                    Height = -3f * strength,
                    Percent = 0f,
                    Duration = 0.6f
                });
            }

            public float GetSurfaceHeight(Vector2 position)
            {
                int idx = (int)((position.X - worldPos.X) / RippleResolution);
                idx = Math.Clamp(idx, 0, heights.Length - 1);
                return worldPos.Y + heights[idx];
            }

            public void Render(Scene scene)
            {
                Camera camera = (scene as Level).Camera;
                int count = heights.Length;

                // Render light rays (lava glow rays)
                for (int r = 0; r < rays.Count; r++)
                {
                    float alpha = (float)Math.Sin(rays[r].Timer / rays[r].Duration * Math.PI) * 0.6f;
                    float rx = worldPos.X + rays[r].Position;
                    float rw = rays[r].Width;
                    float rl = rays[r].Length;

                    Color rayColor = RayTopColor * alpha;
                    Draw.SpriteBatch.Draw(
                        Draw.Pixel.Texture.Texture_Safe,
                        new Rectangle(
                            (int)(rx - camera.X),
                            (int)(worldPos.Y - camera.Y),
                            (int)rw,
                            (int)rl),
                        rayColor);
                }

                // Build vertex mesh for surface
                if (verts.Length < (count) * 2)
                    verts = new VertexPositionColor[count * 2];

                for (int i = 0; i < count; i++)
                {
                    float px = worldPos.X + i * RippleResolution - camera.X;
                    float py = worldPos.Y + heights[i] - camera.Y;

                    verts[i * 2] = new VertexPositionColor(
                        new Vector3(px, py - 1f, 0f), color);
                    verts[i * 2 + 1] = new VertexPositionColor(
                        new Vector3(px, py + 1f, 0f), color * 0.3f);
                }

                if (count > 1)
                {
                    GFX.DrawVertices(Matrix.Identity, verts, count * 2, null, null);
                }
            }

            private class Ripple
            {
                public float Position;
                public float Speed;
                public float Height;
                public float Percent;
                public float Duration;
            }

            private class Ray
            {
                public float Position;
                public float Width;
                public float Length;
                public float Duration;
                public float Timer;
            }
        }
    }
}
