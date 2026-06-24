namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 13 Axis Boss Battle
    /// </summary>
    [Tracked]
    public class CS13_AxisBossBattle : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS13_AxisBossBattle(global::Celeste.Player player) : base(false, true)
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
            // Axis approach with heavy footsteps
            level.Shake(2.0f);
            
            yield return Textbox.Say("MAGGYHELPER_CH13_AXIS_APPROACH");

            // Boss introduction
            level.Flash(Color.Red, false);
            level.Shake(1.5f);
            
            yield return Textbox.Say("MAGGYHELPER_CH13_AXIS_BOSS_INTRO");

            // Mid-battle power increase
            yield return 1f;
            level.Shake(2.5f);
            
            yield return Textbox.Say("MAGGYHELPER_CH13_AXIS_BATTLE_MID");

            // Final defeat
            level.Flash(Color.Blue, false);
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH13_AXIS_DEFEATED");

            // Chapter ending
            yield return Textbox.Say("MAGGYHELPER_CH13_CHAPTER_END");

            EndCutscene(level);
        }
    }
}



