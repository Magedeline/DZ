namespace DZ
{
    [HotReloadable]
    public class WaveFazePage01 : WaveFazePage
    {
        private AreaCompleteTitle title;
        private float subtitleEase;

        public WaveFazePage01()
        {
            Transition = Transitions.ScaleIn;
            ClearColor = Calc.HexToColor("9fc5e8");
        }

        public override void Added(WaveFazePresentation presentation) => base.Added(presentation);

        public override IEnumerator Routine()
        {
            WaveFazePage01 waveFazePage01 = this;
            Audio.SetAltMusic("event:/pusheen/new_content/music/lvl19/intermission_toby_slideshow");
            yield return 1f;
            waveFazePage01.title = new AreaCompleteTitle(new Vector2(waveFazePage01.Width / 2f, (float) (waveFazePage01.Height / 2.0 - 100.0)), Dialog.Clean("WAVEFAZE_PAGE1_TITLE"), 2f, true);
            yield return 1f;
            while (waveFazePage01.subtitleEase < 1.0)
            {
                waveFazePage01.subtitleEase = Calc.Approach(waveFazePage01.subtitleEase, 1f, Engine.DeltaTime);
                yield return null;
            }
            yield return 0.1f;
        }

        public override void Update()
        {
            if (title == null)
                return;
            title.Update();
        }

        public override void Render()
        {
            if (title != null)
                title.Render();
            if (subtitleEase <= 0.0)
                return;
            Vector2 position = new Vector2(Width / 2f, (float) (Height / 2.0 + 80.0));
            float x = (float) (1.0 + Ease.BigBackIn(1f - subtitleEase) * 2.0);
            float y = (float) (0.25 + Ease.BigBackIn(subtitleEase) * 0.75);
            ActiveFont.Draw(Dialog.Clean("WAVEFAZE_PAGE1_SUBTITLE"), position, new Vector2(0.5f, 0.5f), new Vector2(x, y), Color.Black * 0.8f);
        }
    }
}




