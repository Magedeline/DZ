namespace Celeste.Entities
{
    /// <summary>
    /// A custom Memorial entity for Desolo Zantas that displays configurable dialog
    /// text when the player walks into its hitbox.
    /// Supports normal and dreamy text styles, custom sprites, and audio.
    /// Place via LÃ¶nn as "DZ/DZMemorial".
    /// </summary>
    [CustomEntity(ids: "DZ/DZMemorial")]
    [Tracked]
    public class DZMemorial : Entity
    {
        // Visual
        private Image sprite;
        private Sprite dreamyText;    // animated floaty sparkle above the memorial (dreamy only)

        // The companion HUD text entity
        private DZMemorialText text;

        // Whether we were showing text last frame (for SFX transitions)
        private bool wasShowing;

        // Looping ambient SFX while text is shown in dreamy mode
        private SoundSource loopingSfx;

        // â”€â”€ Configuration â”€â”€
        private string dialogKey;
        private string spritePath;
        private bool dreamy;

        /// <summary>
        /// LÃ¶nn / map-loader constructor.
        /// </summary>
        public DZMemorial(EntityData data, Vector2 offset)
            : this(
                data.Position + offset,
                data.Attr("dialogKey", "DZ_MEMORIAL_DEFAULT"),
                data.Attr("spritePath", "scenery/memorial/memorial"),
                data.Bool("dreamy", false))
        {
        }

        /// <summary>
        /// Core constructor.
        /// </summary>
        public DZMemorial(Vector2 position, string dialogKey, string spritePath, bool dreamy)
            : base(position)
        {
            this.dialogKey = dialogKey;
            this.spritePath = spritePath;
            this.dreamy = dreamy;

            Tag = Tags.PauseUpdate;
            Depth = 100;

            // Load the sprite
            sprite = new Image(GFX.Game[spritePath]);
            sprite.Origin = new Vector2(sprite.Width / 2f, sprite.Height);
            Add(sprite);

            // Player-detection hitbox (centered horizontally, extends upward from the base)
            Collider = new Hitbox(60f, 80f, -30f, -60f);

            Add(loopingSfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            Level level = scene as Level;
            if (level == null) return;

            bool isDreaming = dreamy || level.Session.Dreaming;

            // Spawn the companion HUD-text entity
            text = new DZMemorialText(this, isDreaming, dialogKey);
            level.Add(text);

            // If dreamy, add the animated "floatytext" sparkle above the memorial
            if (isDreaming && GFX.Game.Has("scenery/memorial/floatytext00"))
            {
                dreamyText = new Sprite(GFX.Game, "scenery/memorial/floatytext");
                dreamyText.AddLoop("dreamy", "", 0.1f);
                dreamyText.Position = new Vector2(-sprite.Width / 2f, -33f);
                Add(dreamyText);
                dreamyText.Play("dreamy");
            }
        }

        public override void Update()
        {
            base.Update();

            Level level = SceneAs<Level>();
            if (level == null) return;

            // Pause the looping SFX while the game is paused
            if (level.Paused)
            {
                loopingSfx.Pause();
                return;
            }
            loopingSfx.Resume();

            // Check player proximity (collider overlap)
            CelestePlayer player = level.Tracker.GetEntity<CelestePlayer>();
            bool showing = player != null && CollideCheck(player);
            text.Show = showing;

            // SFX transitions
            if (showing && !wasShowing)
            {
                Audio.Play(
                    text.Dreamy
                        ? "event:/ui/game/memorial_dream_text_in"
                        : "event:/ui/game/memorial_text_in",
                    Position);

                if (text.Dreamy)
                    loopingSfx.Play("event:/ui/game/memorial_dream_loop");
            }
            else if (!showing && wasShowing)
            {
                Audio.Play(
                    text.Dreamy
                        ? "event:/ui/game/memorial_dream_text_out"
                        : "event:/ui/game/memorial_text_out",
                    Position);

                loopingSfx.Stop();
            }

            wasShowing = showing;
        }
    }
}

