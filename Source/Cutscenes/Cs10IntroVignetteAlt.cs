using System.Collections;
using Celeste;
using Monocle;

namespace DZ;

/// <summary>
/// Intro vignette for Chapter 10 (Ruins).
/// Displays chapter introduction and transitions to the level.
/// </summary>
public class Cs10IntroVignetteAlt : Scene
{
    private Session session;
    private bool completed;

    public Cs10IntroVignetteAlt(Session session)
    {
        this.session = session;
        this.completed = false;
    }

    public override void Begin()
    {
        base.Begin();
        var entity = new Entity();
        entity.Add(new Coroutine(Routine()));
        Add(entity);
    }

    private IEnumerator Routine()
    {
        yield return 0.5f;
        Logger.Log(LogLevel.Info, "DZ", "Chapter 10 intro vignette displayed");
        yield return 1.5f;
        completed = true;
        LevelEnter.Go(session, fromSaveData: false);
    }
}
