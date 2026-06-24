namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs21ELSTerminaBossIntro : CutsceneEntity
{
    public const string FLAG = "ch21_els_termina_boss_intro";
    private readonly global::Celeste.Player player;

    public Cs21ELSTerminaBossIntro(global::Celeste.Player player) : base(true, false)
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

        yield return Textbox.Say("MAGGYHELPER_CH21_ELS_TERMINA_BOSS_INTRO");

        yield return 0.5f;
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
            player.StateMachine.State = Player.StNormal;
    }
}
