namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 14 Hollow Programmer (Digital Dedede) Battle
    /// </summary>
    [Tracked]
    public class CS14_HollowProgrammer : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS14_HollowProgrammer(global::Celeste.Player player) : base(false, true)
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
            // Dedede corruption discovery
            level.Flash(Color.Purple, false);
            
            yield return Textbox.Say("MAGGYHELPER_CH14_DEDEDE_CORRUPTION_DISCOVERY");

            // Hollow Programmer confrontation
            level.Shake(1.0f);
            level.Flash(Color.Magenta, false);
            
            yield return Textbox.Say("MAGGYHELPER_CH14_HOLLOW_PROGRAMMER_CONFRONTATION");

            // Battle sequence
            yield return 1f;
            level.Flash(Color.Magenta, false);
            level.Shake(1.2f);
            
            yield return Textbox.Say("MAGGYHELPER_CH14_HOLLOW_PROGRAMMER_BATTLE");

            // Mid-battle restoration attempts
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH14_HOLLOW_PROGRAMMER_MID_BATTLE");

            // Final defeat and restoration
            level.Flash(Color.White, false);
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH14_HOLLOW_PROGRAMMER_DEFEATED");

            EndCutscene(level);
        }
    }
}



