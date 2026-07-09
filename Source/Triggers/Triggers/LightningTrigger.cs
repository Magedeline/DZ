#nullable disable

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Entities;

[CustomEntity("DZ/LightningTrigger")]
[Tracked(false)]
internal class LightningTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    private bool triggered;
    private bool pendingRemove;
    private Level level;

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        if (level.Session.GetFlag("ch20_lightning_trigger_1"))
        {
            pendingRemove = true;
        }
    }

    public override void Update()
    {
        base.Update();
        if (pendingRemove) { RemoveSelf(); return; }
    }

    public override void OnEnter(global::Celeste.Player player)
    {
        if (triggered)
        {
            return;
        }
        base.OnEnter(player);
        triggered = true;
        level.Session.SetFlag("ch20_lightning_trigger_1");
        Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
        Add(new Coroutine(StrikeRoutine(player)));
    }

    private IEnumerator StrikeRoutine(global::Celeste.Player player)
    {
        // Ensure the vanilla LightningRenderer is present so the ambient
        // lightning visual can be drawn.
        LightningRenderer renderer = level.Tracker.GetEntity<LightningRenderer>();
        if (renderer == null)
        {
            renderer = new LightningRenderer();
            level.Add(renderer);
        }

        // Add a visual-only Lightning block for the renderer to draw.
        Vector2 camPos = level.Camera.Position;
        VisualLightning visual = new VisualLightning(camPos, 160, 160);
        level.Add(visual);

        // Core impact effects.
        level.Flash(Color.White);
        level.Shake();

        // The actual lightning bolts.
        level.Add(new LightningStrike(new Vector2(player.X + 60f, level.Bounds.Bottom - 180), 10, 200f));
        level.Add(new LightningStrike(new Vector2(player.X + 220f, level.Bounds.Bottom - 180), 40, 200f, 0.25f));

        // Wait one frame for the renderer/visual to be added, then run the
        // vanilla pulse/bloom/glitch routine that drives the background flash.
        yield return null;
        yield return Lightning.PulseRoutine(level);

        // Clean up the visual-only helper; leave the renderer in case other
        // vanilla Lightning entities are present in the level.
        visual.RemoveSelf();
    }
}

