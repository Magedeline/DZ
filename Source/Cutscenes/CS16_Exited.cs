namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs16Exited : CutsceneEntity
{
    public const string FLAG = "ch16_exited_trigger";
    private readonly global::Celeste.Player player;

    public Cs16Exited(global::Celeste.Player player) : base(true, false)
    {
        this.player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
        if (player?.StateMachine == null) yield break;

        player.StateMachine.State = Player.StDummy; // Dummy state
        yield return 0.5f;

        yield return Textbox.Say("DZ_CH16_EXITED");

        yield return 0.5f;
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
            player.StateMachine.State = Player.StNormal; // Normal state
    }
}
