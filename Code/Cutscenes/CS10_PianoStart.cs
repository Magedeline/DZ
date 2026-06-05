namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 10 Piano Start cutscene
    /// </summary>
    [Tracked]
    public class CS10_PianoStart : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CS10_PianoStart(global::Celeste.Player player) : base(false, true)
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
            // Player approaches the piano
            yield return Textbox.Say("MAGGYHELPER_CH12_PIANO_START");

            // Zoom in to madeline and piano (convert world position to screen-space)
            Vector2 screenSpacePosition = new Vector2(player.X, player.Y - 30) - level.Camera.Position;
            yield return level.ZoomTo(screenSpacePosition, 2f, 1f);

            // Piano interaction effect
            yield return 0.5f;
            Audio.Play("event:/game/general/touchswitch_any");
            level.Flash(Color.LightBlue, false);

            // Zoom back out
            yield return level.ZoomBack(1f);

            EndCutscene(level);
        }
    }
}




