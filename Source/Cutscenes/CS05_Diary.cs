#nullable enable

namespace Celeste.Cutscenes;
// Note: Removed [CustomEntity] - cutscene entities are created programmatically, not placed in maps
    [HotReloadable]
    public class Cs05Diary(bool v, bool v1, global::Celeste.Player player) : CutsceneEntity(v, v1) {
        private global::Celeste.Player player = player;

        public Cs05Diary(global::Celeste.Player player) : this(true, false, player) {
    }

    public override void OnBegin(Level level) => Add(new Coroutine(routine()));

        private IEnumerator routine()
        {
            Cs05Diary cs03Diary = this;
            cs03Diary.player.StateMachine.State = 11;
            cs03Diary.player.StateMachine.Locked = true;
            yield return Textbox.Say("CH5_DIARY");
            yield return 0.1f;
            cs03Diary.endCutscene(cs03Diary.Level);
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




