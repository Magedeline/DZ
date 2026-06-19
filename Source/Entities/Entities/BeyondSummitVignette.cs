using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using Celeste.Cutscenes;
using Monocle;

namespace DZ;

public class VortexVignette : DesoloZantasVignette
{
    private CompleteRenderer complete;

    private bool slideFinished;

    private Session session;

    private bool ending;

    private bool ready;

    private bool addedRenderer;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public VortexVignette(Session session)
    {
        this.session = session;
        session.Audio.Apply(false);
        RunThread.Start(LoadCompleteThread, "VORTEX_VIGNETTE");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LoadCompleteThread()
    {
        Atlas atlas = null;
        XmlElement xmlElement = GFX.CompleteScreensXml["Screens"]["VortexIntro"];
        if (xmlElement != null)
        {
            atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", xmlElement.Attr("atlas")), Atlas.AtlasDataFormat.PackerNoAtlas);
        }
        complete = new CompleteRenderer(xmlElement, atlas, 0f, delegate
        {
            slideFinished = true;
        });
        complete.SlideDuration = 7.5f;
        ready = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        if (ready && !addedRenderer)
        {
            Add(complete);
            addedRenderer = true;
        }
        base.Update();
        if ((Input.MenuConfirm.Pressed || slideFinished) && !ending && ready)
        {
            ending = true;
            new MountainWipe(this, wipeIn: false, [MethodImpl(MethodImplOptions.NoInlining)] () =>
            {
                Engine.Scene = new LevelLoader(session);
            });
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void End()
    {
        base.End();
        if (complete != null)
        {
            complete.Dispose();
        }
    }
}
