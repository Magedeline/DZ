namespace Celeste
{
    [HotReloadable]
    public abstract class WaveFazePage
    {
        public WaveFazePresentation Presentation;
        public Color ClearColor;
        public Transitions Transition;
        public bool AutoProgress;
        public bool WaitingForInput;

        public int Width => Presentation.ScreenWidth;

        public int Height => Presentation.ScreenHeight;

        public abstract IEnumerator Routine();

        public virtual void Added(WaveFazePresentation presentation) => Presentation = presentation;

        public abstract void Update();

        public virtual void Render()
        {
        }

        protected IEnumerator PressButton()
        {
            WaitingForInput = true;
            while (!Input.MenuConfirm.Pressed)
                yield return null;
            WaitingForInput = false;
            Audio.Play("guid://{5c9780f7-de2d-4811-813e-f89201d92e35}");
        }

        public enum Transitions
        {
            ScaleIn,
            FadeIn,
            Rotate3D,
            Blocky,
            Spiral,
        }
    }
}





