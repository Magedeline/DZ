using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 12 Titanis boss outro cutscene.
    /// Plays DZ_CH12_TITAN_BOSS_OUTRO and DZ_CH12_TITAN_GONE.
    /// </summary>
    [HotReloadable]
    public class CS12_TitanBossOutro : CutsceneEntity
    {
        public const string FLAG = "ch12_titan_boss_outro";
        private readonly global::Celeste.Player player;

        public CS12_TitanBossOutro(global::Celeste.Player player) : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            if (player?.StateMachine == null) yield break;

            player.StateMachine.State = Player.StDummy;
            level.Flash(Color.White, true);
            yield return 0.5f;

            yield return Textbox.Say("DZ_CH12_TITAN_BOSS_OUTRO");
            yield return Textbox.Say("DZ_CH12_TITAN_GONE");

            yield return 0.5f;
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
                player.StateMachine.State = Player.StNormal;

            level.Session.SetFlag(FLAG);
        }
    }
}
