using System.Runtime.CompilerServices;
using MaggyHelper.Cutscenes;

namespace MaggyHelper.Entities;

/// <summary>
/// Final ascension manager for Ch20: Kirby, Madeline, and Badeline ascend to heaven
/// through a void filled with gold, pink, and rainbow stars plus Deltarune-style
/// star/diamond symbols. Procedurally renders all effects with vertex geometry.
/// </summary>
[Tracked(true)]
[HotReloadable]
public class KirbyHeavenAscendManager : Entity
{
    private Level level;
    private float fade;
    private float timer;

    // Sub-entities
    private HeavenStreaks streaks;
    private DeltaruneStarField deltaStars;
    private VoidRainbowBackground voidBg;
    private HeavenFader fader;

    public float Fade => fade;

    public KirbyHeavenAscendManager()
    {
        Tag = (int)Tags.TransitionUpdate;
        Depth = 8900;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = Scene as Level;
    }

    /// <summary>
    /// Call to spawn all visual layers and fade in.
    /// </summary>
    public void Activate()
    {
        if (level == null)
            level = Scene as Level;
        
        if (level == null)
            return;

        voidBg = new VoidRainbowBackground(this);
        level.Add(voidBg);

        streaks = new HeavenStreaks(this);
        level.Add(streaks);

        deltaStars = new DeltaruneStarField(this);
        level.Add(deltaStars);
    }

    public IEnumerator FadeIn(float duration = 1.5f)
    {
        float from = fade;
        for (float t = 0f; t < 1f; t += Engine.DeltaTime / duration)
        {
            fade = MathHelper.Lerp(from, 1f, Ease.SineOut(t));
            yield return null;
        }
        fade = 1f;
    }

    public HeavenFader SpawnFader()
    {
        fader = new HeavenFader(this);
        Scene.Add(fader);
        return fader;
    }

    public override void Update()
    {
        base.Update();
        timer += Engine.DeltaTime;
    }

    public override void Render()
    {
        // Dark void base
        Draw.Rect(level.Camera.X - 10f, level.Camera.Y - 10f, 340f, 200f, Color.Black * fade);
    }

    private static float Mod(float x, float m) => (x % m + m) % m;

    // =========================================================================
    //  HEAVEN STREAKS — Gold, Pink, Rainbow colored launch streaks from the void
    // =========================================================================
    public class HeavenStreaks : Entity
    {
        private const float MinSpeed = 500f;
        private const float MaxSpeed = 2200f;
        private const int ParticleCount = 120;
        public float Alpha = 1f;

        private readonly Particle[] particles = new Particle[ParticleCount];
        private readonly List<MTexture> textures;
        private readonly KirbyHeavenAscendManager manager;

        // Gold / Pink / Rainbow palette
        private static readonly Color Gold = Calc.HexToColor("ffd700");
        private static readonly Color GoldLight = Calc.HexToColor("fff4b0");
        private static readonly Color Pink = Calc.HexToColor("ff69b4");
        private static readonly Color PinkLight = Calc.HexToColor("ffb6c1");
        private static readonly Color RoseGold = Calc.HexToColor("b76e79");

        public HeavenStreaks(KirbyHeavenAscendManager manager)
        {
            this.manager = manager;
            Depth = 15;
            textures = GFX.Game.GetAtlasSubtextures("scenery/launch/slice");

            for (int i = 0; i < particles.Length; i++)
            {
                float x = 160f + Calc.Random.Range(24f, 160f) * Calc.Random.Choose(-1, 1);
                float y = Calc.Random.NextFloat(500f);
                float speedFactor = Calc.ClampedMap(Math.Abs(x - 160f), 0f, 160f, 0.3f);
                float speed = speedFactor * Calc.Random.Range(MinSpeed, MaxSpeed);

                particles[i] = new Particle
                {
                    Position = new Vector2(x, y),
                    Speed = speed,
                    Index = Calc.Random.Next(textures.Count),
                    ColorSeed = Calc.Random.NextFloat(1f),
                    WaveOffset = Calc.Random.NextFloat(MathF.PI * 2f),
                    Scale = Calc.Random.Range(0.6f, 1.4f)
                };
            }
        }

        public override void Update()
        {
            base.Update();
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
                particles[i].WaveOffset += Engine.DeltaTime * 1.5f;
                particles[i].ColorSeed += Engine.DeltaTime * 0.15f;
                if (particles[i].ColorSeed > 1f) particles[i].ColorSeed -= 1f;
            }
        }

