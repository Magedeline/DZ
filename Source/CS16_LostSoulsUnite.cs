namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 16 Lost Souls Unite Against Els
    /// </summary>
    [Tracked]
    public class CS16_LostSoulsUnite : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS16_LostSoulsUnite(global::Celeste.Player player) : base(false, true)
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
            // Lost souls calling for help
            yield return Textbox.Say("CH16_LOST_SOULS_CALLING");

            // Calling for help from all friends
            level.Flash(Color.White, false);
            
            yield return Textbox.Say("CH16_CALLING_FOR_HELP");

            // Eight souls unite
            level.Flash(Color.Cyan, false);
            yield return 1f;
            
            yield return Textbox.Say("CH16_EIGHT_SOULS_UNITE");

            EndCutscene(level);
        }
    }
}



