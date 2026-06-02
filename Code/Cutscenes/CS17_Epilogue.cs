using global::Celeste.Mod.MaggyHelper;

namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs17Epilogue : CutsceneEntity
{
    public const string FLAG = "ch17_epilogue_trigger";
    private const string PostEpilogueCreditsFlag = "ch21_epilogue_credits_trigger";
    private readonly global::Celeste.Player player;
    private bool launchPostEpilogueCredits;

    public Cs17Epilogue(global::Celeste.Player player) : base(true, false)
    {
        this.player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public override void OnBegin(Level level)
    {
        launchPostEpilogueCredits = !MaggyHelperModule.IsChapter17EpilogueCompleted()
            && !level.Session.GetFlag(PostEpilogueCreditsFlag)
            && !level.Session.GetFlag("epilogue_credits_complete");

        Add(new Coroutine(Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
        if (player?.StateMachine == null) yield break;

        player.StateMachine.State = Player.StDummy; // Dummy state
        yield return 0.5f;

        yield return Textbox.Say("CH17_EPILOGUE");

        yield return 0.5f;
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        bool chainPostEpilogueCredits = launchPostEpilogueCredits
            && level != null
            && !level.Session.GetFlag(PostEpilogueCreditsFlag);

        if (player != null)
            player.StateMachine.State = chainPostEpilogueCredits ? 11 : 0;

        if (!chainPostEpilogueCredits)
            return;

        level.Session.SetFlag(PostEpilogueCreditsFlag);
        level.OnEndOfFrame += () =>
        {
            if (Engine.Scene == level)
                level.Add(new CS21_EpilogueCredits(player));
        };
    }
}
