namespace MaggyHelper
{
    [HotReloadable]
    public class WaveFazeSlammerPage01 : WaveFazeSlammerPage
    {
        private FancyText.Text infoText;
        private int infoIndex;

        public WaveFazeSlammerPage01()
        {
            Transition = Transitions.FadeIn;
            ClearColor = Calc.HexToColor("f4cccc");
        }

        public override IEnumerator Routine()
        {
            yield return 0.5f;
            infoText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE1_INFO"), Width - 240, 32, defaultColor: Color.Black * 0.7f);
            
            float delay = 0.0f;
            for (; infoIndex < infoText.Nodes.Count; ++infoIndex)
            {
                if (infoText.Nodes[infoIndex] is FancyText.NewLine)
                {
                    yield return 0.3f;
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
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(Dialog.Clean("WAVEFAZESLAMMER_PAGE1_TITLE"), new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
            if (infoText != null)
                infoText.Draw(new Vector2(120f, 300f), new Vector2(0.0f, 0.0f), Vector2.One, 1f, end: infoIndex);
        }
    }
}
