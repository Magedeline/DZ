#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/MoonGlitchBackgroundTrigger")]
public class MoonGlitchBackgroundTrigger : Trigger {
    public enum Duration { Short, Medium, Long }

    private Duration duration;
    private bool triggered;
    private bool stayOn;
    private bool running;
    private bool doGlitch;

    public MoonGlitchBackgroundTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        duration = data.Enum("duration", Duration.Short);
        stayOn = data.Bool("stay", false);
        doGlitch = data.Bool("glitch", true);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        Invoke();
    }

    public void Invoke() {
        if (triggered) return;
        triggered = true;
        if (doGlitch)
            Add(new Coroutine(GlitchRoutine()));
        else if (!stayOn)
            Toggle(false);
    }

    private IEnumerator GlitchRoutine() {
        running = true;
        Tag = Tags.Persistent;
        float glitchDuration = duration switch {
            Duration.Short => 0.2f,
            Duration.Medium => 0.5f,
            _ => 1.25f,
        };
        Input.Rumble(RumbleStrength.Strong, duration == Duration.Long ? RumbleLength.Long : RumbleLength.Medium);
        Audio.Play(duration == Duration.Long ? "event:/new_content/game/10_farewell/glitch_long"
            : duration == Duration.Medium ? "event:/new_content/game/10_farewell/glitch_medium"
            : "event:/new_content/game/10_farewell/glitch_short");
        Toggle(true);
        for (float a = 0f; a < 1f; a += Engine.DeltaTime / 0.1f) {
            Fade(a, true);
            yield return null;
        }
        Fade(1f);
        yield return glitchDuration;
        if (!stayOn) {
            for (float a = 0f; a < 1f; a += Engine.DeltaTime / 0.1f) {
                Fade(1f - a);
                yield return null;
            }
            Fade(0f);
            Toggle(false);
        }
        Tag = 0;
        running = false;
    }

    private static void Toggle(bool on) {
        foreach (var backdrop in Engine.Scene.Entities.OfType<Backdrop>()) {
            if (backdrop.Name == "blackhole")
                backdrop.ForceVisible = on;
        }
    }

    private static void Fade(float alpha, bool max = false) {
        foreach (var backdrop in Engine.Scene.Entities.OfType<Backdrop>()) {
            if (backdrop.Name == "blackhole")
                backdrop.FadeAlphaMultiplier = max ? Math.Max(backdrop.FadeAlphaMultiplier, alpha) : alpha;
        }
    }

    public override void Removed(Scene scene) {
        if (running) {
            Fade(0f);
            if (!stayOn)
                Toggle(false);
        }
        base.Removed(scene);
    }
}
