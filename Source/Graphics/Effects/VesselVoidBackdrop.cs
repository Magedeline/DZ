using Monocle;

namespace Celeste.Effects
{
    /// <summary>
    /// Procedural "frozen lightning" void backdrop for the vessel creation ritual —
    /// a four-way-mirrored web of glowing cracks radiating from screen center, with
    /// a soft pulsing core glow. Reference: a fan animation of Deltarune's vessel/
    /// GONER MAKER creation scene, which frames the ritual in a symmetric kaleidoscope
    /// of veined light rather than a flat color.
    ///
    /// Shared by two callers:
    ///  - VesselCreationVignette calls Update/Render directly each frame in place of
    ///    its old flat black rect (see renderVesselGraphics-adjacent code in Render()).
    ///  - Everest/Loenn can place "DZ/VesselVoidBackdrop" as a normal in-game
    ///    styleground via the [CustomBackdrop] registration below.
    ///
    /// No art assets required — the vein pattern is generated once from a seed and
    /// only its brightness/color animate, so it works before final DLC art exists.
    /// Geometry is stored in a normalized [-1,1]-ish space around the origin and
    /// scaled to whatever canvas the caller is rendering into (320x180 for a Level
    /// styleground, or the vignette's full view size), so the same instance works
    /// in both contexts.
    /// </summary>
    [CustomBackdrop("DZ/VesselVoidBackdrop")]
    [HotReloadable]
    public class VesselVoidBackdrop : Backdrop
    {
        private struct Vein
        {
            public Vector2 Start;
            public Vector2 End;
            public float Thickness;
            public float Distance;
        }

        private const int TRUNK_COUNT = 4;
        private const int MAX_DEPTH = 5;
        private const float REACH = 1.15f;

        public float Alpha = 1f;
        public float PulseSpeed = 1.4f;
        public Color InnerColor = Calc.HexToColor("FF6FCF");
        public Color OuterColor = Calc.HexToColor("3AC8FF");

        private readonly Vein[] wedge;
        private float maxDistance;
        private float time;

        public VesselVoidBackdrop() : this(seed: 42)
        {
        }

        public VesselVoidBackdrop(int seed)
        {
            UseSpritebatch = true;
            wedge = GenerateWedge(seed, out maxDistance);
        }

        public VesselVoidBackdrop(BinaryPacker.Element data) : this(data.AttrInt("seed", 42))
        {
            Alpha = data.AttrFloat("alpha", 1f);
            PulseSpeed = data.AttrFloat("pulseSpeed", 1.4f);
            InnerColor = Calc.HexToColor(data.Attr("innerColor", "FF6FCF"));
            OuterColor = Calc.HexToColor(data.Attr("outerColor", "3AC8FF"));
        }

        private static Vein[] GenerateWedge(int seed, out float maxDistance)
        {
            var rng = new System.Random(seed);
            var segments = new List<Vein>();
            maxDistance = 0f;

            for (int i = 0; i < TRUNK_COUNT; i++)
            {
                float baseAngle = MathHelper.Lerp(20f, 75f, i / (float)(TRUNK_COUNT - 1));
                baseAngle += (float)(rng.NextDouble() * 8f - 4f);
                Branch(Vector2.Zero, baseAngle, 0.5f, MAX_DEPTH, rng, segments, ref maxDistance);
            }

            return segments.ToArray();
        }

        private static void Branch(Vector2 origin, float angleDeg, float length, int depth, System.Random rng, List<Vein> segments, ref float maxDistance)
        {
            if (depth <= 0 || length < 0.02f)
                return;

            float angleRad = MathHelper.ToRadians(angleDeg);
            Vector2 dir = new Vector2((float)System.Math.Cos(angleRad), -(float)System.Math.Sin(angleRad));
            Vector2 end = origin + dir * length;

            float thickness = MathHelper.Lerp(0.6f, 2.2f, depth / (float)MAX_DEPTH);
            float distance = ((origin + end) * 0.5f).Length();
            maxDistance = System.Math.Max(maxDistance, end.Length());

            segments.Add(new Vein { Start = origin, End = end, Thickness = thickness, Distance = distance });

            int childCount = depth > 2 ? rng.Next(1, 3) : rng.Next(0, 2);
            for (int c = 0; c < childCount; c++)
            {
                float childAngle = angleDeg + (float)(rng.NextDouble() * 70f - 35f);
                float childLength = length * (0.55f + (float)rng.NextDouble() * 0.2f);
                Branch(end, childAngle, childLength, depth - 1, rng, segments, ref maxDistance);
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (!Visible)
                return;

            time += Engine.DeltaTime;
        }

        public override void Render(Scene scene)
        {
            if (!Visible || Alpha <= 0f)
                return;

            Vector2 canvas = scene is Level ? new Vector2(320f, 180f) : new Vector2(Engine.ViewWidth, Engine.ViewHeight);
            Vector2 center = canvas * 0.5f;
            float scale = System.Math.Min(canvas.X, canvas.Y) * 0.5f * REACH;

            // Soft core glow at the origin, where the vessel sits.
            float corePulse = 0.75f + 0.25f * (float)System.Math.Sin(time * PulseSpeed * 1.5f);
            Draw.Circle(center, scale * 0.05f + 4f, InnerColor * (0.20f * Alpha * corePulse), 10);
            Draw.Circle(center, scale * 0.03f + 2f, InnerColor * (0.35f * Alpha * corePulse), 10);

            for (int mirrorX = -1; mirrorX <= 1; mirrorX += 2)
            {
                for (int mirrorY = -1; mirrorY <= 1; mirrorY += 2)
                {
                    RenderWedge(mirrorX, mirrorY, center, scale);
                }
            }
        }

        private void RenderWedge(int mirrorX, int mirrorY, Vector2 center, float scale)
        {
            for (int i = 0; i < wedge.Length; i++)
            {
                Vein v = wedge[i];
                Vector2 start = center + new Vector2(v.Start.X * mirrorX, v.Start.Y * mirrorY) * scale;
                Vector2 end = center + new Vector2(v.End.X * mirrorX, v.End.Y * mirrorY) * scale;

                float normDist = maxDistance > 0f ? MathHelper.Clamp(v.Distance / maxDistance, 0f, 1f) : 0f;
                Color baseColor = Color.Lerp(InnerColor, OuterColor, normDist);

                float pulse = 0.5f + 0.5f * (float)System.Math.Sin(v.Distance * 9f - time * PulseSpeed);
                float glowAlpha = MathHelper.Lerp(0.18f, 0.5f, pulse) * Alpha;
                float coreAlpha = MathHelper.Lerp(0.45f, 1f, pulse) * Alpha;

                Draw.Line(start, end, baseColor * glowAlpha, v.Thickness * 3f);
                Draw.Line(start, end, Color.Lerp(baseColor, Color.White, 0.3f) * coreAlpha, v.Thickness);
            }
        }
    }
}
