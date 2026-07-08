using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using DZ.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Trigger that spawns or removes AngryOshiro.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class OshiroTrigger : CelesteTrigger
{
    public bool State;

    public OshiroTrigger(Vector2 position, int width, int height, bool state = true) : base(position, width, height)
    {
        State = state;
    }

    public override void OnEnter(PlayerController player)
    {
        if (State)
        {
            // TODO: Spawn AngryOshiro at level bounds
            // var levelBounds = new Rectangle(0, 0, 320, 180); // placeholder
            // var spawnPos = new Vector2(levelBounds.Left - 32, levelBounds.Top + levelBounds.Height / 2);
            // Scene.Add(new AngryOshiro(spawnPos, false));

            Destroy();
        }
        else
        {
            // TODO: Find and remove AngryOshiro
            // var oshiro = Scene.FindComponentOfType<AngryOshiro>();
            // oshiro?.Leave();

            Destroy();
        }
    }
}
