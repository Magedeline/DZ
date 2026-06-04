namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 15 Roaring Titan Council Judgment
    /// </summary>
    [Tracked]
    public class CS15_TitanCouncilJudgment : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS15_TitanCouncilJudgment(global::Celeste.Player player) : base(false, true)
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
            // Judgment ceremony begins
            level.Flash(Color.Purple, false);
            
            yield return Textbox.Say("MAGGYHELPER_CH15_JUDGMENT_CEREMONY");

            // Roaring Titan Council entrance
            level.Shake(1.0f);
            Audio.Play("guid://{0886e368-bce4-4922-97c3-edeadf714e29}");
            
            yield return Textbox.Say("MAGGYHELPER_CH15_ROARING_TITAN_COUNCIL_ENTRANCE");

            // Council judgment results
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH15_COUNCIL_JUDGMENT_RESULTS");

            // Barrier revelation
            level.Flash(Color.White, false);
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH15_BARRIER_REVELATION");

            EndCutscene(level);
        }
    }
}




