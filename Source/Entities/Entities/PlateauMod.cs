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
        On.Celeste.Player.OnTransition += OnPlayerTransition;
    }

    public static void Unload()
    {
        On.Celeste.Player.OnTransition -= OnPlayerTransition;
    }

    // When the player transitions to a new room while riding a PlateauMod,
    // remove the plateau so it doesn't carry through to the next room
    // (matching vanilla FallPlateau behaviour).
    private static void OnPlayerTransition(On.Celeste.Player.orig_OnTransition orig, global::Celeste.Player self)
    {
        orig(self);
        // If the player was standing on a PlateauMod, remove it — it belongs to
        // the room being left, not the one being entered.
        if (self.Scene?.Tracker.GetEntity<PlateauMod>() is PlateauMod plateau
            && plateau.HasPlayerRider())
        {
            plateau.RemoveSelf();
        }
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
            if (!level.Session.GetFlag(Cs08Campfire.FLAG) && !level.Session.GetFlag(CS08_StarJumpEnd.Flag) && !cutsceneTriggered)
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




