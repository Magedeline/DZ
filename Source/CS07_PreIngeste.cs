using BadelineDummy = MaggyHelper.Entities.BadelineDummy;
using MaggyHelper.Entities;

namespace MaggyHelper.Cutscenes;

[HotReloadable]
public class Cs07PreIngeste : CutsceneEntity
{
    public const string FLAG = "ch7_pre_ingeste_trigger";

    private static readonly Vector2 BadelineOffset = new(24f, -16f);
    private static readonly Vector2 RalseiOffset = new(-24f, -16f);

    private readonly global::Celeste.Player player;
    private BadelineDummy badeline;
    private RalseiDummy ralsei;

    public Cs07PreIngeste(global::Celeste.Player player) : base(true, false)
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

        // Spawn Badeline floating near Kirby
        badeline = new BadelineDummy(player.Position + BadelineOffset);
        Scene.Add(badeline);
        badeline.Appear(level);
        yield return 0.3f;

        // Spawn Ralsei floating on the other side
        ralsei = new RalseiDummy(player.Position + RalseiOffset);
        Scene.Add(ralsei);
        ralsei.Appear(level);
        yield return 0.3f;

        // Float Badeline and Ralsei to follow Kirby's sides
        Add(new Coroutine(badeline.FloatTo(player.Position + BadelineOffset, null, true, false, true)));
        Add(new Coroutine(ralsei.FloatTo(player.Position + RalseiOffset, null, true, false, true)));
        yield return 0.2f;

        // Talk to Chara with Badeline and Ralsei present
        yield return Textbox.Say("CH7_PRE_INGESTE_0");
        yield return 0.3f;

        yield return Textbox.Say("CH7_PRE_INGESTE_1");
        yield return 0.3f;

        yield return Textbox.Say("CH7_PRE_INGESTE_2");
        yield return 0.3f;

        yield return Textbox.Say("CH7_PRE_INGESTE_3");

        yield return 0.5f;

        // Vanish companions
        badeline?.Vanish();
        ralsei?.Vanish();
        yield return 0.3f;

        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
            player.StateMachine.State = 11; // Normal state

        // Clean up companions if cutscene was skipped
        if (badeline?.Scene != null)
            badeline.RemoveSelf();
        if (ralsei?.Scene != null)
            ralsei.RemoveSelf();
    }
}
