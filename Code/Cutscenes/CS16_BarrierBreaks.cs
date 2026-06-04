namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 16 Ending - Barrier Breaks and Return to Celeste
    /// </summary>
    [Tracked]
    public class CS16_BarrierBreaks : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS16_BarrierBreaks(global::Celeste.Player player) : base(false, true)
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
            // Barrier finally breaks
            level.Flash(Color.White, false);
            level.Shake(2.0f);
            
            yield return Textbox.Say("MAGGYHELPER_CH16_BARRIER_BREAKS");

            // Farewell to Titan King
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH16_FAREWELL_TITAN_KING");

            // Return to Celeste
            level.Flash(Color.Cyan, false);
            yield return 1f;
            
            yield return Textbox.Say("MAGGYHELPER_CH16_RETURN_TO_CELESTE");

            // Els' ominous final words
            level.Flash(Color.DarkRed, false);
            Audio.Play("guid://{0886e368-bce4-4922-97c3-edeadf714e29}");
            
            // Credits would roll here in actual game
            yield return 2f;

            EndCutscene(level);
        }
    }
}




