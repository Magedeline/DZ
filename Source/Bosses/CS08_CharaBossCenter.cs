namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs08CharaBossCenter(global::Celeste.Player player) : CutsceneEntity
{
    public const string FLAG = "charaboss_center";
    private readonly global::Celeste.Player player = player;

    public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

    private IEnumerator Cutscene(Level level)
    {
        Cs08CharaBossCenter bossCenter = this;

        // Wait until the player is assigned or added.
        while (bossCenter.player == null)
        {
            yield return null;
        }

        // Lock the player's state and force a specific movement sequence.
        bossCenter.player.StateMachine.State = 11;
        bossCenter.player.StateMachine.Locked = true;

        // Simulate walking action and camera behavior for the cutscene.
        yield return bossCenter.player.DummyWalkToExact((int)bossCenter.player.X + 20);
        yield return level.ZoomTo(new Vector2(80f, 110f), 2f, 0.5f);

        // Play the respective cutscene dialog.
        yield return Textbox.Say("CH8_CHARA_BOSS_AFTER_MIDDLE");

        // Add a delay with the camera and cutscene cleanup.
        yield return 0.1f;
        yield return level.ZoomBack(0.4f);

        // Mark the cutscene end with cleanup or transitions.
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
        {
            // Reset player state
            player.StateMachine.Locked = false;

            if (WasSkipped)
            {
                player.StateMachine.State = 11;
            }
        }
        level.Session.SetFlag("charaboss_center");
    }
}



