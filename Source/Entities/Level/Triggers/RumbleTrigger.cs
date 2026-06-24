using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that creates a rumble effect and can break crumble walls.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class RumbleTrigger : CelesteTrigger
{
    private bool manualTrigger;
    private bool started;
    private bool persistent;
    private string id;
    private float rumble;
    private float left;
    private float right;
    private List<Entity> decals = new();
    private List<Entity> crumbles = new();

    public RumbleTrigger(Vector2 position, int width, int height, bool manualTrigger = false, bool persistent = false, string id = "", Vector2[]? nodes = null) : base(position, width, height)
    {
        this.manualTrigger = manualTrigger;
        this.persistent = persistent;
        this.id = id;
        
        if (nodes != null && nodes.Length >= 2)
        {
            left = Math.Min(nodes[0].X, nodes[1].X);
            right = Math.Max(nodes[0].X, nodes[1].X);
        }
    }

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        
        bool flag = false;
        // if (persistent && Session.GetFlag(id))
        //     flag = true;
        
        // Find crumble walls in range
        // foreach (var crumble in Scene.FindComponentsOfType<CrumbleWallOnRumble>())
        // {
        //     if (crumble.Entity.Position.X >= left && crumble.Entity.Position.X <= right)
        //     {
        //         if (flag)
        //             crumble.Entity.RemoveFromScene();
        //         else
        //             crumbles.Add(crumble.Entity);
        //     }
        // }
        
        if (!flag)
        {
            // Find crack decals in range
            // foreach (var decal in Scene.Entities.Where(e => e.Name == "Decal"))
            // {
            //     // if (decal.IsCrack && decal.Position.X >= left && decal.Position.X <= right)
            //     // {
            //     //     decal.Visible = false;
            //     //     decals.Add(decal);
            //     // }
            // }
            
            // Random sort
            crumbles = crumbles.OrderBy(_ => Nez.Random.NextInt(int.MaxValue)).ToList();
        }
        
        if (flag)
            Destroy();
    }

    public override void OnEnter(PlayerController player)
    {
        if (manualTrigger)
            return;
        Invoke();
    }

    private void Invoke(float delay = 0f)
    {
        if (started)
            return;
        started = true;
        
        // if (persistent)
        //     Session.SetFlag(id);
        
        AddComponent(new CoroutineComponent(RumbleRoutine(delay)));
    }

    private IEnumerator RumbleRoutine(float delay)
    {
        yield return delay;
        
        rumble = 1f;
        // TODO: play sound: event:/new_content/game/10_farewell/quake_onset
        // TODO: rumble medium medium
        
        foreach (var decal in decals)
        {
            // decal.Visible = true; // TODO: Visible not available in Nez.Entity
        }
        
        foreach (var crumble in crumbles)
        {
            // crumble.GetComponent<CrumbleWallOnRumble>()?.Break();
            yield return 0.05f;
        }
    }

    public override void Update()
    {
        base.Update();
        rumble = MathHelper.Lerp(rumble, 0f, Time.DeltaTime * 0.7f);
    }

    public static void ManuallyTrigger(float x, float delay)
    {
        // foreach (var trigger in Core.Scene.FindComponentsOfType<RumbleTrigger>())
        // {
        //     if (trigger.manualTrigger && x >= trigger.left && x <= trigger.right)
        //         trigger.Invoke(delay);
        // }
    }
}
