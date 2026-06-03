namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 15 Flowey Attack and Transformation - Major Plot Twist
    /// </summary>
    [Tracked]
    public class CS15_FloweyTransformation : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS15_FloweyTransformation(global::Celeste.Player player) : base(false, true)
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
            // Sudden Flowey attack
            level.Flash(Color.Black, false);
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH15_FLOWEY_ATTACK");

            // Flowey transformation to Genocider
            level.Flash(Color.Purple, false);
            level.Shake(5.0f);
            yield return 1f;
            
            yield return Textbox.Say("MAGGYHELPER_CH15_FLOWEY_TRANSFORMATION");

            // World reboot sequence
            level.Flash(Color.White, false);
            yield return 2f;
            
            yield return Textbox.Say("MAGGYHELPER_CH15_WORLD_REBOOT");

            // Chapter ending with ominous tone
            yield return Textbox.Say("MAGGYHELPER_CH15_CHAPTER_END");

            EndCutscene(level);
        }
    }
}



