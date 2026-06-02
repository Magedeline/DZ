using System.Runtime.CompilerServices;
using Celeste.Entities;

namespace Celeste.Cutscenes
{
    public class CS08_BossCenter : CutsceneEntity
    {
        public const string Flag = "chara_boss_center";

        private global::Celeste.Player player;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator Cutscene(Level level)
        {
            while (player == null)
            {
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                yield return null;
            }
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            while (!player.OnGround())
            {
                yield return null;
            }
            yield return player.DummyWalkToExact((int)player.X + 20);
            yield return level.ZoomTo(new Vector2(80f, 110f), 2f, 0.5f);
            yield return Textbox.Say("ch8_chara_boss_center");
            yield return 0.1f;
            yield return level.ZoomBack(0.4f);
            EndCutscene(level);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnEnd(Level level)
        {
            if (WasSkipped && player != null)
            {
                while (!player.OnGround() && player.Y < (float)level.Bounds.Bottom)
                {
                    player.Y++;
                }
            }
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal;
            }
            level.Entities.FindFirst<CharaBoss>()?.OnPlayer(null);
            level.Session.SetFlag("chara_boss_center");
        }
    }
}
