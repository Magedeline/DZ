namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 10 Roxus Ship Start cutscene
    /// </summary>
    [Tracked]
    public class CS10_RoxusStart : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS10_RoxusStart(global::Celeste.Player player) : base(false, true)
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
            // Ship emerges from water effect
            level.Shake(0.5f);
            Audio.Play("event:/game/general/strawberry_get_1up");
            
            yield return Textbox.Say("DZ_CH12_START_ROXUS");

            // Transition to ship interior
            level.Flash(Color.Blue, false);
            yield return 1f;

            yield return Textbox.Say("DZ_CH12_START_ONBAORD");

            EndCutscene(level);
        }
    }
}



