namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs12UndyneRefusedToDied : CutsceneEntity
{
    public const string FLAG = "ch12_undyne_refused_to_died_trigger";
    private readonly global::Celeste.Player player;

    public Cs12UndyneRefusedToDied(global::Celeste.Player player) : base(true, false)
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

        yield return Textbox.Say("DZ_CH12_UNDYNE_REFUSED_TO_DIED");

        yield return 0.5f;
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
            player.StateMachine.State = Player.StNormal; // Normal state
    }
}
