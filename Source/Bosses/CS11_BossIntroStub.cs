using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    // Stub implementation - methods disabled due to compilation issues
    // This is a placeholder to allow the project to build
    public class CS11_BossIntroStub : CutsceneEntity
    {
        private Player player;

        public CS11_BossIntroStub(Player player)
            : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public override void OnBegin(Level level)
        {
            if (level.Session.GetFlag("cs11_boss_intro_complete"))
            {
                WasSkipped = true;
                EndCutscene(level);
                return;
            }

            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            Add(new Coroutine(MinimalSequence(level)));
        }

        private IEnumerator MinimalSequence(Level level)
        {
            yield return 1f;
            level.Session.SetFlag("cs11_boss_intro_complete", true);
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            level.TimerStopped = false;
        }
    }
}
