using System.Collections;
using Celeste;
using Monocle;

namespace DZ;

/// <summary>
/// Outro vignette for Chapter 4 (Legend).
/// Displays chapter completion message and transitions back to overworld.
/// </summary>
public class Cs04LegendVignette : Scene
{
    private Session session;

    public Cs04LegendVignette(Session session)
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
        Logger.Log(LogLevel.Info, "DZ", "Chapter 4 legend vignette displayed");
        yield return 1.5f;
        Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, null);
    }
}
