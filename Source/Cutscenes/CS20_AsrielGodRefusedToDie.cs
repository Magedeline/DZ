using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Asriel God Boss "refused to die" sequence for K_Player and SoulPlayer.
    /// When the player/soul would fall during the God Boss battle, this cutscene
    /// plays the struck-down dialog, refills HP, and resumes the fight.
    /// </summary>
    [HotReloadable]
    public class CS20_AsrielGodRefusedToDie : CutsceneEntity
    {
        private readonly K_Player kirbyPlayer;
        private readonly SoulPlayer soulPlayer;
        private Level level;

        public CS20_AsrielGodRefusedToDie(K_Player kirbyPlayer)
            : base(true, false)
        {
            this.kirbyPlayer = kirbyPlayer ?? throw new ArgumentNullException(nameof(kirbyPlayer));
        }

        public CS20_AsrielGodRefusedToDie(SoulPlayer soulPlayer)
            : base(true, false)
        {
            this.soulPlayer = soulPlayer ?? throw new ArgumentNullException(nameof(soulPlayer));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void OnBegin(Level level)
        {
            this.level = level;
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            // Freeze the player/soul
            if (kirbyPlayer != null)
            {
                kirbyPlayer.StateMachine.State = global::Celeste.Player.StDummy;
                kirbyPlayer.StateMachine.Locked = true;
                kirbyPlayer.Speed = Vector2.Zero;
            }
            else if (soulPlayer != null)
            {
                soulPlayer.Controller.SetMovementEnabled(false);
                soulPlayer.Controller.Reset();
            }

            level.Shake(1.0f);
            level.Flash(Color.Gold * 0.5f, true);

            yield return Textbox.Say("DZ_CH20_KIRBY_STRUCK_DOWN");

            // Refuse to die - refill HP and visual feedback
            if (kirbyPlayer != null)
            {
                kirbyPlayer.RefillHealth();
            }
            else if (soulPlayer != null)
            {
                soulPlayer.OnRefused();
            }

            SpawnVoidEffects();
            level.Shake(0.5f);
            level.Flash(Color.Cyan * 0.4f);
            yield return 1.0f;

            EndCutscene(level);
        }

        private void SpawnVoidEffects()
        {
            Vector2 center = kirbyPlayer?.Center ?? soulPlayer?.Center ?? Position;
            if (level == null) return;

            for (int i = 0; i < 50; i++)
            {
                Vector2 pos = center + Calc.Random.Range(new Vector2(-100, -100), new Vector2(100, 100));
                level.ParticlesFG.Emit(FlyFeather.P_Boost, pos);
            }

            Audio.Play("event:/game/general/save");
        }

        public override void OnEnd(Level level)
        {
            if (kirbyPlayer != null)
            {
                kirbyPlayer.ReviveFromRefusedDeath();
            }
            else if (soulPlayer != null)
            {
                soulPlayer.Controller.SetMovementEnabled(true);
                soulPlayer.IsDead = false;
            }
        }
    }
}
