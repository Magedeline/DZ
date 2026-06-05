namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 15 Mountain Peak Arrival and Castle Approach
    /// </summary>
    [Tracked]
    public class CS15_MountainPeakArrival : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS15_MountainPeakArrival(global::Celeste.Player player) : base(false, true)
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
            // Arrival at the peak with floating castle
            yield return Textbox.Say("MAGGYHELPER_CH15_MOUNTAIN_PEAK_ARRIVAL");

            // Chara's moment of recognition
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH15_CHARA_MOMENT");

            // Bridge crossing with mystical effects
            Audio.Play("event:/game/general/strawberry_pulse");
            level.Flash(Color.Purple, false);
            
            yield return Textbox.Say("MAGGYHELPER_CH15_BRIDGE_CROSSING");

            // Castle entrance
            yield return Textbox.Say("MAGGYHELPER_CH15_HOVERING_CASTLE_ENTRANCE");

            EndCutscene(level);
        }
    }
}




