namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 15 Roaring Titan King Battle
    /// </summary>
    [Tracked]
    public class CS15_RoaringTitanKingBattle : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS15_RoaringTitanKingBattle(global::Celeste.Player player) : base(false, true)
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
            // Ceremony of flame begins
            level.Flash(Color.Orange, false);
            level.Shake(1.0f);
            
            yield return Textbox.Say("MAGGYHELPER_CH15_CEREMONY_OF_FLAME");

            // Roaring Titan King battle
            level.Shake(2.0f);
            yield return 1f;
            
            yield return Textbox.Say("MAGGYHELPER_CH15_ROARING_TITAN_KING_BATTLE");

            // Near defeat - emotional moment
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH15_ROARING_TITAN_KING_NEAR_DEFEAT");

            // Emotional revelation
            yield return Textbox.Say("MAGGYHELPER_CH15_EMOTIONAL_REVELATION");

            // The first wish granted
            level.Flash(Color.Cyan, false);
            yield return 0.5f;
            
            yield return Textbox.Say("MAGGYHELPER_CH15_THE_FIRST_WISH");

            EndCutscene(level);
        }
    }
}



