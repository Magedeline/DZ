#nullable enable

namespace Celeste.Cutscenes;
// Note: Removed [CustomEntity] - cutscene entities are created programmatically, not placed in maps
    public class Cs05Guestbook : CutsceneEntity
    {
        private global::Celeste.Player player;

    public Cs05Guestbook(bool v, bool v1) : base(v, v1) {
        player = Scene.Tracker.GetEntity<global::Celeste.Player>();
    }

    public Cs05Guestbook(global::Celeste.Player player1) {
        player = player1;
    }

    public override void OnBegin(Level level) => Add(new Coroutine(routine()));

        private IEnumerator routine()
        {
            Cs05Guestbook cs03Guestbook = this;
            cs03Guestbook.player.StateMachine.State = 11;
            cs03Guestbook.player.StateMachine.Locked = true;
            yield return Textbox.Say("ch5_guestbook");
            yield return 0.1f;
            cs03Guestbook.endCutscene(cs03Guestbook.Level);
        }

    private void endCutscene(Level level)
    {
        level.EndCutscene();
    }

    public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }
    }




