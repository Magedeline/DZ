namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 14 Digital Dimension Introduction
    /// </summary>
    [Tracked]
    public class CS14_Intro : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS14_Intro(global::Celeste.Player player) : base(false, true)
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
            // Digital world introduction
            level.Flash(Color.Purple, false);
            
            yield return Textbox.Say("MAGGYHELPER_CH14_INTRO");

            // Digital guide introduction
            yield return 0.5f;
            Audio.Play("event:/game/general/touchswitch_any");
            
            yield return Textbox.Say("MAGGYHELPER_CH14_DIGITAL_WORLD_EXPLORATION");

            EndCutscene(level);
        }
    }
}




