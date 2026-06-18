using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// Controller entity that manages the barrier break visual sequence.
/// Use this to easily trigger the barrier break effects from cutscenes.
/// </summary>
[Tracked]
public class BarrierBreakController : Entity
{
    private BarrierBreakEffect barrierEffect;
    private Level level;
    private Vector2 targetPosition;
    private bool isActive = false;

    public BarrierBreakController(Vector2 position) : base(position)
    {
        targetPosition = position;
        base.Depth = -1000002;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = scene as Level;
        
        // Create the barrier effect entity
        barrierEffect = new BarrierBreakEffect(targetPosition);
        Scene.Add(barrierEffect);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        barrierEffect?.RemoveSelf();
    }

    /// <summary>
    /// Execute the complete barrier break sequence with all three phases.
    /// This is the main method to call from CS20_Saved.Trigger13_ReleaseSouls
    /// </summary>
    public IEnumerator ExecuteBarrierBreakSequence()
    {
        isActive = true;
        
        // Part 1: Breaking the 4th wall 3 times
        for (int hit = 0; hit < 3; hit++)
        {
            Audio.Play("event:/Mods/pusheen/new_content/music/lvl20/cinematic/break_fourth_wall_part1");
            yield return barrierEffect.PlayCrackEffect();
            
            // Additional screen effects per hit
            if (level != null)
            {
                level.Shake(1f + hit * 0.5f);
            }
        }
        
        yield return 0.5f;
        
        // Part 2: The wall opens and crumbles down sideways
        Audio.Play("event:/Mods/pusheen/new_content/music/lvl20/cinematic/break_fourth_wall_part2");
        
        if (level != null)
        {
            level.Flash(Color.White, true);
            level.Shake(3f);
        }
        
        yield return barrierEffect.PlayShatterEffect();
        
        // Part 3: Final destruction and text
        Audio.Play("event:/Mods/pusheen/new_content/music/lvl20/cinematic/break_fourth_wall_part3");
        
        if (level != null)
        {
            level.Flash(Color.White * 0.5f, false);
            level.Shake(1f);
        }
        
        yield return barrierEffect.PlayDestroyedEffect();
        
        isActive = false;
    }

    /// <summary>
    /// Play only Part 1 (crack effect) - for manual control
    /// </summary>
    public IEnumerator PlayPart1_Crack()
    {
        Audio.Play("event:/Mods/pusheen/new_content/music/lvl20/cinematic/break_fourth_wall_part1");
        yield return barrierEffect.PlayCrackEffect();
    }

    /// <summary>
    /// Play only Part 2 (shatter effect) - for manual control
    /// </summary>
    public IEnumerator PlayPart2_Shatter()
    {
        Audio.Play("event:/Mods/pusheen/new_content/music/lvl20/cinematic/break_fourth_wall_part2");
        yield return barrierEffect.PlayShatterEffect();
    }

    /// <summary>
    /// Play only Part 3 (destroyed effect) - for manual control
    /// </summary>
    public IEnumerator PlayPart3_Destroyed()
    {
        Audio.Play("event:/Mods/pusheen/new_content/music/lvl20/cinematic/break_fourth_wall_part3");
        yield return barrierEffect.PlayDestroyedEffect();
    }

    public bool IsActive => isActive;
}