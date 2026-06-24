using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that creates a glitch effect on the moon/background.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class MoonGlitchBackgroundTrigger : CelesteTrigger
{
    public enum Duration
    {
        Short,
        Medium,
        Long
    }

    private Duration duration;
    private bool triggered;
    private bool stayOn;
    private bool running;
    private bool doGlitch;

    public MoonGlitchBackgroundTrigger(Vector2 position, int width, int height, Duration duration, bool stay = false, bool glitch = true) : base(position, width, height)
    {
        this.duration = duration;
        stayOn = stay;
        doGlitch = glitch;
    }

    public override void OnEnter(PlayerController player) => Invoke();

    public void Invoke()
    {
        if (triggered)
            return;
        triggered = true;
        
        if (doGlitch)
        {
            AddComponent(new CoroutineComponent(InternalGlitchRoutine()));
        }
        else if (!stayOn)
        {
            Toggle(false);
        }
    }

    private IEnumerator InternalGlitchRoutine()
    {
        running = true;
        SetTag(1); // Persistent tag
        
        float glitchDuration;
        if (duration == Duration.Short)
        {
            glitchDuration = 0.2f;
            // TODO: rumble strong medium
            // TODO: play sound: event:/new_content/game/10_farewell/glitch_short
        }
        else if (duration == Duration.Medium)
        {
            glitchDuration = 0.5f;
            // TODO: rumble strong medium
            // TODO: play sound: event:/new_content/game/10_farewell/glitch_medium
        }
        else
        {
            glitchDuration = 1.25f;
            // TODO: rumble strong long
            // TODO: play sound: event:/new_content/game/10_farewell/glitch_long
        }
        
        yield return GlitchRoutine(glitchDuration, stayOn);
        
        Tag = 0;
        running = false;
    }

    private static void Toggle(bool on)
    {
        // TODO: Toggle blackhole backdrops
        // foreach (var backdrop in Scene.FindComponentsOfType<Backdrop>().Where(b => b.Name == "blackhole"))
        //     backdrop.ForceVisible = on;
    }

    private static void Fade(float alpha, bool max = false)
    {
        // TODO: Fade blackhole backdrops
        // foreach (var backdrop in Scene.FindComponentsOfType<Backdrop>().Where(b => b.Name == "blackhole"))
        //     backdrop.FadeAlphaMultiplier = max ? Math.Max(backdrop.FadeAlphaMultiplier, alpha) : alpha;
    }

    public static IEnumerator GlitchRoutine(float glitchDuration, bool stayOn)
    {
        Toggle(true);
        
        // TODO: Check if flashes are disabled
        // if (Settings.Instance.DisableFlashes)
        // {
            for (float a = 0f; a < 1f; a += Time.DeltaTime / 0.1f)
            {
                Fade(a, true);
                yield return null;
            }
            Fade(1f);
            yield return glitchDuration;
            
            if (!stayOn)
            {
                for (float a = 0f; a < 1f; a += Time.DeltaTime / 0.1f)
                {
                    Fade(1f - a);
                    yield return null;
                }
                Fade(1f);
            }
        // }
        // else if (glitchDuration > 0.4f)
        // {
        //     Glitch.Value = 0.3f;
        //     yield return 0.2f;
        //     Glitch.Value = 0f;
        //     yield return glitchDuration - 0.4f;
        //     if (!stayOn)
        //         Glitch.Value = 0.3f;
        //     yield return 0.2f;
        //     Glitch.Value = 0f;
        // }
        // else
        // {
        //     Glitch.Value = 0.3f;
        //     yield return glitchDuration;
        //     Glitch.Value = 0f;
        // }
        
        if (!stayOn)
            Toggle(false);
    }

    public override void OnRemovedFromScene()
    {
        if (running)
        {
            // Glitch.Value = 0f;
            Fade(1f);
            if (!stayOn)
                Toggle(false);
        }
        base.OnRemovedFromScene();
    }
}

/// <summary>
/// Helper component to run coroutines
/// </summary>
public class CoroutineComponent : Component, IUpdatable
{
    private IEnumerator routine;

    public CoroutineComponent(IEnumerator routine)
    {
        this.routine = routine;
    }

    public void Update()
    {
        if (!routine.MoveNext())
            Entity.RemoveComponent(this);
    }
}
