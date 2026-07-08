using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 12 Titanis boss intro cutscene.
    /// Plays DZ_CH12_TITAN_BOSS_INTRO and the pre-end sequence.
    /// </summary>
    [HotReloadable]
    public class CS12_TitanBossIntro : CutsceneEntity
    {
        public const string FLAG = "ch12_titan_boss_intro";
        private readonly global::Celeste.Player player;

        public CS12_TitanBossIntro(global::Celeste.Player player) : base(true, false)
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
            level.Shake(0.5f);
            yield return 0.5f;

            yield return Textbox.Say("DZ_CH12_TITAN_BOSS_INTRO");
            yield return Textbox.Say("DZ_CH12_TITAN_BOSSES_PRE_END");

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
