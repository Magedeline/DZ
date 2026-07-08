using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 21 Els Termina (True Final Boss) intro and end cutscene.
    /// </summary>
    [HotReloadable]
    public class CS21_ElsTerminaBoss : CutsceneEntity
    {
        public const string INTRO_FLAG = "ch21_els_termina_intro";
        public const string END_FLAG = "ch21_els_termina_end";
        private readonly global::Celeste.Player player;
        private readonly bool playEnd;

        public CS21_ElsTerminaBoss(global::Celeste.Player player, bool playEnd = false) : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.playEnd = playEnd;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            if (player?.StateMachine == null) yield break;

            player.StateMachine.State = Player.StDummy;
            level.Flash(Color.DarkViolet * 0.5f, true);
            yield return 0.5f;

            if (playEnd)
            {
                yield return Textbox.Say("DZ_CH21_ELS_TERMINA_BOSS_END");
                level.Session.SetFlag(END_FLAG);
            }
            else
            {
                yield return Textbox.Say("DZ_CH21_ELS_TERMINA_BOSS_INTRO");
                level.Session.SetFlag(INTRO_FLAG);
            }

            yield return 0.5f;
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
                player.StateMachine.State = Player.StNormal;
        }
    }
}
