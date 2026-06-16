using Celeste.Cutscenes;
using Celeste.NPCs;

namespace Celeste.Entities;
[CustomEntity(ids: "DZ/PlateauMod")]
[Tracked]
public class PlateauMod : Solid
{
    private Image sprite;
    public readonly LightOcclude Occluder;
    private bool cutsceneTriggered = false;

    public static void Load()
    {
        // no-op: reserved for future hooks
    }

    public PlateauMod(EntityData data, Vector2 offset) 
        : base(data.Position + offset, 104f, 4f, true)
    {
        Collider.Left += 8f;
        Add(sprite = new Image(GFX.Game["scenery/fallplateau"]));
        Add(Occluder = new LightOcclude());
        SurfaceSoundIndex = 23;
        EnableAssistModeChecks = true;
    }

    public PlateauMod(Vector2 position) 
        : base(position, 104f, 4f, true)
    {
        Collider.Left += 8f;
        Add(sprite = new Image(GFX.Game["scenery/fallplateau"]));
        Add(Occluder = new LightOcclude());
        SurfaceSoundIndex = 23;
        EnableAssistModeChecks = true;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        // Check if we should trigger the cs08_campfire cutscene on room entry
        if (scene is Level level)
        {
            // Check if the cutscene hasn't been played yet
            if (!level.Session.GetFlag(Cs08Campfire.FLAG) && !cutsceneTriggered)
            {
                cutsceneTriggered = true;
                
                // The NPC08_Madeline_Plateau will handle triggering the cutscene
                // This flag check ensures we only allow it when appropriate
                Logger.Log(LogLevel.Info, "DZ", 
                    "PlateauMod: Room entered, cs08_campfire cutscene ready to trigger");
            }
        }
    }
}




