namespace MaggyHelper.Cutscenes;

[HotReloadable]
public class Cs08Theo : CutsceneEntity
{
    public const string FLAG = "ch8_theo_trigger";
    private readonly global::Celeste.Player player;

    public Cs08Theo(global::Celeste.Player player) : base(true, false)
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

        player.StateMachine.State = 11; // Dummy state
        yield return 0.5f;

        yield return Textbox.Say("CH8_THEO");

        yield return 0.5f;
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
            player.StateMachine.State = 0; // Normal state
    }
}
