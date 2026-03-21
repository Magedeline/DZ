namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Chapter 0 cutscene featuring Theo
    /// Initial interaction cutscene with Theo character
    /// </summary>
    [HotReloadable]
    [Tracked]
    public class Cs00Theo : CutsceneEntity
    {
        #region Constants
        public const string FLAG = "theo_00_house";
        private const float player_walk_distance = 48f;
        private const float initial_walk_distance = 20f;
        private const float zoom_level = 2f;
        private const float camera_transition_time = 0.5f;
        #endregion

        #region Fields
        private Vector2 playerEndPosition = Vector2.Zero;
        private Coroutine cameraRoutine;
        private Vector2 initialFocusPoint = new Vector2(210f, 90f);
        private Vector2 distanceFocusPoint = new Vector2(210f, 100f);
        private readonly global::Celeste.Player player;
        #endregion

        #region Constructor
        public Cs00Theo(global::Celeste.Player player) : base(false, true)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
        }
        #endregion

        #region Overrides
        public override void Awake(Scene scene)
        {
            try
            {
                base.Awake(scene);

                if (player != null)
                {
                    playerEndPosition = player.Position + new Vector2(player_walk_distance, 0f);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper", $"Error in CS00_Theo.Awake: {ex}");
            }
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(cutscene(level)));
        }

        public override void OnEnd(Level level)
        {
            try
            {
                CutsceneManager.ResetPlayerState(player);
                restorePlayerState();
                resetCamera();
                // no base.OnEnd call because CutsceneEntity.OnEnd is abstract
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper", $"Error in CS00_Theo.OnEnd: {ex}");
            }
        }

        public override void Removed(Scene scene)
        {
            try
            {
                cleanupCameraRoutine();
                base.Removed(scene);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper", $"Error in CS00_Theo.Removed: {ex}");
                base.Removed(scene);
            }
        }
        #endregion

        #region Cutscene Sequence Methods
        private IEnumerator cutscene(Level level)
        {
            yield return initialSetup();
            yield return firstDialogue();
            yield return secondDialogue();
            yield return concludeScene(level);
        }

        private IEnumerator initialSetup()
        {
            SetupPlayer();
            yield return 0.3f;

            // Initial movement and greeting
            if (player != null)
            {
                yield return player.DummyWalkTo(player.X + initial_walk_distance);
                player.Facing = Facings.Right;
            }
            yield return 0.5f;
        }

        private IEnumerator firstDialogue()
        {
            // First dialogue with interactive functions
            yield return Textbox.Say("CH0_THEO_A",
                walkCloser,
                walkTogether,
                zoomToDistance,
                panToPlayer);
        }

        private IEnumerator secondDialogue()
        {
            // Second part of conversation
            yield return Textbox.Say("CH0_THEO_B", finalInteraction);
            yield return 0.3f;
        }

        private IEnumerator concludeScene(Level level)
        {
            // Set completion flag
            level.Session.SetFlag(FLAG);

            // End the cutscene
            EndCutscene(level);
            yield return null;
        }
        #endregion

        #region Textbox Interaction Methods
        private IEnumerator walkCloser()
        {
            yield return 0.25f;
            if (player != null)
            {
                yield return player.DummyWalkTo(player.X - initial_walk_distance);
                player.Facing = Facings.Right;
            }
            yield return 0.8f;
        }

        private IEnumerator walkTogether()
        {
            if (player != null)
            {
                yield return player.DummyWalkToExact((int)playerEndPosition.X);
                yield return 0.8f;
                player.Facing = Facings.Left;
            }
            yield return 0.4f;

            // Camera work
            if (Level != null)
            {
                yield return Level.ZoomTo(initialFocusPoint, zoom_level, camera_transition_time);
            }
            yield return 0.2f;
        }

        private IEnumerator zoomToDistance()
        {
            if (Level != null)
            {
                cameraRoutine = new Coroutine(Level.ZoomAcross(distanceFocusPoint, 4f, 3f));
                Add(cameraRoutine);
            }
            yield return 0.2f;
        }

        private IEnumerator panToPlayer()
        {
            // Wait for camera routine to finish
            while (cameraRoutine != null && cameraRoutine.Active)
            {
                yield return null;
            }

            yield return 0.2f;
            if (Level != null)
            {
                yield return Level.ZoomAcross(initialFocusPoint, zoom_level, camera_transition_time);
            }
            yield return 0.2f;
        }

        private IEnumerator finalInteraction()
        {
            yield return 0.25f;
            if (player != null)
            {
                player.Facing = Facings.Right;
            }
            yield return 0.5f;
        }
        #endregion

        #region Helper Methods
        private void SetupPlayer()
        {
            if (player == null) return;

            try
            {
                player.StateMachine.State = 11; // dummy state used across cutscenes
                player.StateMachine.Locked = true;
            }
            catch
            {
            }

            player.ForceCameraUpdate = true;
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
        }

        private void restorePlayerState()
        {
            if (player == null) return;

            player.StateMachine.Locked = false;
            player.StateMachine.State = 0; // StNormal
            player.ForceCameraUpdate = false;
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
            player.Speed = Vector2.Zero;
            player.Position = new Vector2(playerEndPosition.X, player.Position.Y);
            player.Facing = Facings.Left;
        }

        private void resetCamera()
        {
            if (Level != null)
            {
                Level.ResetZoom();
            }
        }

        private void cleanupCameraRoutine()
        {
            if (cameraRoutine != null && cameraRoutine.Active)
            {
                cameraRoutine.RemoveSelf();
                cameraRoutine = null;
            }
        }
        #endregion
    }
}




