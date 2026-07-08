namespace DZ
{
    [HotReloadable]
    public class WaveFazeSlammerPage02 : WaveFazeSlammerPage
    {
        private WaveFazeSlammerPlaybackTutorial tutorial;
        private FancyText.Text stepText;
        private int stepIndex;

        public WaveFazeSlammerPage02()
        {
            Transition = Transitions.Rotate3D;
            ClearColor = Calc.HexToColor("d9ead3");
        }

        public override void Added(WaveFazeSlammerPresentation presentation)
        {
            base.Added(presentation);
            tutorial = new WaveFazeSlammerPlaybackTutorial(
                "DZ/wavefazeslammer",
                new Vector2(-150f, 0f),
                new Vector2(1f, -0.4f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, -1f)
            );
            
            tutorial.OnRender = () => 
            {
                Draw.Rect(-100f, 80f, 200f, 8f, Color.Gray);
                Draw.Rect(120f, -60f, 8f, 140f, Color.Gray);
            };
        }

        public override IEnumerator Routine()
        {
            yield return 0.5f;
            stepText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE2_STEPS"), Width - 300, 32, defaultColor: Color.Black * 0.7f);
            
            float delay = 0.0f;
            for (; stepIndex < stepText.Nodes.Count; ++stepIndex)
            {
                if (stepText.Nodes[stepIndex] is FancyText.NewLine)
                {
                    yield return 0.4f;
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
            yield return 1f;
        }

        public override void Update()
        {
            if (tutorial != null)
                tutorial.Update();
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(Dialog.Clean("WAVEFAZESLAMMER_PAGE2_TITLE"), new Vector2(Width / 2f, 100f), new Vector2(0.5f, 0f), Vector2.One * 1.5f, Color.White, 2f, Color.Black);
            
            if (tutorial != null)
                tutorial.Render(new Vector2(Width / 2f, Height / 2f - 50f), 4f);
            
            if (stepText != null)
                stepText.Draw(new Vector2(150f, Height - 450), new Vector2(0.0f, 0.0f), Vector2.One, 1f, end: stepIndex);
        }
    }
}
