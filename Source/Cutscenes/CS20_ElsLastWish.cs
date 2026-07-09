using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ
{
    /// <summary>
    /// Cutscene for Chapter 20: Els Last Wish
    /// Handles the brutal sequence where Els gives Kirby a final wish,
    /// hears Kirby's heartfelt speech, then punishes him violently before
    /// ripping out and shattering his heart.
    /// </summary>
    [Tracked(true)]
    public class CS20_ElsLastWish : CutsceneEntity
    {
        #region Constants

        private const string DIALOGUE_KEY = "DZ_CH20_ELS_LAST_WISH";

        // Session flags
        private const string FLAG_ELS_LAST_WISH_DONE = "ch20_els_last_wish_done";

        // SFX
        private const string SFX_PUNCH_IMPACT      = "event:/DZ/new_content/game/20_last_push/els_punch_impact";
        private const string SFX_HEART_PULLED       = "event:/DZ/new_content/game/20_last_push/els_heart_pull";
        private const string SFX_HEART_SHATTERED    = "event:/DZ/new_content/game/20_last_push/els_heart_shatter";
        private const string SFX_TESSERACT_GRAB     = "event:/DZ/new_content/game/20_last_push/tesseract_grab";
        private const string SFX_DARK_AMBIENCE      = "event:/DZ/new_content/game/20_last_push/els_last_wish_ambience";

        #endregion

        #region Fields

        private Player player;
        private Level level;

        // Dummy entity used to represent Kirby suspended in the air
        private Entity kirbyDummy;

        // Tracks number of punches landed for escalating shake
        private int punchCount = 0;

        #endregion

        #region Constructor

        public CS20_ElsLastWish(Player player)
        {
            this.player = player;
        }

        /// <summary>
        /// Static factory method Ã¢â‚¬â€ call this to begin the cutscene from a trigger.
        /// </summary>
        public static void Start(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            if (player == null)
                return;

            CS20_ElsLastWish cutscene = new CS20_ElsLastWish(player);
            level.Add(cutscene);
        }

        #endregion

        #region Cutscene Lifecycle

        public override void OnBegin(Level level)
        {
            this.level = level;
            Add(new Coroutine(CutsceneSequence()));
        }

        public override void OnEnd(Level level)
        {
            // Restore player if the cutscene was skipped
            if (player != null && player.StateMachine.State == Player.StDummy)
            {
                player.StateMachine.State = Player.StNormal;
                player.DummyAutoAnimate = true;
            }

            // Remove any lingering dummy
            if (kirbyDummy != null)
            {
                kirbyDummy.RemoveSelf();
                kirbyDummy = null;
            }

            Glitch.Value = 0f;
            level.Session.SetFlag(FLAG_ELS_LAST_WISH_DONE);
        }

        #endregion

        #region Main Sequence

        private IEnumerator CutsceneSequence()
        {
            // Lock player into dummy state; Els controls everything from here.
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate = false;

            // Fade music to a dark ambient layer
            Audio.Play(SFX_DARK_AMBIENCE);

            // Run the full dialogue.
            // Triggers are passed as coroutines in the same order they appear
            // in the dialogue file (trigger 0..10).
            yield return Textbox.Say(
                DIALOGUE_KEY,
                Trigger0_TesseractGrabsKirby,   // {trigger 0 Tesseract grab Kirby and pull him}
                null,                            // trigger 1 Ã¢â‚¬â€ unused gap
                Trigger2_PunchOne,               // {trigger 2 Els punch Kirby one}
                Trigger3_PunchTwo,               // {trigger 3 Els punch Kirby two}
                Trigger4_PunchThree,             // {trigger 4 Els punch Kirby three}
                Trigger5_PunchFour,              // {trigger 5 Els punch Kirby four}
                Trigger6_PunchFive,              // {trigger 6 Els punch Kirby five}
                Trigger7_PunchFinal,             // {trigger 7 Els punch Kirby final}
                Trigger8_PullHeartOut,           // {trigger 8 Els pull Kirby heart out}
                Trigger9_ShatterHeart,           // {trigger 9 Els crushed Kirby heart and shattered it}
                Trigger10_DropKirby             // {trigger 10 Els Let Go and Drop Kirby}
            );

            EndCutscene(level);
        }

        #endregion

        #region Trigger Methods

        /// <summary>
        /// Trigger 0 Ã¢â‚¬â€ Tesseract materialises and levitates Kirby helplessly into the air.
        /// </summary>
        private IEnumerator Trigger0_TesseractGrabsKirby()
        {
            Audio.Play(SFX_TESSERACT_GRAB, player.Position);
            level.Shake(0.3f);

            // Spawn a visual "grabbed" dummy at Kirby's current position
            kirbyDummy = new Entity(player.Position);
            level.Add(kirbyDummy);

            // Hide the real player sprite while Els drags him
            player.Visible = false;

            // Smoothly float Kirby upward ~64px over 1.2 seconds
            Vector2 startPos = player.Position;
            Vector2 endPos   = startPos + new Vector2(0f, -64f);
            float duration   = 1.2f;

            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                float progress = Ease.CubeOut(t / duration);
                player.Position = Vector2.Lerp(startPos, endPos, progress);
                if (kirbyDummy != null)
                    kirbyDummy.Position = player.Position;
                yield return null;
            }

            player.Position = endPos;

            // Glitch the screen to sell the Tesseract's spatial distortion
            Glitch.Value = 0.4f;
            yield return 0.3f;
            Glitch.Value = 0f;
        }

        /// <summary>
        /// Generic reusable punch helper Ã¢â‚¬â€ shakes the screen, plays a sound,
        /// and jolts Kirby's suspended position.
        /// </summary>
        private IEnumerator PunchSequence(float shakeStrength)
        {
            punchCount++;
            Audio.Play(SFX_PUNCH_IMPACT, player.Position);
            level.Shake(shakeStrength);

            // Brief position jolt to sell impact
            Vector2 originalPos = player.Position;
            player.Position += new Vector2(Calc.Random.Choose(-1, 1) * 6f, -4f);
            yield return 0.08f;
            player.Position = originalPos;

            // Flash the screen a dark crimson on harder hits
            if (punchCount >= 4)
                level.Flash(Color.DarkRed * 0.25f, drawPlayerOver: false);

            yield return 0.2f;
        }

        /// <summary>Trigger 2 Ã¢â‚¬â€ Els punch Kirby one</summary>
        private IEnumerator Trigger2_PunchOne()   => PunchSequence(0.25f);

        /// <summary>Trigger 3 Ã¢â‚¬â€ Els punch Kirby two</summary>
        private IEnumerator Trigger3_PunchTwo()   => PunchSequence(0.30f);

        /// <summary>Trigger 4 Ã¢â‚¬â€ Els punch Kirby three</summary>
        private IEnumerator Trigger4_PunchThree() => PunchSequence(0.35f);

        /// <summary>Trigger 5 Ã¢â‚¬â€ Els punch Kirby four</summary>
        private IEnumerator Trigger5_PunchFour()  => PunchSequence(0.40f);

        /// <summary>Trigger 6 Ã¢â‚¬â€ Els punch Kirby five</summary>
        private IEnumerator Trigger6_PunchFive()  => PunchSequence(0.50f);

        /// <summary>Trigger 7 Ã¢â‚¬â€ Els final devastating punch</summary>
        private IEnumerator Trigger7_PunchFinal()
        {
            Audio.Play(SFX_PUNCH_IMPACT, player.Position);
            level.Shake(0.8f);
            level.Flash(Color.Red * 0.4f, drawPlayerOver: false);
            Glitch.Value = 0.6f;

            // Heavy recoil Ã¢â‚¬â€ Kirby gets thrown back
            Vector2 originalPos = player.Position;
            player.Position += new Vector2(-20f, 10f);
            yield return 0.12f;

            // Slowly drift back to centre
            Vector2 thrownPos = player.Position;
            float recoverTime = 0.5f;
            for (float t = 0f; t < recoverTime; t += Engine.DeltaTime)
            {
                player.Position = Vector2.Lerp(thrownPos, originalPos, Ease.CubeOut(t / recoverTime));
                yield return null;
            }
            player.Position = originalPos;

            Glitch.Value = 0f;
            yield return 0.3f;
        }

        /// <summary>Trigger 8 Ã¢â‚¬â€ Els reaches into Kirby's chest and pulls out his heart.</summary>
        private IEnumerator Trigger8_PullHeartOut()
        {
            Audio.Play(SFX_HEART_PULLED, player.Position);
            level.Shake(0.5f);
            Glitch.Value = 0.5f;

            // Slow, dreadful pause to let the action sink in
            yield return 1.5f;

            Glitch.Value = 0f;
        }

        /// <summary>Trigger 9 Ã¢â‚¬â€ Els crushes and shatters Kirby's heart.</summary>
        private IEnumerator Trigger9_ShatterHeart()
        {
            Audio.Play(SFX_HEART_SHATTERED, player.Position);

            // Violent full-screen flash and extended shake for the most impactful moment
            level.Flash(Color.White, drawPlayerOver: false);
            level.Shake(1.0f);
            Glitch.Value = 1.0f;

            yield return 0.1f;

            level.Flash(Color.DarkRed * 0.6f, drawPlayerOver: false);
            Glitch.Value = 0.7f;

            yield return 0.5f;

            // Particles bursting outward
            Vector2 heartPos = player.Position + new Vector2(0f, -16f);
            for (int i = 0; i < 40; i++)
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                level.ParticlesFG.Emit(
                    new ParticleType
                    {
                        Color      = Color.Red,
                        Color2     = Color.DarkRed,
                        Size       = 1f,
                        SpeedMin   = 40f,
                        SpeedMax   = 120f,
                        LifeMin    = 0.5f,
                        LifeMax    = 1.0f,
                        DirectionRange = MathHelper.TwoPi
                    },
                    heartPos,
                    angle
                );
            }

            yield return 0.8f;
            Glitch.Value = 0f;
        }

        /// <summary>Trigger 10 Ã¢â‚¬â€ Els releases Kirby and lets him fall.</summary>
        private IEnumerator Trigger10_DropKirby()
        {
            // Re-show the player sprite now that the dummy trick is done
            player.Visible = true;

            if (kirbyDummy != null)
            {
                kirbyDummy.RemoveSelf();
                kirbyDummy = null;
            }

            // Freefall Ã¢â‚¬â€ gravity pulls Kirby down
            Vector2 startPos   = player.Position;
            Vector2 groundPos  = startPos + new Vector2(0f, 80f);
            float fallDuration = 0.9f;

            for (float t = 0f; t < fallDuration; t += Engine.DeltaTime)
            {
                // Accelerating fall with gravity feel
                float progress = Ease.CubeIn(t / fallDuration);
                player.Position = Vector2.Lerp(startPos, groundPos, progress);
                yield return null;
            }

            player.Position = groundPos;

            // Thud
            level.Shake(0.3f);
            Audio.Play("event:/DZ/char/kirby/landing", player.Position);

            // Brief beat of silence Ã¢â‚¬â€ scene ends
            yield return 1.5f;
        }

        #endregion
    }
}

