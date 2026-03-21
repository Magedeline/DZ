using System.Runtime.CompilerServices;

namespace MaggyHelper.Cutscenes;

[HotReloadable]
public class CS07_Darker(global::Celeste.Player player) : CutsceneEntity
{
    public const string Flag = "reflection";

    private readonly global::Celeste.Player player = player;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        player.StateMachine.State = 11;
        player.StateMachine.Locked = true;
        player.ForceCameraUpdate = true;
        TempleMirror templeMirror = Scene.Entities.FindFirst<TempleMirror>();
        yield return player.DummyWalkTo(templeMirror.Center.X + 8f);
        yield return 0.2f;
        player.Facing = Facings.Left;
        yield return 0.3f;
        if (!player.Dead)
        {
            yield return Textbox.Say("ch7_darker", BadelineFallsToKnees, BadelineStopsPanicking, BadelineGetsUp);
        }
        else
        {
            yield return 100f;
        }
        yield return Level.ZoomBack(0.5f);
        EndCutscene(level);
    }

    private IEnumerator BadelineFallsToKnees()
    {
        yield return 0.2f;
        player.DummyAutoAnimate = false;
        player.Sprite.Play("tired");
        yield return 0.2f;
        yield return Level.ZoomTo(new Vector2(90f, 116f), 2f, 0.5f);
        yield return 0.2f;
    }

    private IEnumerator BadelineStopsPanicking()
    {
        yield return 0.8f;
        player.Sprite.Play("tiredStill");
        yield return 0.4f;
    }

    private IEnumerator BadelineGetsUp()
    {
        player.DummyAutoAnimate = true;
        player.Sprite.Play("idle");
        yield break;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        CutsceneManager.ResetPlayerState(player);
        player.StateMachine.Locked = false;
        player.StateMachine.State = 0;
        player.ForceCameraUpdate = false;
        player.FlipInReflection = false;
        level.Session.SetFlag("reflection");
    }
}




