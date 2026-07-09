using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 10 - Flowey Intro Cutscene
    /// Flowey appears and threatens Madeline, then Kirby and friends arrive to save her.
    /// Supports 3 variants: Normal, Returning (remembers progress), and Assist mode.
    /// </summary>
    [CustomEntity("DesoloZantas/FloweyIntroScene")]
    [HotReloadable]
    public class FloweyIntroScene : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch10_flowey_intro_complete";

        private const string DIALOG_NORMAL = "DZ_CH10_FLOWEY_INTRO";
        private const string DIALOG_RETURNING = "DZ_CH10_FLOWEY_INTRO_RETURNING";
        private const string DIALOG_ASSIST = "DZ_CH10_FLOWEY_INTRO_ASSIST";

        // Audio
        private const string MUSIC_FLOWEY = "event:/DZ/music/lvl10/flowey";
        private const string MUSIC_FLOWEY_ALT = "event:/DZ/music/lvl10/flowey_alt";
        private const string SFX_FLOWEY_EMERGE = "event:/DZ/sfx/lvl10/flowey_emerge";
        private const string SFX_MUSIC_DROP = "event:/DZ/sfx/lvl10/music_drop";
        private const string SFX_SEED_CIRCLE = "event:/DZ/sfx/lvl10/seed_circle";
        private const string SFX_FLOWEY_LAUGH = "event:/DZ/sfx/lvl10/flowey_laugh";
        private const string SFX_STAR_BULLET = "event:/DZ/sfx/lvl10/star_bullet_hit";

        // Default spawn offset from player (right side of screen)
        private const float FLOWEY_SPAWN_OFFSET_X = 120f;
        #endregion

        #region Fields
        private readonly global::Celeste.Player player;
        private FloweyNPC flowey;
        private string dialogKey;
        private bool isAlternative;
        private bool spawnedFlowey;
        #endregion

        /// <summary>
        /// Constructor for programmatic creation (e.g. from EventTrigger).
        /// </summary>
        public FloweyIntroScene(global::Celeste.Player player)
            : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
        }

        /// <summary>
        /// Constructor for map/Loenn placement via EntityData.
        /// </summary>
        public FloweyIntroScene(EntityData data, Vector2 offset)
            : base(true, false)
        {
            Position = data.Position + offset;
            // Player will be resolved in OnBegin
            player = null;
        }

        public override void OnBegin(Level level)
        {
            if (level.Session.GetFlag(FLAG_CUTSCENE_COMPLETE))
            {
                WasSkipped = true;
                EndCutscene(level);
                return;
            }

            // Resolve player if not set (EntityData constructor path)
            var activePlayer = player ?? level.Tracker.GetEntity<global::Celeste.Player>();
            if (activePlayer == null)
            {
                EndCutscene(level);
                return;
            }

            // Find or spawn FloweyNPC in the scene
            flowey = level.Tracker.GetEntity<FloweyNPC>();
            if (flowey == null)
            {
                // Spawn Flowey to the right of the player, hidden, ready to emerge
                Vector2 floweyPos = new Vector2(activePlayer.X + FLOWEY_SPAWN_OFFSET_X, activePlayer.Y);
                flowey = new FloweyNPC(floweyPos, startHidden: true);
                level.Add(flowey);
                spawnedFlowey = true;
            }

            // Determine which variant to play
            dialogKey = GetDialogueKey();
            isAlternative = dialogKey != DIALOG_NORMAL;

            // Lock player movement
            activePlayer.StateMachine.State = global::Celeste.Player.StDummy;
            activePlayer.StateMachine.Locked = true;

            Add(new Coroutine(CutsceneSequence(level, activePlayer)));
        }

        /// <summary>
        /// Main cutscene coroutine - plays the full Flowey intro sequence
        /// </summary>
        private IEnumerator CutsceneSequence(Level level, global::Celeste.Player activePlayer)
        {
            // Play the dialog with all 9 trigger callbacks (indices 0-8)
            yield return Textbox.Say(dialogKey,
                Trigger0_FloweyEmergesAndStartMusic,   // {trigger 0}
                Trigger1_MadelineStepForwardZoomIn,    // {trigger 1}
                Trigger2_FloweyCaughtMadelineMusicDrop,// {trigger 2}
                Trigger3_CircleMadelineWithSeed,       // {trigger 3}
                Trigger4_FloweyLaugh,                  // {trigger 4}
                Trigger5_StarBulletHitFlowey,          // {trigger 5}
                Trigger6_KirbyWalkIn,                  // {trigger 6}
                Trigger7_TheoWalkIn,                   // {trigger 7}
                Trigger8_EveryonePosed                  // {trigger 8}
            );

            // Set completion flag
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            EndCutscene(level);
        }

        #region Trigger 0 - Flowey Emerges and Start Music

        /// <summary>
        /// Flowey emerges from the ground and the music starts playing.
        /// Normal variant plays standard Flowey theme, alt variant plays an alternative version.
        /// </summary>
        private IEnumerator Trigger0_FloweyEmergesAndStartMusic()
        {
            Level level = Scene as Level;

            // Flowey emerges from the ground
            if (flowey != null)
            {
                flowey.FacePlayer();
                yield return flowey.Emerge();
            }
            else
            {
                // Fallback if no Flowey NPC
                Audio.Play(SFX_FLOWEY_EMERGE);
                if (level != null) level.Shake(0.2f);
            }

            yield return 0.3f;

            // Start music - alt version for returning players
            string musicTrack = isAlternative ? MUSIC_FLOWEY_ALT : MUSIC_FLOWEY;
            Audio.SetMusic(musicTrack);

            yield return 0.5f;
        }

        #endregion

        #region Trigger 1 - Madeline Step Forward and Ominous Zoom In

        /// <summary>
        /// Madeline steps forward toward Flowey. Camera zooms in ominously.
        /// </summary>
        private IEnumerator Trigger1_MadelineStepForwardZoomIn()
        {
            Level level = Scene as Level;

            // Madeline walks forward
            if (player != null)
            {
                player.Facing = Facings.Right;
                yield return player.DummyWalkTo(player.X + 24f);
            }

            yield return 0.2f;

            // Ominous zoom in
            if (level != null)
            {
                float targetZoom = 1.5f;
                for (float t = 0f; t < 1f; t += Engine.DeltaTime * 0.8f)
                {
                    level.ZoomSnap(new Vector2(160f, 90f), MathHelper.Lerp(1f, targetZoom, Ease.CubeIn(t)));
                    yield return null;
                }
            }

            yield return 0.3f;
        }

        #endregion

        #region Trigger 2 - Flowey Caught Madeline and Music Dropped

        /// <summary>
        /// Flowey traps Madeline. The music drops out dramatically.
        /// </summary>
        private IEnumerator Trigger2_FloweyCaughtMadelineMusicDrop()
        {
            Level level = Scene as Level;

            // Flowey shows evil expression
            flowey?.PlayExpression("evil");

            // Music drops
            Audio.Play(SFX_MUSIC_DROP);
            Audio.SetMusic(null);

            // Screen shake for impact
            if (level != null)
            {
                level.Shake(0.4f);
                level.Flash(Color.White * 0.3f, true);
            }

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            yield return 0.8f;
        }

        #endregion

        #region Trigger 3 - Circle Madeline with Seed and Heal

        /// <summary>
        /// Flowey's seeds/bullets circle around Madeline in a threatening pattern.
        /// </summary>
        private IEnumerator Trigger3_CircleMadelineWithSeed()
        {
            // Flowey grins while circling seeds
            flowey?.PlayExpression("grin");

            // Play seed circling sound
            Audio.Play(SFX_SEED_CIRCLE);

            // Screen shake to sell the danger
            Level level = Scene as Level;
            if (level != null)
            {
                level.Shake(0.3f);
            }

            Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);

            yield return 1.5f;
        }

        #endregion

        #region Trigger 4 - Flowey Laugh

        /// <summary>
        /// Flowey laughs maniacally. Screen distorts slightly.
        /// </summary>
        private IEnumerator Trigger4_FloweyLaugh()
        {
            Level level = Scene as Level;

            // Flowey laugh animation + sound
            if (flowey != null)
            {
                yield return flowey.Laugh(1.0f);
            }
            else
            {
                Audio.Play(SFX_FLOWEY_LAUGH);
                if (level != null) level.Shake(0.2f);
                yield return 1.0f;
            }
        }

        #endregion

        #region Trigger 5 - Star Bullet Hit Flowey

        /// <summary>
        /// Kirby's star bullet hits Flowey and knocks him to the ground.
        /// </summary>
        private IEnumerator Trigger5_StarBulletHitFlowey()
        {
            Level level = Scene as Level;

            // Star bullet impact
            Audio.Play(SFX_STAR_BULLET);

            if (level != null)
            {
                level.Shake(0.5f);
                level.Flash(Color.Yellow * 0.4f, true);
            }

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);

            // Flowey gets knocked back
            if (flowey != null)
            {
                yield return flowey.GetHit(new Vector2(1f, 0f), 24f);
            }
            else
            {
                yield return 0.3f;
            }

            // Zoom back to normal after the hit
            if (level != null)
            {
                for (float t = 0f; t < 1f; t += Engine.DeltaTime * 1.5f)
                {
                    level.ZoomSnap(new Vector2(160f, 90f), MathHelper.Lerp(1.5f, 1f, Ease.CubeOut(t)));
                    yield return null;
                }
                level.ZoomSnap(new Vector2(160f, 90f), 1f);
            }

            yield return 0.5f;
        }

        #endregion

        #region Trigger 6 - Kirby Walk In

        /// <summary>
        /// Kirby walks in from the right side of the screen.
        /// </summary>
        private IEnumerator Trigger6_KirbyWalkIn()
        {
            // Kirby walks in from the right
            // The NPC entity handles its own walk animation via the dialog system
            yield return 0.5f;
        }

        #endregion

        #region Trigger 7 - Theo Walk In

        /// <summary>
        /// Theo walks in from the right side of the screen.
        /// </summary>
        private IEnumerator Trigger7_TheoWalkIn()
        {
            // Theo walks in from the right
            yield return 0.5f;
        }

        #endregion

        #region Trigger 8 - Everyone Posed

        /// <summary>
        /// All characters strike a determined pose together before moving on.
        /// </summary>
        private IEnumerator Trigger8_EveryonePosed()
        {
            Level level = Scene as Level;

            // Flowey shows pissed expression as heroes arrive
            flowey?.PlayExpression("pissed");

            // Brief pause for dramatic effect
            yield return 0.3f;

            // Resume music for the team moment
            Audio.SetMusic(MUSIC_FLOWEY);

            // Slight zoom for the group shot
            if (level != null)
            {
                for (float t = 0f; t < 1f; t += Engine.DeltaTime)
                {
                    level.ZoomSnap(new Vector2(160f, 90f), MathHelper.Lerp(1f, 1.2f, Ease.CubeOut(t)));
                    yield return null;
                }
            }

            yield return 0.5f;

            // Zoom back to normal
            if (level != null)
            {
                for (float t = 0f; t < 1f; t += Engine.DeltaTime * 1.5f)
                {
                    level.ZoomSnap(new Vector2(160f, 90f), MathHelper.Lerp(1.2f, 1f, Ease.CubeOut(t)));
                    yield return null;
                }
                level.ZoomSnap(new Vector2(160f, 90f), 1f);
            }
        }

        #endregion

        #region Dialog Key Selection

        /// <summary>
        /// Determines which dialogue variant to use based on player progress and settings.
        /// - Assist mode: player is using accessibility assists
        /// - Returning: player has been to higher chapters (remembers progress)
        /// - Normal: first time through
        /// </summary>
        private string GetDialogueKey()
        {
            if (IsUsingAssistMode())
                return DIALOG_ASSIST;

            if (HasBeenToHigherChapters())
                return DIALOG_RETURNING;

            return DIALOG_NORMAL;
        }

        private bool IsUsingAssistMode()
        {
            var saveData = SaveData.Instance;
            if (saveData?.Assists == null) return false;
            return saveData.Assists.DashAssist || saveData.Assists.InfiniteStamina || saveData.Assists.Invincible;
        }

        private bool HasBeenToHigherChapters()
        {
            try
            {
                var saveData = SaveData.Instance;
                return saveData?.TotalDeaths > 200 || saveData?.TotalStrawberries > 50;
            }
            catch (Exception ex)
            {
                IngesteLogger.Error($"FloweyIntroScene: Error checking chapter progress: {ex.Message}");
                return false;
            }
        }

        #endregion

        public override void OnEnd(Level level)
        {
            // Resolve player
            var activePlayer = player ?? level?.Tracker.GetEntity<global::Celeste.Player>();

            // Restore player control
            if (activePlayer != null)
            {
                activePlayer.StateMachine.State = global::Celeste.Player.StNormal;
                activePlayer.StateMachine.Locked = false;
            }

            // Reset Flowey to idle
            flowey?.PlayExpression("idle");

            // Clean up spawned Flowey if we created it
            if (spawnedFlowey && flowey != null)
            {
                flowey.RemoveSelf();
            }

            // Reset zoom
            level?.ZoomSnap(new Vector2(160f, 90f), 1f);
        }
    }
}

