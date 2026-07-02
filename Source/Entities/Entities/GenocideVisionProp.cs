using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Entities
{
    public class GenocideVisionProp : Entity
    {
        public enum PropKind
        {
            RealKnife,
            HeartLocket
        }

        private readonly PropKind kind;
        private readonly Color fallbackColor;
        private readonly MTexture texture;
        private float bobTimer;

        public GenocideVisionProp(Vector2 position, PropKind kind)
            : base(position)
        {
            this.kind = kind;
            Depth = 12;
            Collider = new Hitbox(10f, 10f, -5f, -10f);
            fallbackColor = kind == PropKind.RealKnife ? Color.Silver : Calc.HexToColor("B71C3C");
            texture = TryLoadTexture(kind == PropKind.RealKnife ? "objects/DZ/DZ/ch7genocide/realknife" : "objects/DZ/DZ/ch7genocide/heartlocket");
        }

        public void Collect()
        {
            Visible = false;
            Collidable = false;
        }

        public override void Update()
        {
            base.Update();
            bobTimer += Engine.DeltaTime * 2f;
        }

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }

            Vector2 drawPosition = Position + new Vector2(0f, (float)Math.Sin(bobTimer) * 2f);

            if (texture != null)
            {
                texture.DrawCentered(drawPosition, Color.White);
                return;
            }

            if (kind == PropKind.RealKnife)
            {
                Draw.Line(drawPosition + new Vector2(-3f, -8f), drawPosition + new Vector2(2f, 0f), fallbackColor, 2f);
                Draw.Rect(drawPosition.X - 1f, drawPosition.Y - 1f, 5f, 2f, Color.Brown);
            }
            else
            {
                Draw.Circle(drawPosition, 4f, fallbackColor, 16);
                Draw.Line(drawPosition + new Vector2(0f, -6f), drawPosition + new Vector2(0f, -10f), Color.Gold, 1f);
            }
        }

        private static MTexture TryLoadTexture(string path)
        {
            try
            {
                return GFX.Game[path];
            }
            catch
            {
                return null;
            }
        }
    }
}
