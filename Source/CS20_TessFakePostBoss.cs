namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs20TessFakePostBoss : CutsceneEntity
{
    public const string FLAG = "ch20_tess_fake_post_boss_trigger";
    private readonly global::Celeste.Player player;

    public Cs20TessFakePostBoss(global::Celeste.Player player) : base(true, false)
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

        yield return Textbox.Say("CH20_TESS_FAKE_POST_BOSS");

        yield return 0.5f;
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
            player.StateMachine.State = 0; // Normal state
    }
}
