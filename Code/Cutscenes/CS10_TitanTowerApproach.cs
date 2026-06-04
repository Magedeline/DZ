namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 10 Titan Tower Approach cutscene
    /// </summary>
    [Tracked]
    public class CS10_TitanTowerApproach : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS10_TitanTowerApproach(global::Celeste.Player player) : base(false, true)
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
            // Approaching the massive tower
            yield return Textbox.Say("MAGGYHELPER_CH12_TITAN_TOWER_APPROACH");

            // Climbing sequence
            yield return Textbox.Say("MAGGYHELPER_CH12_TITAN_TOWER_CLIMBING_START");

            // Mid-climb warnings
            yield return 0.5f;
            Audio.Play("guid://{0886e368-bce4-4922-97c3-edeadf714e29}");
            
            yield return Textbox.Say("MAGGYHELPER_CH12_TITAN_TOWER_MID_CLIMB");

            // Summit approach
            yield return Textbox.Say("MAGGYHELPER_CH12_TITAN_TOWER_SUMMIT_APPROACH");

            EndCutscene(level);
        }
    }
}




