namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class Cs07MaddyMirror(global::Celeste.Player player) : CutsceneEntity()
    {
        public const string FLAG = "maddytrapInMirror";
        private readonly global::Celeste.Player player = player;

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene()));
        }

        public override void OnEnd(Level level)
        {
            // Required implementation
        }

        private IEnumerator Cutscene()
        {
            // Mirror sequence without NPCs
            yield return 0.4f;

            // Handle player state
            if (player != null)
            {
                player.StateMachine.State = Player.StDummy;
                player.StateMachine.Locked = true;
            }

            // Show mirror dialog
            yield return Textbox.Say("CH7_MADELINE_MIRROR");

            // Set the flag
            Level.Session.SetFlag(FLAG);

            // Complete the cutscene
            EndCutscene(Level);
        }
    }
}




