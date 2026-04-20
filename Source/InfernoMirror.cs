namespace Celeste.Entities
{
    /// <summary>
    /// A mirror entity for Chapter 7 (Infernal Reflections).
    /// Renders a dark reflective surface with a tiled frame border.
    /// Uses MirrorSurface for reflection effects with a hellish red tint.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/InfernoMirror")]
    [Tracked]
    public class InfernoMirror : Entity
    {
        private readonly Color color = Calc.HexToColor("1a0505");
        private readonly Vector2 size;
        private MTexture[,] frame = new MTexture[3, 3];
        private MirrorSurface surface;

        public InfernoMirror(EntityData e, Vector2 offset)
            : base(e.Position + offset)
        {
            size = new Vector2(e.Width, e.Height);
            Depth = 9500;
            Collider = new Hitbox(e.Width, e.Height);

            Add(surface = new MirrorSurface());
            surface.ReflectionOffset = new Vector2(e.Float("reflectX"), e.Float("reflectY"));
            surface.OnRender = () =>
            {
                Draw.Rect(X + 2f, Y + 2f, size.X - 4f, size.Y - 4f, surface.ReflectionColor);
            };

            MTexture mtexture = GFX.Game["scenery/templemirror"];
            for (int i = 0; i < mtexture.Width / 8; i++)
            {
                for (int j = 0; j < mtexture.Height / 8; j++)
                    frame[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(new Frame(this));
        }

        public override void Render()
        {
            Draw.Rect(X + 3f, Y + 3f, size.X - 6f, size.Y - 6f, color);
        }

        private class Frame : Entity
        {
            private readonly InfernoMirror mirror;

            public Frame(InfernoMirror mirror)
            {
                this.mirror = mirror;
                Depth = 8995;
            }

            public override void Render()
            {
                Position = mirror.Position;
                MTexture[,] f = mirror.frame;
                Vector2 s = mirror.size;

                // Corners
                f[0, 0].Draw(Position);
                f[2, 0].Draw(Position + new Vector2(s.X - 8f, 0f));
                f[0, 2].Draw(Position + new Vector2(0f, s.Y - 8f));
                f[2, 2].Draw(Position + new Vector2(s.X - 8f, s.Y - 8f));

                // Horizontal edges
                for (int i = 1; i < s.X / 8f - 1f; i++)
                {
                    f[1, 0].Draw(Position + new Vector2(i * 8, 0f));
                    f[1, 2].Draw(Position + new Vector2(i * 8, s.Y - 8f));
                }

                // Vertical edges
                for (int j = 1; j < s.Y / 8f - 1f; j++)
                {
                    f[0, 1].Draw(Position + new Vector2(0f, j * 8));
                    f[2, 1].Draw(Position + new Vector2(s.X - 8f, j * 8));
                }
            }
        }
    }
}
