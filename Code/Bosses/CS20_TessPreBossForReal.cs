namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs20TessPreBossForReal : CutsceneEntity
{
    public const string FLAG = "ch20_tess_pre_boss_for_real_trigger";
    private readonly global::Celeste.Player player;

    public Cs20TessPreBossForReal(global::Celeste.Player player) : base(true, false)
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

        yield return Textbox.Say("CH20_TESS_PRE_BOSS_FOR_REAL");

        yield return 0.5f;
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
            player.StateMachine.State = Player.StNormal; // Normal state
    }
}
