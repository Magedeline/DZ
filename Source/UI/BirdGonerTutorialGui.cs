using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.UI;

public class BirdGonerTutorialGui : Entity
{
    public enum ButtonPrompt
    {
        Dash,
        Jump,
        Grab,
        Talk
    }

    public Entity Entity;

    public bool Open;

    public float Scale;

    private object info;

    private List<object> controls;

    private float controlsWidth;

    private float infoWidth;

    private float infoHeight;

    private float buttonPadding;

    private Color bgColor;

    private Color lineColor;

    private Color textColor;

    private static Dictionary<VirtualButton, ButtonPrompt> controlsButtonsToID = new Dictionary<VirtualButton, ButtonPrompt>();

    private static Dictionary<ButtonPrompt, VirtualButton> controlsIDToButton = new Dictionary<ButtonPrompt, VirtualButton>();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public BirdGonerTutorialGui(Entity entity, Vector2 position, object info, params object[] controls)
    {
        for (int i = 0; i < controls.Length; i++)
        {
            if (controls[i] is VirtualButton virtualButton)
            {
                if (!controlsButtonsToID.TryGetValue(virtualButton, out var value))
                {
                    value = (ButtonPrompt)(-(controlsButtonsToID.Count + 1));
                    controlsButtonsToID[virtualButton] = value;
                    controlsIDToButton[value] = virtualButton;
                }
                controls[i] = value;
            }
        }
        orig_ctor(entity, position, info, controls);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void UpdateControlsSize()
    {
        controlsWidth = 0f;
        foreach (object control in controls)
        {
            if (control is ButtonPrompt)
            {
                controlsWidth += (float)Input.GuiButton(ButtonPromptToVirtualButton((ButtonPrompt)control), null).Width + buttonPadding * 2f;
            }
            else if (control is Vector2)
            {
                controlsWidth += (float)Input.GuiDirection((Vector2)control).Width + buttonPadding * 2f;
            }
            else if (control is string)
            {
                controlsWidth += ActiveFont.Measure(control.ToString()).X;
            }
            else if (control is MTexture)
            {
                controlsWidth += ((MTexture)control).Width;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        UpdateControlsSize();
        Scale = Calc.Approach(Scale, Open ? 1 : 0, Engine.RawDeltaTime * 8f);
        base.Update();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        Level level = base.Scene as Level;
        if (level.FrozenOrPaused || level.RetryPlayerCorpse != null || Scale <= 0f)
        {
            return;
        }
        Camera camera = SceneAs<Level>().Camera;
        Vector2 vector = Entity.Position + Position - camera.Position.Floor();
        if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
        {
            vector.X = 320f - vector.X;
        }
        vector.X *= 6f;
        vector.Y *= 6f;
        float lineHeight = ActiveFont.LineHeight;
        float num = (Math.Max(controlsWidth, infoWidth) + 64f) * Scale;
        float num2 = infoHeight + lineHeight + 32f;
        float num3 = vector.X - num / 2f;
        float num4 = vector.Y - num2 - 32f;
        Draw.Rect(num3 - 6f, num4 - 6f, num + 12f, num2 + 12f, lineColor);
        Draw.Rect(num3, num4, num, num2, bgColor);
        for (int i = 0; i <= 36; i++)
        {
            float num5 = (float)(73 - i * 2) * Scale;
            Draw.Rect(vector.X - num5 / 2f, num4 + num2 + (float)i, num5, 1f, lineColor);
            if (num5 > 12f)
            {
                Draw.Rect(vector.X - num5 / 2f + 6f, num4 + num2 + (float)i, num5 - 12f, 1f, bgColor);
            }
        }
        if (!(num > 3f))
        {
            return;
        }
        Vector2 vector2 = new Vector2(vector.X, num4 + 16f);
        if (info is string)
        {
            ActiveFont.Draw((string)info, vector2, new Vector2(0.5f, 0f), new Vector2(Scale, 1f), textColor);
        }
        else if (info is MTexture)
        {
            ((MTexture)info).DrawJustified(vector2, new Vector2(0.5f, 0f), Color.White, new Vector2(Scale, 1f));
        }
        vector2.Y += infoHeight + lineHeight * 0.5f;
        Vector2 vector3 = new Vector2((0f - controlsWidth) / 2f, 0f);
        foreach (object control in controls)
        {
            if (control is ButtonPrompt)
            {
                MTexture mTexture = Input.GuiButton(ButtonPromptToVirtualButton((ButtonPrompt)control), null);
                vector3.X += buttonPadding;
                mTexture.Draw(vector2, new Vector2(0f - vector3.X, mTexture.Height / 2), Color.White, new Vector2(Scale, 1f));
                vector3.X += (float)mTexture.Width + buttonPadding;
            }
            else if (control is Vector2 direction)
            {
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                {
                    direction.X = 0f - direction.X;
                }
                MTexture mTexture2 = Input.GuiDirection(direction);
                vector3.X += buttonPadding;
                mTexture2.Draw(vector2, new Vector2(0f - vector3.X, mTexture2.Height / 2), Color.White, new Vector2(Scale, 1f));
                vector3.X += (float)mTexture2.Width + buttonPadding;
            }
            else if (control is string)
            {
                string? text = control.ToString();
                float x = ActiveFont.Measure(text).X;
                ActiveFont.Draw(text, vector2 + new Vector2(1f, 2f), new Vector2((0f - vector3.X) / x, 0.5f), new Vector2(Scale, 1f), textColor);
                ActiveFont.Draw(text, vector2 + new Vector2(1f, -2f), new Vector2((0f - vector3.X) / x, 0.5f), new Vector2(Scale, 1f), Color.White);
                vector3.X += x + 1f;
            }
            else if (control is MTexture)
            {
                MTexture mTexture3 = (MTexture)control;
                mTexture3.Draw(vector2, new Vector2(0f - vector3.X, mTexture3.Height / 2), Color.White, new Vector2(Scale, 1f));
                vector3.X += mTexture3.Width;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static VirtualButton ButtonPromptToVirtualButton(ButtonPrompt prompt)
    {
        if (controlsIDToButton.TryGetValue(prompt, out var value))
        {
            return value;
        }
        return orig_ButtonPromptToVirtualButton(prompt);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void orig_ctor(Entity entity, Vector2 position, object info, params object[] controls)
    {
        buttonPadding = 8f;
        bgColor = Calc.HexToColor("061526");
        lineColor = new Color(1f, 1f, 1f);
        textColor = Calc.HexToColor("6179e2");
        AddTag(Tags.HUD);
        Entity = entity;
        Position = position;
        this.info = info;
        this.controls = new List<object>(controls);
        if (info is string)
        {
            infoWidth = ActiveFont.Measure((string)info).X;
            infoHeight = ActiveFont.LineHeight;
        }
        else if (info is MTexture)
        {
            infoWidth = ((MTexture)info).Width;
            infoHeight = ((MTexture)info).Height;
        }
        UpdateControlsSize();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static VirtualButton orig_ButtonPromptToVirtualButton(ButtonPrompt prompt)
    {
        return prompt switch
        {
            ButtonPrompt.Dash => Input.Dash, 
            ButtonPrompt.Jump => Input.Jump, 
            ButtonPrompt.Grab => Input.Grab, 
            ButtonPrompt.Talk => Input.Talk, 
            _ => Input.Jump, 
        };
    }
}
