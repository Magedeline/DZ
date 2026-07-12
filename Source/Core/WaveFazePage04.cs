namespace DZ
{
    [HotReloadable]
    public class WaveFazePage04 : WaveFazePage
    {
        private WaveFazePlaybackTutorial tutorial;
        private FancyText.Text list;
        private int listIndex;
        private float time;

        public WaveFazePage04()
        {
            Transition = Transitions.FadeIn;
            ClearColor = Calc.HexToColor("f4cccc");
        }

        public override void Added(WaveFazePresentation presentation)
        {
            base.Added(presentation);
            List<MTexture> textures = Presentation.Gfx?.GetAtlasSubtextures("playback/platforms") ?? new List<MTexture>();
            tutorial = new WaveFazePlaybackTutorial("DZ/wavefazingppt", new Vector2(-189f, 0.0f), new Vector2(-189f, 0.0f), new Vector2(126f, 0.0f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            tutorial.OnRender = () => {
                if (textures.Count > 0)
                    textures[(int)(time % textures.Count)].DrawCentered(Vector2.Zero);
            };
        }

        public override IEnumerator Routine()
        {
            yield return 0.5f;

            string[] listKeys = new string[]
            {
                "WAVEFAZE_PAGE4_LIST_A",
                "WAVEFAZE_PAGE4_LIST_B",
                "WAVEFAZE_PAGE4_LIST_C",
                "WAVEFAZE_PAGE4_LIST_D",
                "WAVEFAZE_PAGE4_LIST_E",
                "WAVEFAZE_PAGE4_LIST_F"
            };

            foreach (string key in listKeys)
            {
                list = FancyText.Parse(Dialog.Get(key), Width, 32, defaultColor: Color.Black * 0.7f);
                listIndex = 0;
                float delay = 0f;
                for (; listIndex < list.Nodes.Count; ++listIndex)
                {
                    delay += 0.008f;
                    if (delay >= 0.016f)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
                yield return PressButton();
            }
        }

        public override void Update()
        {
            time += Engine.DeltaTime * 4f;
            tutorial.Update();
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(Dialog.Clean("WAVEFAZE_PAGE4_TITLE"), new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
            tutorial.Render(new Vector2(Width / 2f, (float)(Height / 2.0 - 100.0)), 4f);
            if (list == null)
                return;
            list.Draw(new Vector2(160f, Height - 400), new Vector2(0.0f, 0.0f), Vector2.One, 1f, end: listIndex);
        }
    }
}





