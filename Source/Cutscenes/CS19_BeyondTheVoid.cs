using Celeste.Entities;

namespace Celeste.Cutscenes
{
    public class CS19_BeyondTheVoid : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS19_BeyondTheVoid(global::Celeste.Player player) : base()
        {
            Depth = -8500;
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            player.StateMachine.State = Player.StDummy;
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            yield return 0.5f;
            yield return Textbox.Say("DZ_CH19_BEYOND_THE_VOID");
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            player.Depth = 0;
            player.Speed = Vector2.Zero;
            player.StateMachine.State = Player.StNormal;
            level.Session.SetFlag("beyond_the_void");
        }
    }
}
