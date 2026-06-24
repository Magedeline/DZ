using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Linq;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that displays a mini textbox when activated.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class MiniTextboxTrigger : CelesteTrigger
{
    private string[] dialogOptions;
    private Modes mode;
    private bool triggered;
    private bool onlyOnce;
    private int deathCount;
    private string id;

    public enum Modes
    {
        OnPlayerEnter,
        OnLevelStart,
        OnTheoEnter
    }

    public MiniTextboxTrigger(Vector2 position, int width, int height, string dialogId, Modes mode = Modes.OnPlayerEnter, 
        bool onlyOnce = false, int deathCount = -1, string id = "") : base(position, width, height)
    {
        this.mode = mode;
        dialogOptions = dialogId.Split(',');
        this.onlyOnce = onlyOnce;
        this.deathCount = deathCount;
        this.id = id;
    }

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        if (mode == Modes.OnLevelStart)
            Trigger();
    }

    public override void OnEnter(PlayerController player)
    {
        if (mode != Modes.OnPlayerEnter)
            return;
        Trigger();
    }

    private void Trigger()
    {
        if (triggered)
            return;
        
        // TODO: Check death count if specified
        // if (deathCount >= 0 && Session.DeathsInCurrentLevel != deathCount)
        //     return;
        
        triggered = true;
        
        // TODO: Show mini textbox with random dialog choice
        // Scene.Add(new MiniTextbox(Nez.Random.Choose(dialogOptions)));
        
        if (onlyOnce)
        {
            // TODO: Add to DoNotLoad
            // Session.DoNotLoad.Add(id);
        }
    }
}
