using Celeste.Entities;

namespace Celeste.Cutscenes {
    [HotReloadable]
    public class Cs07SaveMaddy(global::Celeste.Player player) : CutsceneEntity {
        public const string FLAG = "FoundMaddy";
        private const float walk_offset = 18f;
        private const float end_position_offset = -24f;
        private const float zoom_duration = 0.5f;
        
        private readonly global::Celeste.Player player = player ?? throw new ArgumentNullException(nameof(player));
        private Entity maddyCrystal;
        private Vector2 playerEndPosition;
        private bool wasDashAssistOn;
        private TheoCrystalPedestal crystalPedestal;

        public override void OnBegin(Level level) {
            InitializeEntities(level);
            StoreDashAssistState();
            Add(new Coroutine(Cutscene(level)));
        }

        private void InitializeEntities(Level level) {
            // Find the MaddyCrystal entity in the level
            maddyCrystal = level.FirstOrDefault<Entity>(entity => {
                return isMaddyCrystal(entity);

                static bool isMaddyCrystal(object o) {
                    if (o is Entity entity) return entity.GetType().Name == nameof(MaddyCrystal);

                    return false;
                }
            });
            // Find the TheoCrystalPedestal entity in the scene
            crystalPedestal = Scene.Entities.FindFirst<TheoCrystalPedestal>();
            // Set the player's end position relative to the MaddyCrystal's position
            if (maddyCrystal != null) {
                playerEndPosition = PlayerEndPosition1(maddyCrystal.Position);
            }
        }

        private static Vector2 PlayerEndPosition1(Vector2 maddyCrystalPosition) {
            return new Vector2(maddyCrystalPosition.X + end_position_offset, maddyCrystalPosition.Y);
        }

        private void StoreDashAssistState() {
            wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
        }

        private IEnumerator Cutscene(Level level) {
            yield return SetupCutscene(level);
            yield return WalkToMaddy();
            yield return ShowDialogue();
            yield return FinalizeCutscene(level);
        }

        private IEnumerator SetupCutscene(Level level) {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            player.ForceCameraUpdate = true;
            
            level.Session.Audio.Music.Layer(6, 0f);
            level.Session.Audio.Apply();
            
            yield break;
        }

        private IEnumerator WalkToMaddy() {
            if (maddyCrystal == null) yield break;
            
            yield return player.DummyWalkTo(maddyCrystal.X - walk_offset);
            player.Facing = Facings.Right;
        }

        private IEnumerator ShowDialogue() {
            yield return Textbox.Say("ch7_found_maddy", TryToBreakCrystal);
            yield return 0.25f;
        }

        private IEnumerator FinalizeCutscene(Level level) {
            yield return Level.ZoomBack(zoom_duration);
            EndCutscene(level);
        }

        private IEnumerator TryToBreakCrystal() {
            yield return PrepareForCrystalBreak();
            yield return ExecuteDash();
            yield return CompleteBreakSequence();
        }

        private IEnumerator PrepareForCrystalBreak() {
            if (crystalPedestal != null) {
                crystalPedestal.Collidable = true;
            }
            
            if (maddyCrystal == null) yield break;
            
            yield return player.DummyWalkTo(maddyCrystal.X);
            yield return 0.1f;
            yield return Level.ZoomTo(new Vector2(160f, 90f), 2f, zoom_duration);
            
            player.DummyAutoAnimate = false;
            player.Sprite.Play("lookUp");
            yield return 1f;
        }

        private IEnumerator ExecuteDash() {
            SetupDashState();
            
            yield return 0.1f;
            
            while (!player.OnGround() || player.Speed.Y < 0f) {
                player.Dashes = 0;
                Input.MoveY.Value = -1;
                Input.MoveX.Value = 0;
                yield return null;
            }
            
            RestorePlayerControl();
        }

        private void SetupDashState() {
            SaveData.Instance.Assists.DashAssist = false;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            MInput.Disabled = true;
            
            player.OverrideDashDirection = new Vector2(0f, -1f);
            player.StateMachine.Locked = false;
            player.StateMachine.State = player.StartDash();
            player.Dashes = 0;
        }

        private void RestorePlayerControl() {
            player.OverrideDashDirection = null;
            Input.MoveX.Value = 0;
            Input.MoveY.Value = 0;
            player.Speed = Vector2.Zero;
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            MInput.Disabled = false;
            player.DummyAutoAnimate = true;
        }

        private IEnumerator CompleteBreakSequence() {
            yield return player.DummyWalkToExact((int)playerEndPosition.X, true);
            yield return 1.5f;
        }

        public override void OnEnd(Level level) {
            // Ensure input is reset even if cutscene ends early
            if (player != null) {
                Input.MoveX.Value = 0;
                Input.MoveY.Value = 0;
                player.Speed = Vector2.Zero;
                player.OverrideDashDirection = null;
            }
            
            RestoreGameState(level);
            PositionPlayer();
            UpdateCameraAndAudio(level);
            HandleFollowers();
            UpdateCrystalState();
            HandleMaddyCrystal();
        }

        private void RestoreGameState(Level level) {
            SaveData.Instance.Assists.DashAssist = wasDashAssistOn;
            level.Session.SetFlag("foundMaddyInCrystal");
            level.ResetZoom();
        }

        private void PositionPlayer() {
            player.Position = playerEndPosition;
            while (!player.OnGround()) {
                player.MoveV(1f);
            }
        }

        private void UpdateCameraAndAudio(Level level) {
            level.Camera.Position = player.CameraTarget;
            level.Session.Audio.Music.Layer(6, 1f);
            level.Session.Audio.Apply();
        }

        private void HandleFollowers() {
            var followerList = new List<Follower>(player.Leader.Followers);
            player.RemoveSelf();
            
            var newPlayer = new global::Celeste.Player(player.Position, player.DefaultSpriteMode);
            Scene.Add(newPlayer);
            
            foreach (var follower in followerList) {
                newPlayer.Leader.Followers.Add(follower);
                follower.Leader = newPlayer.Leader;
            }
            
            newPlayer.Facing = Facings.Right;
            newPlayer.IntroType = global::Celeste.Player.IntroTypes.None;
        }

        private void UpdateCrystalState() {
            if (crystalPedestal != null) {
                crystalPedestal.Collidable = false;
                crystalPedestal.DroppedTheo = true;
            }
        }

        private void HandleMaddyCrystal() {
            if (maddyCrystal == null) return;
            
            maddyCrystal.Depth = 100;
            
            if (maddyCrystal is Actor maddyActor) {
                maddyActor.Position = Vector2.Zero;
                while (!maddyActor.OnGround()) {
                    maddyActor.MoveV(1f);
                }
            }
        }
    }
}



