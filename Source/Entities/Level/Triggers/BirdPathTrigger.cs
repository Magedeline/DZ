using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that activates a BirdPath entity when entered.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class BirdPathTrigger : CelesteTrigger
{
    private Entity? bird;
    private bool triggered;

    public BirdPathTrigger(Vector2 position, int width, int height) : base(position, width, height)
    {
    }

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        // TODO: Find BirdPath entity in scene
        // bird = Scene.FindComponentOfType<BirdPath>()?.Entity;
        if (bird == null)
        {
            Scene?.Entities.Remove(this);
        }
        else
        {
            // bird.GetComponent<BirdPath>()?.WaitForTrigger();
        }
    }

    public override void OnEnter(PlayerController player)
    {
        if (triggered)
            return;
        // TODO: bird.GetComponent<BirdPath>()?.Trigger();
        triggered = true;
    }
}
