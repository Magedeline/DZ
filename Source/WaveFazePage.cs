namespace MaggyHelper
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
            // Note: SpriteBatch.Begin/End is handled by WaveFazePresentation.BeforeRender()
            // Derived classes should override this method and render directly
            if (Presentation != null)
            {
                // Default rendering: show page title if text exists
                ActiveFont.DrawOutline(Dialog.Clean("WAVEFAZE_PAGE_TITLE"), new Vector2(Width / 2f, Height / 4f),
                    new Vector2(0.5f, 0.5f), Vector2.One, Color.White, 2f, Color.Black);
            }
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