        public override void Render()
        {
            float alpha = Ease.SineInOut((manager?.Fade ?? 1f) * Alpha);
            if (alpha <= 0f) return;

            Vector2 cam = (Scene as Level).Camera.Position;
            float time = manager?.timer ?? 0f;

            for (int i = 0; i < particles.Length; i++)
            {
                ref Particle p = ref particles[i];
                Vector2 pos = p.Position;
                pos.X = Mod(pos.X + MathF.Sin(p.WaveOffset) * 12f, 320f);
                pos.Y = Mod(pos.Y, 500f) - 150f;
                Vector2 worldPos = pos + cam;

                float scaleX = Calc.ClampedMap(p.Speed, MinSpeed, MaxSpeed, 1.2f, 0.3f) * p.Scale;
                float scaleY = Calc.ClampedMap(p.Speed, MinSpeed, MaxSpeed, 1.2f, 3.5f) * p.Scale;

                Color color = GetHeavenColor(p.ColorSeed, time) * alpha;
                MTexture tex = textures[p.Index % textures.Count];
                tex.DrawCentered(worldPos, color, new Vector2(scaleX, scaleY));
            }

            // Edge fade strips
            Color edgeColor = Color.Lerp(Gold, Pink, MathF.Sin(time * 0.5f) * 0.5f + 0.5f) * alpha * 0.6f;
            Draw.Rect(cam.X - 10f, cam.Y - 10f, 26f, 200f, edgeColor);
            Draw.Rect(cam.X + 320f - 16f, cam.Y - 10f, 26f, 200f, edgeColor);
        }

        /// <summary>
        /// Maps a 0-1 seed + time into the gold/pink/rainbow palette.
        /// </summary>
        private static Color GetHeavenColor(float seed, float time)
        {
            float hue = (seed + time * 0.08f) % 1f;

            // 40% gold, 30% pink, 30% rainbow
            if (hue < 0.4f)
            {
                float t = hue / 0.4f;
                return Color.Lerp(Gold, GoldLight, MathF.Sin(t * MathF.PI));
            }
            else if (hue < 0.7f)
            {
                float t = (hue - 0.4f) / 0.3f;
                return Color.Lerp(Pink, PinkLight, MathF.Sin(t * MathF.PI));
            }
            else
            {
                // Rainbow section
                float t = (hue - 0.7f) / 0.3f;
                return HSVToRGB(t, 0.7f, 1f);
            }
        }

