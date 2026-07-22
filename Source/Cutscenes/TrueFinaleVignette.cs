using System.Collections;
using Celeste;
using Monocle;

namespace DZ;

/// <summary>
/// Intro vignette for Chapter 21 (True Finale).
/// Displays the final chapter introduction and transitions to the level.
/// </summary>
public class TrueFinaleVignette : Scene
{
    private Session session;

    public TrueFinaleVignette(Session session)
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
        Logger.Log(LogLevel.Info, "DZ", "Chapter 21 true finale vignette displayed");
        yield return 1.5f;
        LevelEnter.Go(session, fromSaveData: false);
    }
}
