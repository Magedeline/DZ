using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

public class CS06_Stronghold : CutsceneEntity
{
    public const string Flag = "theo_1";

    private NPC06_Theo theo;
    private BadelineDummy badeline;

    private Player player;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS06_Stronghold(NPC06_Theo theo, Player player)
    {
        this.theo = theo;
        this.badeline = new BadelineDummy(theo.Position);
        this.player = player;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        player.StateMachine.State = Player.StDummy;
        player.StateMachine.Locked = true;
        player.ForceCameraUpdate = true;
        yield return player.DummyWalkTo(theo.X - 30f);
        player.Facing = Facings.Right;
        yield return Textbox.Say("CH6_THEO_1", WaitABeat, ZoomIn, BaddyAppeared, BaddyTurnsAround, BaddyApproaches, BaddyTurnsRight, BaddyYell, BaddyFlewPastTheo);
        yield return Level.ZoomBack(0.5f);
        EndCutscene(level);
    }

    private IEnumerator BaddyAppeared()
    {
        Scene.Add(badeline);
        badeline.Appear(Level);
        badeline.Position = new Vector2(theo.X + 30f, theo.Y - 8f);
        yield return 0.5f;
    }

    private IEnumerator WaitABeat()
    {
        yield return 1.2f;
    }

    private IEnumerator ZoomIn()
    {
        yield return Level.ZoomTo(new Vector2(123f, 116f), 2f, 0.5f);
    }

    private IEnumerator BaddyTurnsAround()
    {
        yield return 0.2f;
        badeline.Sprite.Scale.X = -1f;
        yield return 0.1f;
    }

        private IEnumerator BaddyTurnsRight()
    {
        yield return 0.2f;
        badeline.Sprite.Scale.X = 1f;
        yield return 0.1f;
    }

    private IEnumerator BaddyYell()
    {
        yield return 0.2f;
        badeline.Sprite.Play("angry");
    }

    private IEnumerator BaddyApproaches()
    {
        yield return player.DummyWalkTo(theo.X - 20f);
    }

    private IEnumerator BaddyFlewPastTheo()
    {
        yield return badeline.FloatTo(new Vector2(theo.X + 30f, theo.Y - 8f), turnAtEndTo: 1, faceDirection: false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        player.X = theo.X + 30f;
        player.StateMachine.Locked = false;
        player.StateMachine.State = Player.StNormal;
        player.ForceCameraUpdate = false;
        if (WasSkipped)
        {
            level.Camera.Position = player.CameraTarget;
        }
        level.Session.SetFlag("theo_1");
    }
}
