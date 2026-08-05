namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs20Nothingness : CutsceneEntity
{
    public const string FLAG = "ch20_nothingness_trigger";

    private const string DIALOG_VOID_AWAKENING = "DZ_CH20_VOID_AWAKENING";
    private const string DIALOG_ARRIVAL = "DZ_CH20_GRANNY_GASTER_TITANKING_AND_KIRBYPARENTS_ARRIVAL";

    private readonly global::Celeste.Player player;
    private Level level;

    public Cs20Nothingness(global::Celeste.Player player) : base(true, false)
    {
        this.player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public override void OnBegin(Level level)
    {
        this.level = level;
        Add(new Coroutine(Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
        if (player?.StateMachine == null) yield break;

        player.StateMachine.State = Player.StDummy;
        player.StateMachine.Locked = true;
        yield return 0.5f;

        yield return Textbox.Say(
            DIALOG_VOID_AWAKENING,
            Trigger0_FloatInVoid,
            Trigger1_DistantLightAppears
        );

        yield return Textbox.Say(
            DIALOG_ARRIVAL,
            null,
            null,
            Trigger2_DreamFountainAppears,
            Trigger3_DreamLightTransfer
        );

        yield return 0.5f;
        EndCutscene(level);
    }

    private IEnumerator Trigger0_FloatInVoid()
    {
        yield return 0.35f;
    }

    private IEnumerator Trigger1_DistantLightAppears()
    {
        level?.Shake(0.15f);
        yield return 0.25f;
    }

    private IEnumerator Trigger2_DreamFountainAppears()
    {
        Glitch.Value = 0.2f;
        level?.Shake(0.35f);
        yield return 0.35f;
        Glitch.Value = 0f;
    }

    private IEnumerator Trigger3_DreamLightTransfer()
    {
        Glitch.Value = 0.1f;
        yield return 0.5f;
        Glitch.Value = 0f;
    }

    public override void OnEnd(Level level)
    {
        Glitch.Value = 0f;

        if (player != null)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = Player.StNormal;
        }
    }
}
