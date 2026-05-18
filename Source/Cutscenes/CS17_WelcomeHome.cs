using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

public class CS17_WelcomeHome : CutsceneEntity
{
    private Player player;

    private float targetX;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS17_WelcomeHome(Player player, float targetX)
    {
        this.player = player;
        this.targetX = targetX;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        player.StateMachine.State = 11;
        Add(new Coroutine(player.DummyWalkToExact((int)targetX, walkBackwards: false, 0.7f)));
        Add(new Coroutine(level.ZoomTo(new Vector2(targetX - level.Camera.X, 90f), 2f, 2f)));
        FadeWipe fadeWipe = new FadeWipe(level, wipeIn: false);
        fadeWipe.Duration = 2f;
        yield return fadeWipe.Wait();
        EndCutscene(level);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        level.OnEndOfFrame += [MethodImpl(MethodImplOptions.NoInlining)] () =>
        {
            level.Remove(player);
            level.UnloadLevel();
            level.Session.Level = "inside";
            level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
            level.LoadLevel(Player.IntroTypes.None);
            level.Add(new CS17_EndingMod());
        };
    }
}
