using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Asriel Zero "soul refused to die" sequence for the SoulPlayer/BattlePlayerController path.
    /// When the soul would fall in the Asriel Angel of Death battle, this cutscene plays the
    /// rise/kill/struggle/void dialog, refills the soul's HP, and lets the battle continue.
    /// </summary>
    [HotReloadable]
    public class CS20_SoulRefusedToDie : CutsceneEntity
    {
        private readonly SoulPlayer soul;
        private Level level;

        public CS20_SoulRefusedToDie(SoulPlayer soul)
            : base(true, false)
        {
            this.soul = soul ?? throw new ArgumentNullException(nameof(soul));
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
            if (soul == null) yield break;

            // Freeze the soul and any active battle controller
            soul.Controller.SetMovementEnabled(false);
            soul.Controller.Reset();

            // Dramatic rise and kill sequence
            level.Shake(1.0f);
            level.Flash(Color.Gold * 0.5f, true);
            yield return Textbox.Say("DZ_CH20_ASRIEL_ZERO_RISE_KILL");

            // The soul refuses to die
            yield return Textbox.Say("DZ_CH20_ASRIEL_ZERO_STRUGGLE_START");

            // Void answers
            yield return 1.0f;
            yield return Textbox.Say("DZ_CH20_ASRIEL_ZERO_CALL_VOID");
            yield return Textbox.Say("DZ_CH20_ASRIEL_ZERO_VOID_ANSWERS");
            yield return Textbox.Say("DZ_CH20_ASRIEL_ZERO_VOID_GUIDANCE");

            // Visual void effects
            SpawnVoidEffects();

            // Refill the soul's HP and mark it as having refused
            soul.OnRefused();
            level.Shake(0.5f);
            level.Flash(Color.Cyan * 0.4f);
            yield return 1.0f;

            // Resume battle
            EndCutscene(level);
        }

        private void SpawnVoidEffects()
        {
            if (level == null || soul == null) return;

            for (int i = 0; i < 50; i++)
            {
                Vector2 pos = soul.Center + Calc.Random.Range(new Vector2(-100, -100), new Vector2(100, 100));
                level.ParticlesFG.Emit(FlyFeather.P_Boost, pos);
            }

            Audio.Play("event:/game/general/save");
        }

        public override void OnEnd(Level level)
        {
            if (soul != null)
            {
                soul.Controller.SetMovementEnabled(true);
                soul.IsDead = false;
            }
        }
    }
}
