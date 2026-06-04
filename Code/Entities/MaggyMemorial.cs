namespace Celeste.Entities
{
    /// <summary>
    /// A custom Memorial entity for Desolo Zantas that displays configurable dialog
    /// text when the player walks into its hitbox.
    /// Supports normal and dreamy text styles, custom sprites, and audio.
    /// Place via LÃ¶nn as "MaggyHelper/MaggyMemorial".
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/MaggyMemorial")]
    [Tracked]
    public class MaggyMemorial : Entity
    {
        // Visual
        private Image sprite;
        private Sprite dreamyText;    // animated floaty sparkle above the memorial (dreamy only)

        // The companion HUD text entity
        private MaggyMemorialText text;

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
        public MaggyMemorial(EntityData data, Vector2 offset)
            : this(
                data.Position + offset,
                data.Attr("dialogKey", "MAGGY_MEMORIAL_DEFAULT"),
                data.Attr("spritePath", "scenery/memorial/memorial"),
                data.Bool("dreamy", false))
        {
        }

        /// <summary>
        /// Core constructor.
        /// </summary>
        public MaggyMemorial(Vector2 position, string dialogKey, string spritePath, bool dreamy)
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
            text = new MaggyMemorialText(this, isDreaming, dialogKey);
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
                        ? "guid://{037333ef-5916-4131-a76c-77b8d31c21cd}"
                        : "guid://{27c68b11-4893-406e-8a68-2c7cf6a7ae0d}",
                    Position);

                if (text.Dreamy)
                    loopingSfx.Play("guid://{6e6a7734-f98c-45cf-930f-61c51f193827}");
            }
            else if (!showing && wasShowing)
            {
                Audio.Play(
                    text.Dreamy
                        ? "guid://{78a27ccb-8b29-41e9-aa60-20e4d193f41d}"
                        : "guid://{812275c5-1e5a-4d69-98e1-f7892c4bb440}",
                    Position);

                loopingSfx.Stop();
            }

            wasShowing = showing;
        }
    }
}

