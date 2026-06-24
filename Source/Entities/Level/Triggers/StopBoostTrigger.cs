using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that stops the player's boost/summit launch state.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class StopBoostTrigger : CelesteTrigger
{
    public StopBoostTrigger(Vector2 position, int width, int height) : base(position, width, height)
    {
    }

    public override void OnEnter(PlayerController player)
    {
        // TODO: Check if in boost state (state 10 in Celeste)
        // if (player.StateMachine.State != 10)
        //     return;
        
        // TODO: Stop summit launch
        // player.StopSummitLaunch();
    }
}
