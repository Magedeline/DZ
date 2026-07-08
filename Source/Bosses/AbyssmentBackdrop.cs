#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Bosses
{
    /// <summary>
    /// "Abyssment" backdrop — a void filled with light-speed streaks representing echoes
    /// of many game universes. Active during Phase 1 of the Kirby Flying Final Battle
    /// (Warp Star ride → void flight).
    ///
    /// This is a Backdrop (styleground), not a CustomEntity.
    /// Add it to the room's Backgrounds in 21_LastLevel.bin using the Loenn styleground
    /// panel. KirbyFinalBattleScene looks it up at runtime via Scene.Tracker.GetEntity.
    ///
    /// Public properties mutated at runtime by KirbyFinalBattleScene:
    ///   Alpha       — overall visibility.
    ///   Cycling     — when true, rotates through series palettes automatically.
    ///   CycleSpeed  — seconds between palette shifts.
    ///   ScrollSpeed — horizontal streak speed (pixels/second).
    /// </summary>
    public class AbyssmentBackdrop : Backdrop
    {
        // ── Public state ──────────────────────────────────────────────────────
        public float Alpha      = 1f;
        public bool  Cycling    = false;
        public float CycleSpeed = 0.35f;
        public float ScrollSpeed = 60f;

        // ── Series palettes ───────────────────────────────────────────────────
        private static readonly Color[][] SeriesPalettes = new Color[][]
        {
            // Kirby
            new[] { Calc.HexToColor("ff4444"), Calc.HexToColor("ff99cc") },
            // Celeste
            new[] { Calc.HexToColor("4488ff"), Calc.HexToColor("aaccff") },
            // Undertale / Deltarune
            new[] { Calc.HexToColor("ffcc00"), Calc.HexToColor("ffe566") },
            // Dream Land
            new[] { Calc.HexToColor("44ff88"), Calc.HexToColor("99ffcc") },
            // Dark Matter void
            new[] { Calc.HexToColor("440088"), Calc.HexToColor("8844cc") },
            // Star Allies
            new[] { Calc.HexToColor("ff8800"), Calc.HexToColor("ffcc44") },
            // Pure light
            new[] { Calc.HexToColor("ffffff"), Calc.HexToColor("ccffff") },
            // Deep void
            new[] { Calc.HexToColor("000088"), Calc.HexToColor("0000cc") },
        };

        // ── Streak pool ───────────────────────────────────────────────────────
        private const int StreakCount = 120;

        private struct Streak
        {
            public Vector2 Position;
            public float   Width;
            public float   Height;
            public float   Speed;
            public Color   Color;
            public float   Alpha;
            public int     Layer; // 0 = slow/dim, 1 = mid, 2 = fast/bright
        }

        private static readonly float[] LayerSpeedMult  = { 0.35f, 0.65f, 1.0f };
        private static readonly float[] LayerAlphaMult  = { 0.35f, 0.60f, 0.85f };

        private readonly Streak[] streaks = new Streak[StreakCount];

        // Vertex budget: BG quad (6) + noise overlay (6) + streaks (120 × 6) = 732
        private const int MaxVerts = 6 + 6 + StreakCount * 6;
        private readonly VertexPositionColor[] verts = new VertexPositionColor[MaxVerts];

        // ── Cycling state ─────────────────────────────────────────────────────
        private int   activePalette;
        private float cycleTimer;

        // ── Constructor ───────────────────────────────────────────────────────
        public AbyssmentBackdrop()
        {
            UseSpritebatch = false;
            activePalette  = 0;
            InitStreaks();
        }

        private void InitStreaks()
        {
            for (int i = 0; i < StreakCount; i++)
                SpawnStreak(ref streaks[i], randomX: true);
        }

        private void SpawnStreak(ref Streak s, bool randomX)
        {
            s.Layer    = Calc.Random.Next(0, 3);
            s.Width    = Calc.Random.Range(12f, 80f) * (s.Layer * 0.4f + 0.6f);
            s.Height   = Calc.Random.Range(1f, 4f);
            s.Speed    = Calc.Random.Range(120f, 360f);
            s.Alpha    = Calc.Random.Range(0.5f, 1.0f);
            s.Position.Y = Calc.Random.Range(0f, 244f);
            s.Position.X = randomX
                ? Calc.Random.Range(-80f, 384f)
                : 384f + s.Width + Calc.Random.Range(0f, 80f);

            Color[] pal = SeriesPalettes[activePalette % SeriesPalettes.Length];
            s.Color = Calc.Random.Choose(pal);
        }

        private void RecolourRandomStreaks(int count)
        {
            Color[] pal = SeriesPalettes[activePalette % SeriesPalettes.Length];
            for (int k = 0; k < count; k++)
            {
                int idx = Calc.Random.Next(0, StreakCount);
                streaks[idx].Color = Calc.Random.Choose(pal);
            }
        }

        // ── Backdrop Update ───────────────────────────────────────────────────
        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (!Visible || Alpha <= 0f) return;

            // Advance streaks
            for (int i = 0; i < StreakCount; i++)
            {
                streaks[i].Position.X -= ScrollSpeed * LayerSpeedMult[streaks[i].Layer] * Engine.DeltaTime;
                if (streaks[i].Position.X + streaks[i].Width < 0f)
                    SpawnStreak(ref streaks[i], randomX: false);
            }

            // Palette cycling
            if (Cycling)
            {
                cycleTimer += Engine.DeltaTime;
                if (cycleTimer >= CycleSpeed)
                {
                    cycleTimer -= CycleSpeed;
                    activePalette = (activePalette + 1) % SeriesPalettes.Length;
                    RecolourRandomStreaks(15);
                }
            }
        }

        // ── Backdrop Render ───────────────────────────────────────────────────
        public override void Render(Scene scene)
        {
            if (!Visible || Alpha <= 0f) return;

            int vi = 0;

            // 1. Black void background
            AddBgQuad(ref vi, Color.Black * Alpha);

            // 2. All streaks
            for (int i = 0; i < StreakCount; i++)
            {
                float a = LayerAlphaMult[streaks[i].Layer] * streaks[i].Alpha * Alpha;
                AddQuad(ref vi, streaks[i].Position, streaks[i].Width, streaks[i].Height,
                    streaks[i].Color * a);
            }

            // 3. Faint sinusoidal noise overlay
            float noiseA = ((float)Math.Sin(scene.TimeActive * 2.3f) * 0.5f + 0.5f) * 0.04f * Alpha;
            if (noiseA > 0.001f)
                AddBgQuad(ref vi, Color.White * noiseA);

            GFX.DrawVertices(Matrix.Identity, verts, vi);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void AddBgQuad(ref int vi, Color color) =>
            AddQuad(ref vi, Vector2.Zero, 384f, 244f, color);

        private void AddQuad(ref int vi, Vector2 pos, float w, float h, Color color)
        {
            if (vi + 6 > MaxVerts) return;
            var tl = new VertexPositionColor(new Vector3(pos.X,     pos.Y,     0f), color);
            var tr = new VertexPositionColor(new Vector3(pos.X + w, pos.Y,     0f), color);
            var bl = new VertexPositionColor(new Vector3(pos.X,     pos.Y + h, 0f), color);
            var br = new VertexPositionColor(new Vector3(pos.X + w, pos.Y + h, 0f), color);
            verts[vi++] = tl; verts[vi++] = tr; verts[vi++] = bl;
            verts[vi++] = tr; verts[vi++] = br; verts[vi++] = bl;
        }
    }
}
