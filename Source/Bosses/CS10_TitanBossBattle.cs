namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 10 Titan Boss Battle cutscene
    /// </summary>
    [Tracked]
    public class CS10_TitanBossBattle : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS10_TitanBossBattle(global::Celeste.Player player) : base(false, true)
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
            // Boss introduction
            level.Flash(Color.Red, false);
            level.Shake(1.0f);
            
            yield return Textbox.Say("CH12_TITAN_BOSS_INTRO");

            // Mid-battle dialogue
            yield return 1f;
            level.Shake(2.0f);
            level.Flash(Color.Yellow, false);
            
            yield return Textbox.Say("CH12_TITAN_BOSS_BATTLE_MID");

            // Boss defeated
            level.Flash(Color.Cyan, false);
            yield return 0.5f;
            
            yield return Textbox.Say("CH12_TITAN_BOSS_DEFEATED");

            // Chapter ending
            yield return Textbox.Say("CH12_CHAPTER_END");

            EndCutscene(level);
        }
    }
}



