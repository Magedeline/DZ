using System.Collections;
using Celeste;
using Monocle;

namespace DZ;

/// <summary>
/// Outro vignette for Chapter 3 (The Stars).
/// Displays chapter completion message and transitions back to overworld.
/// </summary>
public class Cs03OutroVignette : Scene
{
    private Session session;

    public Cs03OutroVignette(Session session)
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
        Logger.Log(LogLevel.Info, "DZ", "Chapter 3 outro vignette displayed");
        yield return 1.5f;
        Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, null);
    }
}
