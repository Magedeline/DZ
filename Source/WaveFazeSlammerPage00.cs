namespace Celeste
{
    [HotReloadable]
    public class WaveFazeSlammerPage00 : WaveFazeSlammerPage
    {
        private string title;
        private string titleDisplayed;

        public WaveFazeSlammerPage00()
        {
            Transition = Transitions.ScaleIn;
            ClearColor = Calc.HexToColor("fce5cd");
            title = Dialog.Clean("WAVEFAZESLAMMER_PAGE0_TITLE");
            titleDisplayed = "";
        }

        public override IEnumerator Routine()
        {
            yield return 0.5f;
            while (titleDisplayed.Length < title.Length)
            {
                titleDisplayed += title[titleDisplayed.Length].ToString();
                yield return 0.05f;
            }
            yield return PressButton();
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(titleDisplayed, new Vector2(Width / 2f, Height / 3f), new Vector2(0.5f, 0.5f), Vector2.One * 2f, Color.White, 2f, Color.Black);
            ActiveFont.DrawOutline(Dialog.Clean("WAVEFAZESLAMMER_PAGE0_SUBTITLE"), new Vector2(Width / 2f, Height / 2f), new Vector2(0.5f, 0.5f), Vector2.One * 1.2f, Color.Black * 0.7f, 2f, Color.White * 0.3f);
        }
    }
}
