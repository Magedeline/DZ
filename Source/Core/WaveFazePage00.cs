using System.Globalization;

namespace Celeste
{
    [HotReloadable]
    public class WaveFazePage00 : WaveFazePage
    {
        private Color taskbarColor = Calc.HexToColor("d9d3b1");
        private string time;
        private Vector2 pptIcon;
        private Vector2 cursor;
        private bool selected;

        public WaveFazePage00()
        {
            AutoProgress = true;
            ClearColor = Calc.HexToColor("118475");
            time = DateTime.Now.ToString("h:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
            pptIcon = new Vector2(600f, 500f);
            cursor = new Vector2(1000f, 700f);
        }

        public override IEnumerator Routine()
        {
            WaveFazePage00 waveFazePage00 = this;
            yield return 1f;
            yield return waveFazePage00.MoveCursor(waveFazePage00.cursor + new Vector2(0.0f, -80f), 0.3f);
            yield return 0.2f;
            yield return waveFazePage00.MoveCursor(waveFazePage00.pptIcon, 0.8f);
            yield return 0.7f;
            waveFazePage00.selected = true;
            Audio.Play("event:/new_content/game/10_farewell/ppt_doubleclick");
            yield return 0.1f;
            waveFazePage00.selected = false;
            yield return 0.1f;
            waveFazePage00.selected = true;
            yield return 0.08f;
            waveFazePage00.selected = false;
            yield return 0.5f;
            waveFazePage00.Presentation.ScaleInPoint = waveFazePage00.pptIcon;
        }

        private IEnumerator MoveCursor(Vector2 to, float time)
        {
            Vector2 from = cursor;
            for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime / time)
            {
                cursor = from + (to - from) * Ease.SineOut(t);
                yield return null;
            }
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            DrawIcon(new Vector2(320f, 120f), "desktop/kirbyemulator_icon", Dialog.Clean("WAVEFAZE_DESKTOP_KIRBY"));
            DrawIcon(new Vector2(160f, 720f), "desktop/gamemaker_icon", Dialog.Clean("WAVEFAZE_DESKTOP_GAMEMAKER"));
            DrawIcon(new Vector2(160f, 520f), "desktop/undernet_icon", Dialog.Clean("WAVEFAZE_DESKTOP_UNDERNET"));
            DrawIcon(new Vector2(160f, 120f), "desktop/mymountain_icon", Dialog.Clean("WAVEFAZE_DESKTOP_MYPC"));
            DrawIcon(new Vector2(160f, 320f), "desktop/recyclebin_icon", Dialog.Clean("WAVEFAZE_DESKTOP_RECYCLEBIN"));
            DrawIcon(pptIcon, "desktop/wavedashing_icon", Dialog.Clean("WAVEFAZE_DESKTOP_POWERPOINT"));
            DrawTaskbar();
            Presentation.Gfx["desktop/cursor"].DrawCentered(cursor);
        }

        public void DrawTaskbar()
        {
            Draw.Rect(0.0f, Height - 80f, Width, 80f, taskbarColor);
            Draw.Rect(0.0f, Height - 80f, Width, 4f, Color.White * 0.5f);
            MTexture mtexture = Presentation.Gfx["desktop/startberry"];
            float height = 64f;
            float iconScale = (float) (height / (double) mtexture.Height * 0.699999988079071);
            string text = Dialog.Clean("WAVEFAZE_DESKTOP_STARTBUTTON");
            float textScale = 0.6f;
            float width = (float) (mtexture.Width * (double) iconScale + ActiveFont.Measure(text).X * (double) textScale + 32.0);
            Vector2 vector2 = new Vector2(8f, (float) (Height - 80.0 + 8.0));
            Draw.Rect(vector2.X, vector2.Y, width, height, Color.White * 0.5f);
            mtexture.DrawJustified(vector2 + new Vector2(8f, height / 2f), new Vector2(0.0f, 0.5f), Color.White, Vector2.One * iconScale);
            ActiveFont.Draw(text, vector2 + new Vector2((float) (mtexture.Width * (double) iconScale + 16.0), height / 2f), new Vector2(0.0f, 0.5f), Vector2.One * textScale, Color.Black * 0.8f);
            ActiveFont.Draw(time, new Vector2(Width - 24f, Height - 40f), new Vector2(1f, 0.5f), Vector2.One * 0.6f, Color.Black * 0.8f);
        }

        private void DrawIcon(Vector2 position, string icon, string text)
        {
            bool flag = cursor.X > position.X - 64.0 && cursor.Y > position.Y - 64.0 && cursor.X < position.X + 64.0 && cursor.Y < position.Y + 80.0;
            if (selected & flag)
                Draw.Rect(position.X - 80f, position.Y - 80f, 160f, 200f, Color.White * 0.25f);
            if (flag)
                DrawDottedRect(position.X - 80f, position.Y - 80f, 160f, 200f);
            MTexture mtexture = Presentation.Gfx[icon];
            float scale = 128f / mtexture.Height;
            mtexture.DrawCentered(position, Color.White, scale);
            ActiveFont.Draw(text, position + new Vector2(0.0f, 80f), new Vector2(0.5f, 0.0f), Vector2.One * 0.6f, selected & flag ? Color.Black : Color.White);
        }

        private void DrawDottedRect(float x, float y, float w, float h)
        {
            float borderWidth = 4f;
            Draw.Rect(x, y, w, borderWidth, Color.White);
            Draw.Rect(x + w - borderWidth, y, borderWidth, h, Color.White);
            Draw.Rect(x, y, borderWidth, h, Color.White);
            Draw.Rect(x, y + h - borderWidth, w, borderWidth, Color.White);
            if (selected)
                return;
            for (float gapX = 4f; gapX < (double) w; gapX += borderWidth * 2f)
            {
                Draw.Rect(x + gapX, y, borderWidth, borderWidth, ClearColor);
                Draw.Rect(x + w - gapX, y + h - borderWidth, borderWidth, borderWidth, ClearColor);
            }
            for (float gapY = 4f; gapY < (double) h; gapY += borderWidth * 2f)
            {
                Draw.Rect(x, y + gapY, borderWidth, borderWidth, ClearColor);
                Draw.Rect(x + w - borderWidth, y + h - gapY, borderWidth, borderWidth, ClearColor);
            }
        }
    }
}
