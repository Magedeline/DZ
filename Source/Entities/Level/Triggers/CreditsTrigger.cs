using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that triggers credits events.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class CreditsTrigger : CelesteTrigger
{
    public string? Event;

    public CreditsTrigger(Vector2 position, int width, int height, string? eventName = null) : base(position, width, height)
    {
        Event = eventName;
    }

    public override void OnEnter(PlayerController player)
    {
        Triggered = true;
        
        // TODO: Set event on credits instance
        // if (Credits.Instance != null)
        //     Credits.Instance.Event = Event;
    }
}
