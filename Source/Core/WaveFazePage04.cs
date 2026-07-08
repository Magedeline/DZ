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
            List<MTexture> textures = Presentation.Gfx.GetAtlasSubtextures("playback/platforms");
            tutorial = new WaveFazePlaybackTutorial("DZ/wavefazingppt", new Vector2(-189f, 0.0f), new Vector2(-189f, 0.0f), new Vector2(126f, 0.0f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            tutorial.OnRender = () => {
                if (textures.Count > 0)
                    textures[(int)(time % textures.Count)].DrawCentered(Vector2.Zero);
            };
        }

        public override IEnumerator Routine()
        {
            WaveFazePage04 waveFazePage04 = this;
            yield return 0.5f;
            waveFazePage04.list = FancyText.Parse(Dialog.Get("WAVEFAZE_PAGE4_LIST"), waveFazePage04.Width, 32, defaultColor: Color.Black * 0.7f);
            float delay = 0.0f;
            for (; waveFazePage04.listIndex < waveFazePage04.list.Nodes.Count; ++waveFazePage04.listIndex)
            {
                if (waveFazePage04.list.Nodes[waveFazePage04.listIndex] is FancyText.NewLine)
                {
                    yield return waveFazePage04.PressButton();
                }
                else
                {
                    delay += 0.008f;
                    if (delay >= 0.016000000759959221)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
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





