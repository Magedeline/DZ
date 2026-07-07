using Celeste.Entities;

namespace Celeste.Cutscenes
{
    public class CS19_SoulsDiscarded : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS19_SoulsDiscarded(global::Celeste.Player player) : base()
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
            player.Dashes = 1;
            yield return 0.5f;

            yield return level.ZoomTo(
                (player.Position + new Vector2(0f, -16f)) - level.Camera.Position,
                2f, 0.5f);

            yield return Textbox.Say("DZ_CH19_SOULS_DISCARDED",
                new Func<IEnumerator>(KirbyKneelsAtGravestone),
                new Func<IEnumerator>(KirbyStandsFacesVoid)
            );

            yield return level.ZoomBack(0.5f);
            EndCutscene(level);
        }

        private IEnumerator KirbyKneelsAtGravestone()
        {
            player.DummyAutoAnimate = false;
            player.Facing = Facings.Left;
            yield return player.DummyWalkTo(player.X - 8f, false, 0.5f, false);
            yield return 0.4f;
        }

        private IEnumerator KirbyStandsFacesVoid()
        {
            player.DummyAutoAnimate = true;
            player.Facing = Facings.Right;
            yield return 0.3f;
            Audio.Play("event:/new_content/game/10_farewell/glitch_short", player.Position);
            yield return 0.5f;
        }

        public override void OnEnd(Level level)
        {
            player.Depth = 0;
            player.Speed = Vector2.Zero;
            player.StateMachine.State = Player.StNormal;
            level.ResetZoom();
            level.Session.SetFlag("souls_discarded");
        }
    }
}

