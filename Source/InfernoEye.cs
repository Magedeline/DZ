namespace MaggyHelper.Entities
{
    /// <summary>
    /// A tracking eye entity for Chapter 7 (Infernal Reflections).
    /// Follows the player with its pupil, blinks periodically, and can burst.
    /// Automatically detects whether it's in a background or foreground position.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/InfernoEye")]
    [Tracked]
    public class InfernoEye : Entity
    {
        private MTexture eyeTexture;
        private MTexture pupilTexture;
        private Sprite eyelid;
        private Vector2 pupilPosition;
        private Vector2 pupilTarget;
        private float blinkTimer;
        private bool bursting;
        private bool isBG;

        public InfernoEye(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Determine if this is a background or foreground eye
            isBG = !scene.CollideCheck<Solid>(Position);

            string prefix = isBG
                ? "scenery/temple/eye/bg_"
                : "scenery/temple/eye/fg_";

            eyeTexture = GFX.Game[prefix + "eye"];
            pupilTexture = GFX.Game[prefix + "pupil"];

            Add(eyelid = new Sprite(GFX.Game, prefix + "lid"));
            Depth = isBG ? 8990 : -10001;

            eyelid.AddLoop("open", "", 0f, default(int));
            eyelid.Add("blink", "", 0.08f, "open", 0, 1, 1, 2, 3, 0);
            eyelid.Play("open");
            eyelid.CenterOrigin();

            SetBlinkTimer();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            // Track the player instead of TheoCrystal
            CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();
            if (player != null)
            {
                pupilTarget = (player.Center - Position).SafeNormalize();
                pupilPosition = pupilTarget * 3f;
            }
        }

        public override void Update()
        {
            if (!bursting)
            {
                pupilPosition = Calc.Approach(pupilPosition, pupilTarget * 3f,
                    Engine.DeltaTime * 16f);

                CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();
                if (player != null)
                {
                    pupilTarget = (player.Center - Position).SafeNormalize();

                    // Rare random blink while tracking
                    if (Scene.OnInterval(0.25f) && Calc.Random.Chance(0.01f))
                        eyelid.Play("blink");
                }

                blinkTimer -= Engine.DeltaTime;
                if (blinkTimer <= 0f)
                {
                    SetBlinkTimer();
                    eyelid.Play("blink");
                }
            }

            base.Update();
        }

        /// <summary>
        /// Causes the eye to burst and remove itself from the scene.
        /// </summary>
        public void Burst()
        {
            bursting = true;

            string burstPrefix = isBG
                ? "scenery/temple/eye/bg_burst"
                : "scenery/temple/eye/fg_burst";

            Sprite burstSprite = new Sprite(GFX.Game, burstPrefix);
            burstSprite.Add("burst", "", 0.08f);
            burstSprite.Play("burst");
            burstSprite.OnLastFrame = _ => RemoveSelf();
            burstSprite.CenterOrigin();

            Add(burstSprite);
            Remove(eyelid);
        }

        public override void Render()
        {
            if (!bursting)
            {
                eyeTexture.DrawCentered(Position);
                pupilTexture.DrawCentered(Position + pupilPosition);
            }
            base.Render();
        }

        private void SetBlinkTimer()
        {
            blinkTimer = Calc.Random.Range(1f, 15f);
        }
    }
}
