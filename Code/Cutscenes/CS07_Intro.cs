namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class Cs07Intro(global::Celeste.Player player) : CutsceneEntity
    {
        public const string FLAG = "ch7_intro";
        private readonly global::Celeste.Player player = player;
        private BadelineDummy badeline;
        private Vector2 playerEndPosition;

        public override void OnBegin(Level level)
        {
            this.playerEndPosition = player.Position;
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            Cs07Intro cs07Intro = this;
            
            // Lock player movement
            cs07Intro.player.StateMachine.State = Player.StDummy;
            cs07Intro.player.StateMachine.Locked = true;
            cs07Intro.player.ForceCameraUpdate = true;

            // Create Badeline on the left side
            cs07Intro.badeline = new BadelineDummy(cs07Intro.player.Position + new Vector2(-48f, 0f));
            cs07Intro.badeline.Appear(level);
            level.Add(cs07Intro.badeline);

            yield return 0.5f;

            cs07Intro.badeline.Sprite.Scale.X = -1f; // Face left

            yield return 0.3f;

            // Initialize the dialog
            yield return Textbox.Say("MAGGYHELPER_CH7_INTRO_DIALOG",
                new Func<IEnumerator>(cs07Intro.BadelineTurnsRight),
                new Func<IEnumerator>(cs07Intro.BadelineVanishes));

            yield return 0.5f;
            
            cs07Intro.EndCutscene(level);
        }

        private IEnumerator BadelineTurnsRight()
        {
            // Step 2: Make Badeline turn right during dialog
            yield return 0.2f;
            badeline.Sprite.Scale.X = 1f; // Turn right
            yield return 0.3f;
        }

        private IEnumerator BadelineVanishes()
        {
            // Step 2: Make Badeline vanish before ending Ralsei dialog
            yield return 0.1f;
            badeline.Vanish();
            yield return 0.5f;
        }

        public override void OnEnd(Level level)
        {
            // Restore player control
            this.player.Position = this.playerEndPosition;
            while (!this.player.OnGround())
                this.player.MoveV(1f);
            level.Camera.Position = this.player.CameraTarget;
            level.Session.SetFlag(FLAG);
            
            // Clean up any remaining entities
            badeline?.RemoveSelf();
        }
    }
}



