namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Chapter 13 Introduction - Mechanical Nightmares
    /// </summary>
    [Tracked]
    public class CS13_Intro : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS13_Intro(global::Celeste.Player player) : base(false, true)
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
            // Industrial atmosphere introduction
            yield return Textbox.Say("CH13_INTRO");

            // Factory entrance warning
            yield return 0.5f;
            Audio.Play("event:/game/general/strawberry_pulse");
            
            yield return Textbox.Say("CH13_FACTORY_ENTRANCE");

            EndCutscene(level);
        }
    }
}



