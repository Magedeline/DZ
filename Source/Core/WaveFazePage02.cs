namespace DZ
{
    [HotReloadable]
    public class WaveFazePage02 : WaveFazePage
    {
        private List<TitleText> title = new List<TitleText>();
        private FancyText.Text list;
        private int listIndex;
        private float impossibleEase;

        public WaveFazePage02()
        {
            Transition = Transitions.Rotate3D;
            ClearColor = Calc.HexToColor("fff2cc");
        }

        public override void Added(WaveFazePresentation presentation) => base.Added(presentation);

        public override IEnumerator Routine()
        {
            string[] text = Dialog.Clean("WAVEFAZE_PAGE2_TITLE").Split('|');
            Vector2 pos = new Vector2(128f, 128f);
            for (int i = 0; i < text.Length; ++i)
            {
                TitleText item = new TitleText(pos, text[i]);
                title.Add(item);
                yield return item.Stamp();
                pos.X += item.Width + ActiveFont.Measure(' ').X * 1.5f;
            }
            yield return PressButton();
            list = FancyText.Parse(Dialog.Get("WAVEFAZE_PAGE2_LIST"), Width, 32, 1f, Color.Black * 0.7f);
            float delay = 0.0f;
            for (; listIndex < list.Nodes.Count; ++listIndex)
            {
                if (list.Nodes[listIndex] is FancyText.NewLine)
                {
                    yield return PressButton();
                }
                else
                {
                    delay += 0.008f;
                    if (delay >= 0.016f)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
            }
            yield return PressButton();
            Audio.Play("event:/new_content/game/10_farewell/ppt_impossible");
            while (impossibleEase < 1f)
            {
                impossibleEase = Calc.Approach(impossibleEase, 1f, Engine.DeltaTime);
                yield return null;
            }
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            foreach (TitleText titleText in title)
                titleText.Render();
            if (list != null)
                list.Draw(new Vector2(160f, 260f), new Vector2(0.0f, 0.0f), Vector2.One, 1f, 0, listIndex);
            if (impossibleEase <= 0.0f || Presentation?.Gfx == null)
                return;
            MTexture mtexture = Presentation.Gfx["Guy Clip Art"];
            float scale = 0.75f;
            mtexture.Draw(new Vector2(Width - mtexture.Width * scale, Height - 640f * impossibleEase), Vector2.Zero, Color.White, scale);
            Matrix transformMatrix = Matrix.CreateRotationZ(-0.5f + Ease.CubeIn(1f - impossibleEase) * 8f) * Matrix.CreateTranslation(Width - 500, Height - 600, 0.0f);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformMatrix);
            ActiveFont.Draw(Dialog.Clean("WAVEFAZE_PAGE2_IMPOSSIBLE"), Vector2.Zero, new Vector2(0.5f, 0.5f), Vector2.One * (2f + (1f - impossibleEase) * 0.5f), Color.Black * impossibleEase);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin();
        }

        private class TitleText
        {
            public const float Scale = 1.5f;
            public string Text;
            public Vector2 Position;
            public float Width;
            private float ease;

            public TitleText(Vector2 pos, string text)
            {
                Position = pos;
                Text = text;
                Width = ActiveFont.Measure(text).X * 1.5f;
            }

            public IEnumerator Stamp()
            {
                while (ease < 1f)
                {
                    ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 4f);
                    yield return null;
                }
                yield return 0.2f;
            }

            public void Render()
            {
                if (ease <= 0.0f)
                    return;
                ActiveFont.DrawOutline(Text, Position + new Vector2(Width / 2f, ActiveFont.LineHeight * 0.5f * 1.5f), new Vector2(0.5f, 0.5f), Vector2.One * (1f + (1f - Ease.CubeOut(ease))) * 1.5f, Color.White, 2f, Color.Black);
            }
        }
    }
}
