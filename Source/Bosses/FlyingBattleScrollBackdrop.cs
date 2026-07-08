#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Bosses
{
    /// <summary>
    /// Fast side-scrolling debris backdrop for Phase 2 of the Kirby Flying Final Battle.
    ///
    /// This is a Backdrop (styleground), not a CustomEntity.
    /// Add it to the room's Backgrounds in 21_LastLevel.bin via the Loenn styleground panel.
    /// KirbyFinalBattleScene looks it up at runtime via Scene.Tracker.GetEntity.
    ///
    /// Public properties mutated at runtime by KirbyFinalBattleScene:
    ///   Active        — whether debris is scrolling.
    ///   ScrollSpeedX  — horizontal pixels/second (increased by PhaseShiftTrigger).
    ///   TintColor     — HP-phase colour overlay pushed each frame by the scene controller.
    ///   TintAlpha     — opacity of the colour tint (0..0.25).
    /// </summary>
    public class FlyingBattleScrollBackdrop : Backdrop
    {
        // ── Public state ──────────────────────────────────────────────────────
        public float ScrollSpeedX = 0f;
        public float ScrollSpeedY = 0f;
        public float Alpha        = 1f;
        public bool  Active       = false;
        public Color TintColor    = Color.Transparent;
        public float TintAlpha    = 0f;

        // ── Debris palette ────────────────────────────────────────────────────
        private static readonly Color[] DebrisPalette = new Color[]
        {
            Calc.HexToColor("4422aa"), // dark-matter purple
            Calc.HexToColor("221133"), // deep void
            Calc.HexToColor("884400"), // scorched rock
            Calc.HexToColor("002244"), // void fragment
            Calc.HexToColor("cc4444"), // burning shard
            Calc.HexToColor("336688"), // ice shard
            Calc.HexToColor("ffdd44"), // star piece
            Calc.HexToColor("ffffff"), // pure light shard
        };

        // ── Debris pool ───────────────────────────────────────────────────────
        private const int DebrisCount = 80;

        private struct Debris
        {
            public Vector2 Position;
            public float   Width;
            public float   Height;
            public float   SpeedX;
            public float   SpeedY;
            public Color   Color;
            public int     Layer; // 0 = far/slow, 1 = mid, 2 = near/fast
        }

        private static readonly float[] LayerSpeedMult = { 0.3f, 0.7f, 1.0f };
        private static readonly float[] LayerAlphaMult = { 0.45f, 0.70f, 1.0f };

        private readonly Debris[] debris = new Debris[DebrisCount];

        // Vertex budget: BG (6) + tint (6) + debris (80 × 6) = 492
        private const int MaxVerts = 6 + 6 + DebrisCount * 6;
        private readonly VertexPositionColor[] verts = new VertexPositionColor[MaxVerts];

        // ── Constructor ───────────────────────────────────────────────────────
        public FlyingBattleScrollBackdrop()
        {
            UseSpritebatch = false;
            InitDebris();
        }

        private void InitDebris()
        {
            for (int i = 0; i < DebrisCount; i++)
                SpawnDebris(ref debris[i], randomX: true);
        }

        private void SpawnDebris(ref Debris d, bool randomX)
        {
            d.Layer    = Calc.Random.Next(0, 3);
            d.Width    = Calc.Random.Range(4f, 18f) * (d.Layer + 1) * 0.5f;
            d.Height   = Calc.Random.Range(2f, 8f);
            d.SpeedX   = Calc.Random.Range(180f, 480f);
            d.SpeedY   = Calc.Random.Range(-8f, 8f);
            d.Color    = Calc.Random.Choose(DebrisPalette);
            d.Position.Y = Calc.Random.Range(0f, 244f);
            d.Position.X = randomX
                ? Calc.Random.Range(0f, 384f)
                : 384f + d.Width + Calc.Random.Range(0f, 60f);
        }

        // ── Called by KirbyFinalBattleScene each frame during Phase 2 ─────────
        public void SetPhaseColor(Color color, float alpha)
        {
            TintColor = color;
            TintAlpha = alpha;
        }

        // ── Backdrop Update ───────────────────────────────────────────────────
        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (!Visible || !Active || Alpha <= 0f) return;

            for (int i = 0; i < DebrisCount; i++)
            {
                float mult = LayerSpeedMult[debris[i].Layer];
                debris[i].Position.X -= ScrollSpeedX * mult * Engine.DeltaTime;
                debris[i].Position.Y += ScrollSpeedY * mult * Engine.DeltaTime;
                if (debris[i].Position.X + debris[i].Width < 0f)
                    SpawnDebris(ref debris[i], randomX: false);
            }
        }

        // ── Backdrop Render ───────────────────────────────────────────────────
        public override void Render(Scene scene)
        {
            if (!Visible || Alpha <= 0f) return;

            int vi = 0;

            // 1. Black background
            AddBgQuad(ref vi, Color.Black * Alpha);

            // 2. Debris
            if (Active)
            {
                for (int i = 0; i < DebrisCount; i++)
                {
                    Color c = debris[i].Color * Alpha * LayerAlphaMult[debris[i].Layer];
                    AddQuad(ref vi, debris[i].Position, debris[i].Width, debris[i].Height, c);
                }
            }

            // 3. Tint overlay
            if (TintAlpha > 0.001f)
                AddBgQuad(ref vi, TintColor * TintAlpha * Alpha);

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
