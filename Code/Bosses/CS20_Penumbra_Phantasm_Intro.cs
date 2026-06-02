namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs20PenumbraPhantasmIntro : CutsceneEntity
{
    public const string FLAG = "ch20_penumbra_phantasm_intro";
    private readonly global::Celeste.Player player;

    public Cs20PenumbraPhantasmIntro(global::Celeste.Player player) : base(true, false)
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

        player.StateMachine.State = Player.StDummy;
        yield return 0.5f;

        yield return Textbox.Say("CH20_PENUMBRA_PHASTASM_INTRO");

        yield return 0.5f;
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
            player.StateMachine.State = Player.StNormal;
    }
}