        private struct Particle
        {
            public Vector2 Position;
            public float Speed;
            public int Index;
            public float ColorSeed;
            public float WaveOffset;
            public float Scale;
        }
    }

    // =========================================================================
    //  DELTARUNE STAR FIELD — Procedural four-pointed and six-pointed star symbols
    //  inspired by Deltarune's star/diamond motifs, in gold/pink/rainbow
    // =========================================================================
    public class DeltaruneStarField : Entity
    {
        private const int StarCount = 60;
        private const int DiamondCount = 30;

        public float Alpha = 1f;
        private readonly StarParticle[] stars = new StarParticle[StarCount];
        private readonly DiamondParticle[] diamonds = new DiamondParticle[DiamondCount];
        private readonly KirbyHeavenAscendManager manager;

        public DeltaruneStarField(KirbyHeavenAscendManager manager)
        {
            this.manager = manager;
            Depth = 10;

            for (int i = 0; i < StarCount; i++)
            {
                stars[i] = new StarParticle
                {
                    Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(600f)),
                    Speed = Calc.Random.Range(200f, 900f),
                    Size = Calc.Random.Range(2f, 8f),
                    Points = Calc.Random.Choose(4, 6), // 4-pointed or 6-pointed
                    Rotation = Calc.Random.NextFloat(MathF.PI * 2f),
                    RotSpeed = Calc.Random.Range(-2f, 2f),
                    ColorSeed = Calc.Random.NextFloat(1f),
                    PulseOffset = Calc.Random.NextFloat(MathF.PI * 2f),
                    SparklePhase = Calc.Random.NextFloat(MathF.PI * 2f)
                };
            }

            for (int i = 0; i < DiamondCount; i++)
            {
                diamonds[i] = new DiamondParticle
                {
                    Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(600f)),
                    Speed = Calc.Random.Range(300f, 1100f),
                    Size = Calc.Random.Range(3f, 10f),
                    Rotation = Calc.Random.NextFloat(MathF.PI * 2f),
                    RotSpeed = Calc.Random.Range(-1.5f, 1.5f),
                    ColorSeed = Calc.Random.NextFloat(1f),
                    PulseOffset = Calc.Random.NextFloat(MathF.PI * 2f)
                };
            }
        }

        public override void Update()
        {
            base.Update();
            float dt = Engine.DeltaTime;

            for (int i = 0; i < StarCount; i++)
            {
                stars[i].Position.Y += stars[i].Speed * dt;
                stars[i].Rotation += stars[i].RotSpeed * dt;
                stars[i].PulseOffset += dt * 3f;
                stars[i].SparklePhase += dt * 5f;
                stars[i].ColorSeed += dt * 0.1f;
                if (stars[i].ColorSeed > 1f) stars[i].ColorSeed -= 1f;
            }

            for (int i = 0; i < DiamondCount; i++)
            {
                diamonds[i].Position.Y += diamonds[i].Speed * dt;
                diamonds[i].Rotation += diamonds[i].RotSpeed * dt;
                diamonds[i].PulseOffset += dt * 2.5f;
                diamonds[i].ColorSeed += dt * 0.12f;
                if (diamonds[i].ColorSeed > 1f) diamonds[i].ColorSeed -= 1f;
            }
        }

        public override void Render()
        {
            float alpha = Ease.SineInOut((manager?.Fade ?? 1f) * Alpha);
            if (alpha <= 0f) return;

            Vector2 cam = (Scene as Level).Camera.Position;
            float time = manager?.timer ?? 0f;

            // Draw four/six-pointed stars (Deltarune style)
            for (int i = 0; i < StarCount; i++)
            {
                ref StarParticle s = ref stars[i];
                Vector2 pos;
                pos.X = Mod(s.Position.X, 320f);
                pos.Y = Mod(s.Position.Y, 600f) - 200f;
                Vector2 worldPos = pos + cam;

                float pulse = 1f + MathF.Sin(s.PulseOffset) * 0.3f;
                float sparkle = 0.6f + MathF.Sin(s.SparklePhase) * 0.4f;
                float size = s.Size * pulse;
                Color color = GetStarColor(s.ColorSeed, time) * alpha * sparkle;

                DrawStar(worldPos, size, s.Points, s.Rotation, color);
            }

            // Draw Deltarune-style diamond symbols
            for (int i = 0; i < DiamondCount; i++)
            {
                ref DiamondParticle d = ref diamonds[i];
                Vector2 pos;
                pos.X = Mod(d.Position.X, 320f);
                pos.Y = Mod(d.Position.Y, 600f) - 200f;
                Vector2 worldPos = pos + cam;

                float pulse = 1f + MathF.Sin(d.PulseOffset) * 0.25f;
                float size = d.Size * pulse;
                Color color = GetStarColor(d.ColorSeed, time) * alpha * 0.85f;
                Color innerColor = Color.White * alpha * 0.5f;

                DrawDeltaruneDiamond(worldPos, size, d.Rotation, color, innerColor);
            }
        }

        /// <summary>
        /// Draws a multi-pointed star shape (4 or 6 points) procedurally.
        /// Inner radius is 30% of outer radius for that classic sharp star.
        /// </summary>
        private static void DrawStar(Vector2 center, float outerRadius, int points, float rotation, Color color)
        {
            float innerRadius = outerRadius * 0.3f;
            int totalPoints = points * 2;

            for (int i = 0; i < totalPoints; i++)
            {
                float angle1 = rotation + (i / (float)totalPoints) * MathF.PI * 2f;
                float angle2 = rotation + ((i + 1) / (float)totalPoints) * MathF.PI * 2f;
                float r1 = (i % 2 == 0) ? outerRadius : innerRadius;
                float r2 = ((i + 1) % 2 == 0) ? outerRadius : innerRadius;

                Vector2 p1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * r1;
                Vector2 p2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * r2;

                Draw.Line(center, p1, color);
                Draw.Line(p1, p2, color);
            }

            // Draw a bright center dot
            Draw.Rect(center.X - 0.5f, center.Y - 0.5f, 1f, 1f, color);
        }

        /// <summary>
        /// Draws the Deltarune-style diamond: an elongated diamond with a smaller
        /// diamond inside, plus four small rays extending from the tips.
        /// </summary>
        private static void DrawDeltaruneDiamond(Vector2 center, float size, float rotation, Color color, Color innerColor)
        {
            // Outer diamond (elongated vertically like Deltarune's save star)
            float outerW = size * 0.6f;
            float outerH = size;

            Vector2[] outerPts = new Vector2[4];
            outerPts[0] = RotatePoint(center, new Vector2(center.X, center.Y - outerH), rotation);       // top
            outerPts[1] = RotatePoint(center, new Vector2(center.X + outerW, center.Y), rotation);       // right
            outerPts[2] = RotatePoint(center, new Vector2(center.X, center.Y + outerH), rotation);       // bottom
            outerPts[3] = RotatePoint(center, new Vector2(center.X - outerW, center.Y), rotation);       // left

            // Draw outer diamond edges
            for (int i = 0; i < 4; i++)
                Draw.Line(outerPts[i], outerPts[(i + 1) % 4], color);

            // Inner diamond (smaller)
            float innerW = size * 0.25f;
            float innerH = size * 0.45f;

            Vector2[] innerPts = new Vector2[4];
            innerPts[0] = RotatePoint(center, new Vector2(center.X, center.Y - innerH), rotation);
            innerPts[1] = RotatePoint(center, new Vector2(center.X + innerW, center.Y), rotation);
            innerPts[2] = RotatePoint(center, new Vector2(center.X, center.Y + innerH), rotation);
            innerPts[3] = RotatePoint(center, new Vector2(center.X - innerW, center.Y), rotation);

            for (int i = 0; i < 4; i++)
                Draw.Line(innerPts[i], innerPts[(i + 1) % 4], innerColor);

            // Extended rays from tips (Deltarune sparkle effect)
            float rayLen = size * 0.4f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 dir = outerPts[i] - center;
                if (dir.LengthSquared() > 0.001f)
                {
                    dir.Normalize();
                    Draw.Line(outerPts[i], outerPts[i] + dir * rayLen, color * 0.5f);
                }
            }

            // Bright center
            Draw.Rect(center.X - 1f, center.Y - 1f, 2f, 2f, Color.White * (color.A / 255f));
        }

        private static Vector2 RotatePoint(Vector2 origin, Vector2 point, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            float dx = point.X - origin.X;
            float dy = point.Y - origin.Y;
            return new Vector2(
                origin.X + dx * cos - dy * sin,
                origin.Y + dx * sin + dy * cos
            );
        }

        private static Color GetStarColor(float seed, float time)
        {
            float hue = (seed + time * 0.06f) % 1f;

            if (hue < 0.35f)
            {
                // Gold range
                float t = hue / 0.35f;
                return Color.Lerp(Calc.HexToColor("ffd700"), Calc.HexToColor("fffacd"), MathF.Sin(t * MathF.PI));
            }
            else if (hue < 0.6f)
            {
                // Pink range
                float t = (hue - 0.35f) / 0.25f;
                return Color.Lerp(Calc.HexToColor("ff69b4"), Calc.HexToColor("ffb6c1"), MathF.Sin(t * MathF.PI));
            }
            else
            {
                // Rainbow range
                float t = (hue - 0.6f) / 0.4f;
                return HSVToRGB(t, 0.8f, 1f);
            }
        }

        private struct StarParticle
        {
            public Vector2 Position;
            public float Speed;
            public float Size;
            public int Points;
            public float Rotation;
            public float RotSpeed;
            public float ColorSeed;
            public float PulseOffset;
            public float SparklePhase;
        }

        private struct DiamondParticle
        {
            public Vector2 Position;
            public float Speed;
            public float Size;
            public float Rotation;
            public float RotSpeed;
            public float ColorSeed;
            public float PulseOffset;
        }
    }

    // =========================================================================
    //  VOID RAINBOW BACKGROUND — Dark void with rainbow-colored expanding rings
    //  and twinkling background stars
    // =========================================================================
    public class VoidRainbowBackground : Entity
    {
        private const int BackgroundStarCount = 200;
        public float Alpha = 1f;

        private readonly KirbyHeavenAscendManager manager;
        private readonly BackgroundStar[] bgStars = new BackgroundStar[BackgroundStarCount];
        private readonly VoidRing[] rings = new VoidRing[6];
        private float ringTimer;

        public VoidRainbowBackground(KirbyHeavenAscendManager manager)
        {
            this.manager = manager;
            Depth = -1000020;

            for (int i = 0; i < BackgroundStarCount; i++)
            {
                bgStars[i] = new BackgroundStar
                {
                    Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(600f)),
                    Speed = Calc.Random.Range(60f, 300f),
                    Size = Calc.Random.Range(0.5f, 2.5f),
                    TwinklePhase = Calc.Random.NextFloat(MathF.PI * 2f),
                    TwinkleSpeed = Calc.Random.Range(2f, 6f),
                    ColorSeed = Calc.Random.NextFloat(1f)
                };
            }

            for (int i = 0; i < rings.Length; i++)
            {
                rings[i] = new VoidRing
                {
                    Radius = Calc.Random.Range(20f, 200f),
                    MaxRadius = 250f,
                    Speed = Calc.Random.Range(40f, 80f),
                    Alpha = 0f,
                    ColorOffset = i / (float)rings.Length
                };
            }
        }

        public override void Update()
        {
            base.Update();
            float dt = Engine.DeltaTime;

            for (int i = 0; i < BackgroundStarCount; i++)
            {
                bgStars[i].Position.Y += bgStars[i].Speed * dt;
                bgStars[i].TwinklePhase += bgStars[i].TwinkleSpeed * dt;
                bgStars[i].ColorSeed += dt * 0.05f;
                if (bgStars[i].ColorSeed > 1f) bgStars[i].ColorSeed -= 1f;
            }

            // Periodically spawn expanding rings
            ringTimer += dt;
            if (ringTimer > 1.5f)
            {
                ringTimer = 0f;
                for (int i = 0; i < rings.Length; i++)
                {
                    if (rings[i].Alpha <= 0.01f)
                    {
                        rings[i].Radius = 0f;
                        rings[i].Alpha = 0.6f;
                        rings[i].ColorOffset = Calc.Random.NextFloat(1f);
                        break;
                    }
                }
            }

            for (int i = 0; i < rings.Length; i++)
            {
                rings[i].Radius += rings[i].Speed * dt;
                if (rings[i].Radius > rings[i].MaxRadius)
                    rings[i].Alpha = Calc.Approach(rings[i].Alpha, 0f, dt * 0.8f);
            }
        }

        public override void Render()
        {
            float alpha = (manager?.Fade ?? 1f) * Alpha;
            if (alpha <= 0f) return;

            Vector2 cam = (Scene as Level).Camera.Position;
            Vector2 screenCenter = cam + new Vector2(160f, 90f);
            float time = manager?.timer ?? 0f;

            // Draw twinkling background stars
            for (int i = 0; i < BackgroundStarCount; i++)
            {
                ref BackgroundStar s = ref bgStars[i];
                Vector2 pos;
                pos.X = Mod(s.Position.X, 320f);
                pos.Y = Mod(s.Position.Y, 600f) - 200f;
                Vector2 worldPos = pos + cam;

                float twinkle = 0.3f + 0.7f * (MathF.Sin(s.TwinklePhase) * 0.5f + 0.5f);
                Color color = GetBgStarColor(s.ColorSeed, time) * alpha * twinkle;
                float size = s.Size * (0.8f + 0.2f * MathF.Sin(s.TwinklePhase * 0.7f));

                // Draw as tiny cross (star sparkle)
                Draw.Rect(worldPos.X - size * 0.5f, worldPos.Y - 0.5f, size, 1f, color);
                Draw.Rect(worldPos.X - 0.5f, worldPos.Y - size * 0.5f, 1f, size, color);
            }

            // Draw expanding rainbow rings from center
            for (int i = 0; i < rings.Length; i++)
            {
                ref VoidRing r = ref rings[i];
                if (r.Alpha <= 0.01f) continue;

                Color ringColor = HSVToRGB((r.ColorOffset + time * 0.1f) % 1f, 0.6f, 1f) * alpha * r.Alpha;
                DrawRing(screenCenter, r.Radius, ringColor, 48);
            }
        }

        private static void DrawRing(Vector2 center, float radius, Color color, int segments)
        {
            float step = MathF.PI * 2f / segments;
            Vector2 prev = center + new Vector2(radius, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * step;
                Vector2 next = center + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
                Draw.Line(prev, next, color);
                prev = next;
            }
        }

        private static Color GetBgStarColor(float seed, float time)
        {
            float hue = (seed + time * 0.03f) % 1f;
            if (hue < 0.3f)
                return Color.Lerp(Calc.HexToColor("ffd700"), Color.White, 0.3f);
            else if (hue < 0.55f)
                return Color.Lerp(Calc.HexToColor("ff69b4"), Color.White, 0.3f);
            else
                return HSVToRGB(hue, 0.5f, 1f);
        }

        private struct BackgroundStar
        {
            public Vector2 Position;
            public float Speed;
            public float Size;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public float ColorSeed;
        }

        private struct VoidRing
        {
            public float Radius;
            public float MaxRadius;
            public float Speed;
            public float Alpha;
            public float ColorOffset;
        }
    }

    // =========================================================================
    //  HEAVEN FADER — Warm white/gold/pink fade-out overlay with heavenly glow
    // =========================================================================
    public class HeavenFader : Entity
    {
        public float Fade;
        private readonly KirbyHeavenAscendManager manager;
        private float glowTimer;

        // Soft heavenly colors that pulse during fade
        private static readonly Color WarmWhite = Calc.HexToColor("fffde0");
        private static readonly Color HeavenGold = Calc.HexToColor("fff4b0");
        private static readonly Color HeavenPink = Calc.HexToColor("ffe0ec");

        public HeavenFader(KirbyHeavenAscendManager manager)
        {
            this.manager = manager;
            Depth = -1000010;
        }

        public override void Update()
        {
            base.Update();
            glowTimer += Engine.DeltaTime;
        }

        public override void Render()
        {
            if (Fade <= 0f) return;

            Vector2 cam = (Scene as Level).Camera.Position;
            float time = manager?.timer ?? 0f;

            // Cycle through warm white → gold → pink for a heavenly glow
            float cycle = (MathF.Sin(glowTimer * 1.2f) * 0.5f + 0.5f);
            Color baseColor = Color.Lerp(WarmWhite, HeavenGold, cycle * 0.4f);
            Color finalColor = Color.Lerp(baseColor, HeavenPink, MathF.Sin(glowTimer * 0.7f + 1f) * 0.15f);

            // Main overlay
            Draw.Rect(cam.X - 10f, cam.Y - 10f, 340f, 200f, finalColor * Fade);

            // Soft radial glow from center (brighter in the middle)
            if (Fade > 0.3f)
            {
                float glowAlpha = (Fade - 0.3f) / 0.7f * 0.25f;
                Vector2 center = cam + new Vector2(160f, 90f);
                Color glowColor = Color.White * glowAlpha;

                // Draw a soft cross-shaped glow at center
                float glowSize = 80f + MathF.Sin(glowTimer * 2f) * 20f;
                Draw.Rect(center.X - glowSize, center.Y - 2f, glowSize * 2f, 4f, glowColor);
                Draw.Rect(center.X - 2f, center.Y - glowSize, 4f, glowSize * 2f, glowColor);

                // Wider softer cross
                float wideGlow = glowSize * 0.6f;
                Color softGlow = Color.White * glowAlpha * 0.4f;
                Draw.Rect(center.X - wideGlow, center.Y - 6f, wideGlow * 2f, 12f, softGlow);
                Draw.Rect(center.X - 6f, center.Y - wideGlow, 12f, wideGlow * 2f, softGlow);
            }
        }
    }

    // =========================================================================
    //  SHARED HELPER — HSV to RGB conversion
    // =========================================================================
    private static Color HSVToRGB(float h, float s, float v)
    {
        h = ((h % 1f) + 1f) % 1f;
        float c = v * s;
        float x = c * (1f - MathF.Abs((h * 6f) % 2f - 1f));
        float m = v - c;

        float r, g, b;
        if (h < 1f / 6f)      { r = c; g = x; b = 0; }
        else if (h < 2f / 6f) { r = x; g = c; b = 0; }
        else if (h < 3f / 6f) { r = 0; g = c; b = x; }
        else if (h < 4f / 6f) { r = 0; g = x; b = c; }
        else if (h < 5f / 6f) { r = x; g = 0; b = c; }
        else                   { r = c; g = 0; b = x; }

        return new Color(r + m, g + m, b + m);
    }
}