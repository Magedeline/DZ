namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 13 Meta Knight Encounter - Metaminator Knight Battle
    /// </summary>
    [Tracked]
    public class CS13_MetaKnightEncounter : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS13_MetaKnightEncounter(global::Celeste.Player player) : base(false, true)
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
            // Meta Knight appears
            level.Flash(Color.Blue, false);
            
            yield return Textbox.Say("MAGGYHELPER_CH13_META_KNIGHT_ENCOUNTER");

            // Transformation to Metaminator Knight
            level.Shake(1.0f);
            level.Flash(Color.Red, false);
            
            yield return Textbox.Say("MAGGYHELPER_CH13_METAMINATOR_KNIGHT_INTRO");

            // Battle sequence
            yield return 1f;
            level.Shake(1.5f);
            
            yield return Textbox.Say("MAGGYHELPER_CH13_METAMINATOR_BATTLE_MID");

            // Defeat and restoration
            level.Flash(Color.White, false);
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH13_METAMINATOR_DEFEATED");

            EndCutscene(level);
        }
    }
}



