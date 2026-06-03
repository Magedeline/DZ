namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 14 Giant Axis Final Battle
    /// </summary>
    [Tracked]
    public class CS14_GiantAxisBattle : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS14_GiantAxisBattle(global::Celeste.Player player) : base(false, true)
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
            // Giant Axis emerges
            level.Shake(3.0f);
            
            yield return Textbox.Say("MAGGYHELPER_CH14_GIANT_AXIS_APPROACH");

            // Battle introduction
            level.Flash(Color.Red, false);
            level.Shake(4.0f);
            
            yield return Textbox.Say("MAGGYHELPER_CH14_GIANT_AXIS_BATTLE_INTRO");

            // Mid-battle power drain
            yield return 1f;
            level.Shake(2.0f);
            
            yield return Textbox.Say("MAGGYHELPER_CH14_GIANT_AXIS_MID_BATTLE");

            // Final phase and defeat
            level.Flash(Color.Red, false);
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH14_GIANT_AXIS_FINAL_PHASE");

            // Digital dimension restored
            level.Flash(Color.Cyan, false);
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH14_DIGITAL_DIMENSION_RESTORED");

            // Chapter ending
            yield return Textbox.Say("MAGGYHELPER_CH14_CHAPTER_END");

            EndCutscene(level);
        }
    }
}



