using System;
using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.DZ;

public class OverworldTracker
{
    public static event Action<AreaKey> AreaChanged;
    public static event Action<Overworld> OverworldCreated;

    private AreaKey? currArea => SaveData.Instance?.LastArea_Safe;
    private int lastAreaID=-1;
    public Overworld currentOverworld;

    private void AttachToNewOverworld(OuiMainMenu menu, List<MenuButton> buttons)
    {
        currentOverworld = menu.Overworld;
        lastAreaID = -1; // invoke area change
        OverworldCreated?.Invoke(currentOverworld);
        currentOverworld.OnEndOfFrame += PollArea;
    }

    private void PollArea()
    {
        if (currentOverworld == null) return;
        if (currArea.HasValue && lastAreaID!=currArea.Value.ID)
        {
            lastAreaID = currArea.Value.ID;
            AreaChanged?.Invoke(currArea.Value);
        }
        currentOverworld.OnEndOfFrame += PollArea;
    }

    public OverworldTracker()
    {
        Everest.Events.MainMenu.OnCreateButtons += AttachToNewOverworld;
    }

    public void Unload()
    {
        Everest.Events.MainMenu.OnCreateButtons -= AttachToNewOverworld;
    }
}