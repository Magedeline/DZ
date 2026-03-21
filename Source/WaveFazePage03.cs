namespace MaggyHelper
{
    [HotReloadable]
    public class WaveFazePage03 : WaveFazePage
    {
        private string title;
        private string titleDisplayed;
        private MTexture clipArt;
        private float clipArtEase;
        private FancyText.Text infoText;
        private AreaCompleteTitle easyText;

        public WaveFazePage03()
        {
            Transition = Transitions.Blocky;
            ClearColor = Calc.HexToColor("d9ead3");
            title = Dialog.Clean("WAVEFAZE_PAGE3_TITLE");
            titleDisplayed = "";
        }

        public override void Added(WaveFazePresentation presentation)
        {
            base.Added(presentation);
            clipArt = presentation.Gfx["moveset"];
        }

        public override IEnumerator Routine()
        {
            WaveFazePage03 waveFazePage03 = this;
            while (waveFazePage03.titleDisplayed.Length < waveFazePage03.title.Length)
            {
                waveFazePage03.titleDisplayed += waveFazePage03.title[waveFazePage03.titleDisplayed.Length].ToString();
                yield return 0.05f;
            }
            yield return waveFazePage03.PressButton();
            Audio.Play("event:/new_content/game/10_farewell/ppt_wavedash_whoosh");
            while (waveFazePage03.clipArtEase < 1.0)
            {
                waveFazePage03.clipArtEase = Calc.Approach(waveFazePage03.clipArtEase, 1f, Engine.DeltaTime);
                yield return null;
            }
            yield return 0.25f;
            waveFazePage03.infoText = FancyText.Parse(Dialog.Get("WAVEFAZE_PAGE3_INFO"), waveFazePage03.Width - 240, 32, defaultColor: Color.Black * 0.7f);
            yield return waveFazePage03.PressButton();
            Audio.Play("event:/new_content/game/10_farewell/ppt_its_easy");
            waveFazePage03.easyText = new AreaCompleteTitle(new Vector2(waveFazePage03.Width / 2f, waveFazePage03.Height - 150), Dialog.Clean("WAVEFAZE_PAGE3_EASY"), 2f, true);
            yield return 1f;
        }

        public override void Update()
        {
            if (easyText == null)
                return;
            easyText.Update();
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(titleDisplayed, new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
            if (clipArtEase > 0.0)
                clipArt.DrawCentered(new Vector2(Width / 2f, (float) (Height / 2.0 - 90.0)), Color.White * clipArtEase, Vector2.One * (float) (1.0 + (1.0 - clipArtEase) * 3.0) * 0.8f, (float) ((1.0 - clipArtEase) * 8.0));
            if (infoText != null)
                infoText.Draw(new Vector2(Width / 2f, Height - 350), new Vector2(0.5f, 0.0f), Vector2.One, 1f);
            if (easyText == null)
                return;
            easyText.Render();
        }
    }
}




