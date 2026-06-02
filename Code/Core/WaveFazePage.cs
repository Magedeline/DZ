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
            Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
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




