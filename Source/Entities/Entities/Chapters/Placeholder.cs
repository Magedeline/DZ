namespace Celeste.Entities.Chapters
{
    /// <summary>
    /// Placeholder drawing helpers shared by the chapter 10-16 entities.
    ///
    /// These entities are gameplay-first: every one of them renders as a flat square
    /// or circle instead of a sprite so they are placeable and testable before any
    /// art exists. Nothing here touches GFX.SpriteBank, so a missing atlas entry can
    /// never crash a room. When real art lands, swap the Render() body for a Sprite
    /// and delete the Placeholder calls - no gameplay code has to change.
    /// </summary>
    internal static class Placeholder
    {
        /// <summary>Filled square/rectangle with a 1px outline.</summary>
        public static void Square(float x, float y, float width, float height, Color fill, Color outline)
        {
            Draw.Rect(x, y, width, height, fill);
            Draw.HollowRect(x, y, width, height, outline);
        }

        public static void Square(Rectangle bounds, Color fill, Color outline)
        {
            Square(bounds.X, bounds.Y, bounds.Width, bounds.Height, fill, outline);
        }

        /// <summary>Filled square centred on a point.</summary>
        public static void SquareCentered(Vector2 center, float size, Color fill, Color outline)
        {
            Square(center.X - size / 2f, center.Y - size / 2f, size, size, fill, outline);
        }

        /// <summary>
        /// Filled disc. Monocle's Draw.Circle only strokes an outline, so this scans
        /// horizontal spans instead. Placeholder radii are small, so the cost is fine.
        /// </summary>
        public static void FillCircle(Vector2 center, float radius, Color color)
        {
            if (radius <= 0f)
            {
                return;
            }

            int r = (int)Math.Ceiling(radius);
            for (int dy = -r; dy <= r; dy++)
            {
                float halfWidth = radius * radius - dy * dy;
                if (halfWidth <= 0f)
                {
                    continue;
                }

                halfWidth = (float)Math.Sqrt(halfWidth);
                Draw.Rect(center.X - halfWidth, center.Y + dy, halfWidth * 2f, 1f, color);
            }
        }

        /// <summary>Filled disc with a stroked edge.</summary>
        public static void Circle(Vector2 center, float radius, Color fill, Color outline)
        {
            FillCircle(center, radius, fill);
            Draw.Circle(center, radius, outline, Math.Max(6, (int)(radius / 2f) + 4));
        }

        /// <summary>
        /// Square drawn with a faux-3D extrusion: a back face offset by
        /// <paramref name="depth"/>, corner struts, then the front face on top.
        /// Used by the Cyber Nexus "2.5D" switch and gate cube.
        /// </summary>
        public static void Cube(float x, float y, float width, float height, float depth, Color front, Color side, Color outline)
        {
            Vector2 back = new Vector2(depth, -depth);

            Draw.Rect(x + back.X, y + back.Y, width, height, side);
            Draw.HollowRect(x + back.X, y + back.Y, width, height, outline);

            Draw.Line(new Vector2(x, y), new Vector2(x, y) + back, outline);
            Draw.Line(new Vector2(x + width, y), new Vector2(x + width, y) + back, outline);
            Draw.Line(new Vector2(x, y + height), new Vector2(x, y + height) + back, outline);
            Draw.Line(new Vector2(x + width, y + height), new Vector2(x + width, y + height) + back, outline);

            Square(x, y, width, height, front, outline);
        }

        /// <summary>Wireframe-only version of <see cref="Cube"/>, for "opened"/inactive states.</summary>
        public static void CubeOutline(float x, float y, float width, float height, float depth, Color outline)
        {
            Vector2 back = new Vector2(depth, -depth);

            Draw.HollowRect(x + back.X, y + back.Y, width, height, outline);
            Draw.HollowRect(x, y, width, height, outline);

            Draw.Line(new Vector2(x, y), new Vector2(x, y) + back, outline);
            Draw.Line(new Vector2(x + width, y), new Vector2(x + width, y) + back, outline);
            Draw.Line(new Vector2(x, y + height), new Vector2(x, y + height) + back, outline);
            Draw.Line(new Vector2(x + width, y + height), new Vector2(x + width, y + height) + back, outline);
        }

        /// <summary>Square outline rotated about its centre, for spinning projectiles.</summary>
        public static void RotatedSquare(Vector2 center, float size, float angle, Color color)
        {
            float half = size / 2f;
            Vector2 a = center + Calc.AngleToVector(angle, half * 1.41421356f);
            Vector2 b = center + Calc.AngleToVector(angle + MathHelper.PiOver2, half * 1.41421356f);
            Vector2 c = center + Calc.AngleToVector(angle + MathHelper.Pi, half * 1.41421356f);
            Vector2 d = center + Calc.AngleToVector(angle - MathHelper.PiOver2, half * 1.41421356f);

            Draw.Line(a, b, color);
            Draw.Line(b, c, color);
            Draw.Line(c, d, color);
            Draw.Line(d, a, color);
        }

        /// <summary>
        /// Shortest distance from <paramref name="point"/> to the segment a-b.
        /// Lets beam/laser hazards use an arbitrary angle without a rotated hitbox.
        /// </summary>
        public static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 axis = b - a;
            float lengthSquared = axis.LengthSquared();
            if (lengthSquared <= 0.0001f)
            {
                return Vector2.Distance(point, a);
            }

            float t = MathHelper.Clamp(Vector2.Dot(point - a, axis) / lengthSquared, 0f, 1f);
            return Vector2.Distance(point, a + axis * t);
        }
    }
}
