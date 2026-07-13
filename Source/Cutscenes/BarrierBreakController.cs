using System;
using System.Collections;
using System.Collections.Generic;
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

    private MTexture breakTexture;
    private MTexture endTexture;
    private List<MTexture> shatterTextures;
    private bool showEnd = false;
    private bool waitingForEndConfirm;
    private bool endConfirmed;
    private float fadeAlpha;

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

        // Load the barrier break graphics
        breakTexture = GFX.Game["cutscenes/DZ/barrierbreak/break00"];
        endTexture = GFX.Game["cutscenes/DZ/barrierbreak/end"];
        shatterTextures = GFX.Game.GetAtlasSubtextures("cutscenes/DZ/barrierbreak/shatter");
    }

    public override void Update()
    {
        base.Update();

        if (waitingForEndConfirm && !endConfirmed && Input.MenuConfirm.Pressed)
        {
            endConfirmed = true;
        }
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
            yield return barrierEffect.PlayCrackEffect();
            
            // Additional screen effects per hit
            if (level != null)
            {
                level.Shake(1f + hit * 0.5f);
            }
        }
        
        yield return 0.5f;
        
        if (level != null)
        {
            level.Flash(Color.White, true);
            level.Shake(3f);
        }
        
        yield return barrierEffect.PlayShatterEffect();
        
        if (level != null)
        {
            level.Flash(Color.White * 0.5f, false);
            level.Shake(1f);
        }
        
        yield return barrierEffect.PlayDestroyedEffect();
        
        showEnd = true;
        waitingForEndConfirm = true;

        // Wait for the player to press the confirm button
        while (!endConfirmed)
        {
            yield return null;
        }

        // Fade the screen to black
        float fadeDuration = 1f;
        for (float t = 0f; t < fadeDuration; t += Engine.DeltaTime)
        {
            fadeAlpha = t / fadeDuration;
            yield return null;
        }
        fadeAlpha = 1f;

        yield return 0.5f;
        isActive = false;
    }

    public override void Render()
    {
        base.Render();

        Vector2 renderPos = Position;

        // Draw the initial break frame
        if (breakTexture != null)
        {
            breakTexture.DrawCentered(renderPos, Color.White);
        }

        // Position shatter_A to the left and shatter_B to the right
        if (shatterTextures != null && shatterTextures.Count > 0 && isActive)
        {
            if (shatterTextures.Count > 0)
            {
                shatterTextures[0].DrawCentered(renderPos + new Vector2(-100f, 0f), Color.White);
            }
            if (shatterTextures.Count > 1)
            {
                shatterTextures[1].DrawCentered(renderPos + new Vector2(100f, 0f), Color.White);
            }
        }

        // Show end.png instantly after the sequence completes
        if (endTexture != null && showEnd)
        {
            endTexture.DrawCentered(renderPos, Color.White);
        }

        // Black fade-out overlay
        if (fadeAlpha > 0f && level != null)
        {
            Draw.Rect(level.Camera.X - 2000f, level.Camera.Y - 2000f, 4000f, 4000f, Color.Black * fadeAlpha);
        }
    }
}