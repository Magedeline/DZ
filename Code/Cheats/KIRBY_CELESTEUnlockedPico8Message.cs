using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

/// <summary>
/// Displays a message when Pico8 Classic is unlocked for Ingeste.
/// Similar to the vanilla Pico8 unlock message but customized for the mod.
/// </summary>
public class KIRBY_CELESTEUnlockedPico8Message : Entity
{
    private float alpha;
    private string text;
    private bool waitForKeyPress;
    private float timer;
    private Action callback;

    public KIRBY_CELESTEUnlockedPico8Message(Action callback = null)
    {
        this.callback = callback;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        base.Tag = (int)Tags.HUD | (int)Tags.PauseUpdate;
        text = ActiveFont.FontSize.AutoNewline(Dialog.Clean("PICO8_UNLOCKED"), 900);
        base.Depth = -10000;
        Add(new Coroutine(Routine()));
    }

    private IEnumerator Routine()
    {
        Level level = Scene as Level;
        level.PauseLock = true;
        level.Paused = true;
        while ((alpha += Engine.DeltaTime / 0.5f) < 1f)
        {
            yield return null;
        }
        alpha = 1f;
        waitForKeyPress = true;
        while (!Input.MenuConfirm.Pressed)
        {
            yield return null;
        }
        waitForKeyPress = false;
        while ((alpha -= Engine.DeltaTime / 0.5f) > 0f)
        {
            yield return null;
        }
        alpha = 0f;
        level.PauseLock = false;
        level.Paused = false;
        RemoveSelf();
        if (callback != null)
        {
            callback();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        timer += Engine.DeltaTime;
        base.Update();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        float num = Ease.CubeOut(alpha);
        Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * num * 0.8f);
        GFX.Gui["pico8"].DrawJustified(Celeste.TargetCenter + new Vector2(0f, -64f * (1f - num) - 16f), new Vector2(0.5f, 1f), Color.White * num);
        Vector2 position = Celeste.TargetCenter + new Vector2(0f, 64f * (1f - num) + 16f);
        Vector2 vector = ActiveFont.Measure(text);
        ActiveFont.Draw(text, position, new Vector2(0.5f, 0f), Vector2.One, Color.White * num);
        if (waitForKeyPress)
        {
            GFX.Gui["textboxbutton"].DrawCentered(Celeste.TargetCenter + new Vector2(vector.X / 2f + 32f, vector.Y + 48f + (float)((timer % 1f < 0.25f) ? 6 : 0)));
        }
    }
}
