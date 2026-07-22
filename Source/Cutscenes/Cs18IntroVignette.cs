using System.Collections;
using Celeste;
using Monocle;

namespace DZ;

/// <summary>
/// Intro vignette for Chapter 18 (Heart).
/// Displays chapter introduction and transitions to the level.
/// </summary>
public class Cs18IntroVignette : Scene
{
    private Session session;

    public Cs18IntroVignette(Session session)
    {
        this.session = session;
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
        Logger.Log(LogLevel.Info, "DZ", "Chapter 18 intro vignette displayed");
        yield return 1.5f;
        LevelEnter.Go(session, fromSaveData: false);
    }
}
