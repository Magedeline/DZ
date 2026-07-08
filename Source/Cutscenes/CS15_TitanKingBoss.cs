using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 15 Titan King boss intro cutscene.
    /// Plays the Roaring Titan King battle dialog sequence.
    /// </summary>
    [HotReloadable]
    public class CS15_TitanKingBoss : CutsceneEntity
    {
        public const string FLAG = "ch15_titan_king_boss_intro";
        private readonly global::Celeste.Player player;

        public CS15_TitanKingBoss(global::Celeste.Player player) : base(true, false)
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
            level.Shake(1.0f);
            yield return 0.5f;

            yield return Textbox.Say("DZ_CH15_ROARING_TITAN_KING_BATTLE_FINAL");
            yield return Textbox.Say("DZ_CH15_ROARING_TITAN_HYPER_KING");

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
