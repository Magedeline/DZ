using System;
using System.Collections;
using Celeste.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ
{
    /// <summary>
    /// Cutscene: Asriel's True Reveal - "Finally. I was so tired of being a flower."
    /// Adapted from Undertale's obj_asrielappear sequence for Celeste 2D platformer
    /// </summary>
    [Tracked(true)]
    public class CS20_AsrielRevealIdentity : CutsceneEntity
    {
        #region Constants

        private const string DIALOGUE_FINALLY = "DZ_CH20_ASRIEL_FINALLY";
        private const string DIALOGUE_HOWDY = "DZ_CH20_ASRIEL_HOWDY";
        private const string FLAG_REVEAL_COMPLETE = "asriel_true_reveal_done";

        private const string SFX_CREATE = "event:/new_content/char/pusheen/asriel/Asriel_Create";
        private const string SFX_GLITCH = "event:/new_content/game/pusheen/19_spaces/glitch_long";
        private const string SFX_REVEAL = "event:/new_content/game/general/dramatic_reveal";
        private const string MUSIC_REVEAL = "event:/new_content/music/pusheen/lvl20/asriel_reveal";

        // Animation timing (in seconds, converted from frames at 60fps)
        private const float FRAME_30 = 0.5f;
        private const float FRAME_60 = 1.0f;
        private const float FRAME_90 = 1.5f;
        private const float FRAME_120 = 2.0f;
        private const float FRAME_180 = 3.0f;
        private const float FRAME_190 = 3.17f;
        private const float FRAME_200 = 3.33f;
        private const float FRAME_210 = 3.5f;
        private const float FRAME_250 = 4.17f;

        #endregion

        #region Fields

        private Player player;
        private AsrielGodBoss asrielBoss;
        private Level level;
        private Sprite asrielSprite;
        private float counter;
        private float con;
        private bool flasher;
        private float ss; // sine wave offset for floating
        private EventInstance createMusic;
        private Vector2 originalPosition;
        private Vector2 floatOffset;

        #endregion

        #region Constructor

        public CS20_AsrielRevealIdentity(Player player, AsrielGodBoss asrielBoss)
        {
            this.player = player;
            this.asrielBoss = asrielBoss;
        }

        public static IEnumerator StartCutscene(AsrielGodBoss boss)
        {
            Level level = boss.SceneAs<Level>();
            Player player = level.Tracker.GetEntity<Player>();

            if (player == null)
                yield break;

            CS20_AsrielRevealIdentity cutscene = new CS20_AsrielRevealIdentity(player, boss);
            level.Add(cutscene);

            yield return null;
        }

        #endregion

        #region Cutscene Sequence

        public override void OnBegin(Level level)
        {
            this.level = level;
            originalPosition = asrielBoss.Position;
            asrielSprite = asrielBoss.NormalSprite;

            // Initialize state
            con = 1f;
            counter = 0f;
            ss = 0f;

            // Start with idle animation
            if (asrielSprite != null && asrielSprite.Has("idle"))
                asrielSprite.Play("idle");

            Add(new Coroutine(CutsceneSequence()));
        }

        private IEnumerator CutsceneSequence()
        {
            // Disable player control
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate = false;
            player.Facing = Facings.Right;

            // Phase 1: Flower form animation sequence (con = 1)
            yield return Phase1_FlowerFormSequence();

            // Phase 1.1: "Finally..." dialogue
            yield return Phase1_FinallyDialogue();

            // Phase 2.2: Transform to Kid Asriel
            yield return Phase2_TransformToKid();

            // Phase 4: Kid Asriel animation
            yield return Phase3_KidAnimation();

            // Phase 5-6: "Howdy!" dialogue
            yield return Phase4_HowdyDialogue();

            // Phase 8-11: Music, flash, and transform to God Form
            yield return Phase5_GodFormReveal();

            // Phase 12: Final Writer effect (text reveal)
            yield return Phase6_NameReveal();

            // Phase 14: Complete
            yield return Phase7_Complete();

            EndCutscene(level);
        }

        #endregion

        #region Phase Methods

        /// <summary>
        /// Phase 1 (con = 1): Transformation sequence using 'back' animation frames
        /// Maps Undertale image_index sequence to Celeste sprite animations
        /// </summary>
        private IEnumerator Phase1_FlowerFormSequence()
        {
            counter = 0f;

            // Play 'back' animation which has frames 00-21 (matches Undertale's 0-7 + flicker sequence)
            if (asrielSprite != null && asrielSprite.Has("back"))
                asrielSprite.Play("back", restart: true);

            float[] frameTimes = new float[]
            {
                FRAME_30,   // back01
                FRAME_60,   // back02
                FRAME_90,   // back03
                FRAME_120,  // back02 (return)
                FRAME_180,  // back04
                FRAME_190,  // back05
                FRAME_200,  // back06
                FRAME_210,  // back07
            };

            int[] backFrameIndices = new int[] { 1, 2, 3, 2, 4, 5, 6, 7 };

            int currentFrame = 0;
            float elapsed = 0f;

            while (currentFrame < frameTimes.Length)
            {
                elapsed += Engine.DeltaTime;

                if (elapsed >= frameTimes[currentFrame])
                {
                    // Set specific frame within back animation
                    if (asrielSprite != null && asrielSprite.Has("back"))
                        asrielSprite.SetAnimationFrame(backFrameIndices[currentFrame]);
                    currentFrame++;
                }

                yield return null;
            }

            // Rapid flicker sequence using back08/back09 alternating
            float flickerStart = FRAME_250;
            float flickerInterval = 0.13f; // ~8 frames
            int[] flickerFrames = new int[] { 9, 8, 9, 8, 9, 8, 9, 8 };

            for (int i = 0; i < flickerFrames.Length; i++)
            {
                float targetTime = flickerStart + (flickerInterval * i);

                while (elapsed < targetTime)
                {
                    elapsed += Engine.DeltaTime;
                    yield return null;
                }

                // Flicker between back08 and back09
                if (asrielSprite != null && asrielSprite.Has("back"))
                    asrielSprite.SetAnimationFrame(flickerFrames[i]);

                // Screen shake on each flicker
                level.Shake(0.15f);
            }

            yield return 1.2f; // Wait until frame 380 equivalent
        }

        /// <summary>
        /// Phase 1.1: "Finally..." dialogue
        /// </summary>
        private IEnumerator Phase1_FinallyDialogue()
        {
            con = 1.1f;

            // Play glitch sound
            Audio.Play(SFX_GLITCH, asrielBoss.Position);

            yield return Textbox.Say(DIALOGUE_FINALLY);

            // Wait for textbox to close
            while (level.Tracker.GetEntity<Textbox>() != null)
                yield return null;

            con = 1.2f;
            yield return 0.83f; // 50 frames
        }

        /// <summary>
        /// Phase 2.2: Transform to smaller/kid form using 'idle' animation
        /// </summary>
        private IEnumerator Phase2_TransformToKid()
        {
            con = 2.2f;

            // Change to idle animation (kid form)
            if (asrielSprite != null && asrielSprite.Has("back"))
                asrielSprite.Play("back", restart: true);

            // Slight position adjustment like Undertale (x += 2, y -= 2)
            asrielBoss.Position = originalPosition + new Vector2(2f, -2f);

            yield return null;

            while (level.Tracker.GetEntity<Textbox>() != null)
                yield return null;

            con = 3f;
            yield return 0.67f; // 40 frames
        }

        /// <summary>
        /// Phase 4: Kid Asriel animation sequence using idle frames
        /// </summary>
        private IEnumerator Phase3_KidAnimation()
        {
            con = 4f;
            counter = 0f;

            // Idle animation frames 1-3 for subtle movement
            float[] kidFrameTimes = new float[] { 0.25f, 0.5f, 0.75f }; // 15, 30, 45 frames
            int[] kidFrameIndices = new int[] { 1, 2, 3 };

            int currentFrame = 0;
            float elapsed = 0f;

            while (currentFrame < kidFrameTimes.Length)
            {
                elapsed += Engine.DeltaTime;

                if (elapsed >= kidFrameTimes[currentFrame])
                {
                    if (asrielSprite != null && asrielSprite.Has("back"))
                        asrielSprite.SetAnimationFrame(kidFrameIndices[currentFrame]);
                    currentFrame++;
                }

                yield return null;
            }

            // Wait until 90 frames
            while (elapsed < 1.5f)
            {
                elapsed += Engine.DeltaTime;
                yield return null;
            }

            con = 5f;
        }

        /// <summary>
        /// Phase 5-6: "Howdy!" dialogue
        /// </summary>
        private IEnumerator Phase4_HowdyDialogue()
        {
            yield return Textbox.Say(DIALOGUE_HOWDY);

            con = 6f;

            while (level.Tracker.GetEntity<Textbox>() != null)
                yield return null;

            con = 7f;
            yield return 0.17f; // 10 frames
        }

        /// <summary>
        /// Phase 8-11: Music start, flash effect, transform to God Form using 'spin' or 'back'
        /// </summary>
        private IEnumerator Phase5_GodFormReveal()
        {
            con = 8f;

            // Load and play "create" music
            createMusic = Audio.Play(MUSIC_REVEAL);
            flasher = true;

            con = 9f;
            yield return 0.08f; // 5 frames

            con = 10f;

            // Transform to god form - use 'spin' animation for dramatic effect, then settle on 'back'
            if (asrielSprite != null && asrielSprite.Has("idle"))
                asrielSprite.Play("idle", restart: true);

            // Center Asriel like Undertale (screen center)
            Vector2 centerPos = new Vector2(
                level.Camera.Position.X + 160f - (asrielBoss.Width / 2f),
                20f
            );

            // Smooth transition to center
            Vector2 startPos = asrielBoss.Position;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
            {
                asrielBoss.Position = Vector2.Lerp(startPos, centerPos, Ease.CubeOut(t));
                yield return null;
            }

            asrielBoss.Position = centerPos;
            ss = 0f;

            // Settle on 'back' animation for god form facing away/floating
            if (asrielSprite != null && asrielSprite.Has("boss"))
                asrielSprite.Play("boss");

            // Enable floating sine wave movement
            Add(new Coroutine(FloatingAnimation()));

            con = 11f;
            yield return 0.5f; // 30 frames
        }

        /// <summary>
        /// Phase 12: Name reveal with FinalWriter effect
        /// </summary>
        private IEnumerator Phase6_NameReveal()
        {
            con = 12f;

            // Flash effect
            level.Flash(Color.White, drawPlayerOver: false);
            level.Shake(0.5f);

            // Create dramatic name reveal
            yield return Textbox.Say("DZ_CH20_ASRIEL_NAME_FINAL");

            con = 13f;

            // Calculate wait time based on text length
            Textbox textbox = level.Tracker.GetEntity<Textbox>();
            if (textbox != null)
            {
                // Approximate time calculation
                yield return 1.5f;
            }

            yield return 1f; // Extra pause
        }

        /// <summary>
        /// Phase 14: Complete - start boss fight
        /// </summary>
        private IEnumerator Phase7_Complete()
        {
            con = 14f;

            // Stop floating
            RemoveSelfComponents();

            // Restore position
            asrielBoss.Position = originalPosition;

            // Set flag for completion
            level.Session.SetFlag(FLAG_REVEAL_COMPLETE);

            yield return 0.5f;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets a specific animation frame within the current animation
        /// </summary>
        private void SetAsrielAnimationFrame(int frameIndex)
        {
            if (asrielSprite != null)
                asrielSprite.SetAnimationFrame(frameIndex);
        }

        private IEnumerator FloatingAnimation()
        {
            while (con >= 10f && con < 14f)
            {
                ss += Engine.DeltaTime * 10f; // sin speed
                floatOffset = new Vector2(0f, (float)Math.Sin(ss / 6f) * 0.5f);
                asrielBoss.Position += floatOffset * Engine.DeltaTime * 60f;
                yield return null;
            }
        }

        private void RemoveSelfComponents()
        {
            foreach (Component component in this)
            {
                if (component is Coroutine coroutine && coroutine != null)
                    coroutine.RemoveSelf();
            }
        }

        #endregion

        #region Update & Render

        public override void Update()
        {
            base.Update();

            // Handle flasher effect
            if (flasher)
            {
                // Flash screen periodically
                if ((int)(Scene.TimeActive * 10f) % 2 == 0)
                {
                    level.Flash(Color.White * 0.3f, drawPlayerOver: false);
                }
            }
        }

        #endregion

        #region Cutscene Control

        public override void OnEnd(Level level)
        {
            flasher = false;

            // Stop music if still playing
            if (createMusic != null)
            {
                createMusic.stop(STOP_MODE.ALLOWFADEOUT);
                createMusic.release();
            }

            // Restore player control
            if (player != null && player.StateMachine.State == Player.StDummy)
            {
                player.StateMachine.State = Player.StNormal;
                player.DummyAutoAnimate = true;
            }

            // Start boss fight music
            level.Session.Audio.Music.Event = "event:/new_content/music/pusheen/lvl20/kirby_vs_asriel_fight_1";
            level.Session.Audio.Apply();

            // Signal to boss that reveal is complete
            if (asrielBoss != null)
            {
                asrielBoss.Position = originalPosition;
            }
        }

        #endregion
    }
}

