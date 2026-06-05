namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 10 Sans Restaurant cutscene with food interaction
    /// </summary>
    [Tracked]
    public class CS10_SansRestaurant : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS10_SansRestaurant(global::Celeste.Player player) : base(false, true)
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
            // Sans pranks and serves food
            yield return Textbox.Say("MAGGYHELPER_CH12_SANS");

            // Food ordering sequence
            yield return 0.5f;
            Audio.Play("event:/game/general/touchswitch_any");

            yield return Textbox.Say("MAGGYHELPER_CH12_ORDERUP");

            // Bad food reaction effects
            level.Shake(0.3f);
            yield return 0.5f;

            yield return Textbox.Say("MAGGYHELPER_CH12_ORDERUP_END");

            // Madeline and Kirby private conversation
            yield return Textbox.Say("MAGGYHELPER_CH12_MADELINE_AND_KIRBY");

            EndCutscene(level);
        }
    }
}




