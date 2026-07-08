namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 16 Corrupted Reality Introduction - The True Darkness
    /// </summary>
    [Tracked]
    public class CS16_CorruptedRealityIntro : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS16_CorruptedRealityIntro(global::Celeste.Player player) : base(false, true)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(CutsceneSequence(level)));
        }

        public override void OnEnd(Level level)
        {
            // Cleanup when cutscene ends
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Corrupted reality introduction
            level.Flash(Color.DarkRed, false);
            
            yield return Textbox.Say("DZ_CH16_CORRUPTED_REALITY_INTRO");

            // Horrific battle begins
            level.Flash(Color.Purple, false);
            level.Shake(4.0f);
            
            yield return Textbox.Say("DZ_CH16_HORRIFIC_BATTLE_BEGIN");

            // First phase battle
            yield return 1f;
            level.Shake(2.0f);
            
            yield return Textbox.Say("DZ_CH16_FIRST_PHASE_BATTLE");

            EndCutscene(level);
        }
    }
}



