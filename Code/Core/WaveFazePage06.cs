namespace Celeste
{
    [HotReloadable]
    public class WaveFazePage06 : WaveFazePage
    {
        private AreaCompleteTitle title;

        public WaveFazePage06()
        {
            Transition = Transitions.Rotate3D;
            ClearColor = Calc.HexToColor("d9d2e9");
        }

        public override IEnumerator Routine()
        {
            WaveFazePage06 waveFazePage06 = this;
            yield return 1f;
            Audio.Play("event:/new_content/game/10_farewell/ppt_happy_wavedashing");
            waveFazePage06.title = new AreaCompleteTitle(new Vector2(waveFazePage06.Width / 2f, 150f), Dialog.Clean("WAVEFAZE_PAGE6_TITLE"), 2f, true);
            yield return 1.5f;
        }

        public override void Update()
        {
            if (title == null)
                return;
            title.Update();
        }

        public override void Render()
        {
            if (Presentation?.Gfx != null)
                Presentation.Gfx["Dog Clip Art"].DrawCentered(new Vector2(Width, Height) / 2f, Color.White, 1.5f);
            if (title == null)
                return;
            title.Render();
        }
    }
}




