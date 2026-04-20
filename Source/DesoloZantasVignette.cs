namespace Celeste.Cutscenes
{
    /// <summary>
    /// Base class for all DesoloZantas vignette scenes (intro cutscenes, outro
    /// cutscenes, vessel creation, etc.).
    ///
    /// Extending this instead of <see cref="Scene"/> directly allows
    /// <see cref="MaggyHelper.DesoloZantas.PauseAnywhere"/> to detect any
    /// mod vignette with a compile-time safe <c>is DesoloZantasVignette</c>
    /// pattern match, without reflection or string-based type checks.
    ///
    /// Subclasses should override <see cref="CanPause"/> to control whether
    /// the pause menu may be opened (e.g. return false during unskippable
    /// sequences), and override <see cref="OpenMenu"/> to show their specific
    /// pause UI.
    /// </summary>
    [HotReloadable]
    public abstract class DesoloZantasVignette : Scene
    {
        // ── Pause support ──────────────────────────────────────────────────────────

        /// <summary>
        /// Whether this vignette currently allows the player to open the pause menu.
        /// Defaults to <c>true</c>; override to restrict during unskippable moments.
        /// </summary>
        public virtual bool CanPause => true;

        /// <summary>
        /// Whether this vignette is currently paused.
        /// Subclasses should use this flag to halt coroutines / animations.
        /// </summary>
        public new bool Paused { get; protected set; }

        /// <summary>
        /// Opens the pause menu for this vignette.
        /// The base implementation sets <see cref="Paused"/> = <c>true</c> and
        /// adds a <see cref="TextMenu"/> that lets the player resume or quit.
        /// Override to provide a custom UI.
        /// </summary>
        public virtual void OpenMenu()
        {
            if (!CanPause || Paused)
                return;

            Paused = true;
            Add(BuildDefaultMenu());
        }

        /// <summary>
        /// Closes the pause menu and resumes the vignette.
        /// </summary>
        public virtual void CloseMenu()
        {
            Paused = false;
        }

        // ── Scene lifecycle ────────────────────────────────────────────────────────

        public override void Update()
        {
            base.Update();

            // Let subclasses respond to the pause button without duplicating input
            // polling — they can check Input.Pause themselves or call OpenMenu().
            if (!Paused && CanPause && Input.Pause.Pressed)
            {
                Input.Pause.ConsumePress();
                OpenMenu();
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a minimal pause <see cref="TextMenu"/> with Resume and Quit options.
        /// Called by <see cref="OpenMenu"/> when no custom menu is provided.
        /// </summary>
        protected virtual TextMenu BuildDefaultMenu()
        {
            TextMenu menu = new TextMenu();
            menu.Add(new TextMenu.Header(Dialog.Clean("MENU_PAUSE")));
            menu.Add(new TextMenu.Button(Dialog.Clean("MENU_RESUME")).Pressed(() =>
            {
                menu.RemoveSelf();
                CloseMenu();
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("MENU_SAVEQUIT")).Pressed(() =>
            {
                menu.RemoveSelf();
                Engine.Scene = new LevelExit(LevelExit.Mode.SaveAndQuit, null);
            }));
            return menu;
        }
    }
}
