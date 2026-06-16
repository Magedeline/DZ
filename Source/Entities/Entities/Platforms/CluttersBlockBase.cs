using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ;

public class CustomClutterBlockBase : Solid
{
    private static readonly Color enabledColor = Color.Black * 0.7f;

    private static readonly Color disabledColor = Color.Black * 0.3f;

    public string BlockColor;

    private Color color;

    private bool enabled;

    private LightOcclude occluder;

    private bool renderShadow;

    public CustomClutterBlockBase(Vector2 position, int width, int height, bool enabled, string blockColor, int surfaceSoundIndex, bool renderShadow)
        : base(position, (float)width, (float)height, true)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0045: Unknown result type (might be due to invalid IL or missing references)
        //IL_003e: Unknown result type (might be due to invalid IL or missing references)
        //IL_004a: Unknown result type (might be due to invalid IL or missing references)
        //IL_005d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0062: Unknown result type (might be due to invalid IL or missing references)
        //IL_0064: Expected O, but got Unknown
        //IL_0069: Expected O, but got Unknown
        base.EnableAssistModeChecks = false;
        BlockColor = blockColor;
        ((Entity)this).Depth = 8999;
        this.enabled = enabled;
        this.renderShadow = renderShadow;
        color = (enabled ? enabledColor : disabledColor);
        if (enabled)
        {
            LightOcclude val = new LightOcclude(1f);
            LightOcclude val2 = val;
            occluder = val;
            ((Entity)this).Add((Component)(object)val2);
        }
        else
        {
            ((Entity)this).Collidable = false;
        }
        ((Platform)this).SurfaceSoundIndex = surfaceSoundIndex;
    }

    public void Deactivate()
    {
        //IL_0009: Unknown result type (might be due to invalid IL or missing references)
        //IL_000e: Unknown result type (might be due to invalid IL or missing references)
        ((Entity)this).Collidable = false;
        color = disabledColor;
        enabled = false;
        if (occluder != null)
        {
            ((Entity)this).Remove((Component)(object)occluder);
            occluder = null;
        }
    }

    public void Activate()
    {
        //IL_0009: Unknown result type (might be due to invalid IL or missing references)
        //IL_000e: Unknown result type (might be due to invalid IL or missing references)
        //IL_002f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0034: Unknown result type (might be due to invalid IL or missing references)
        //IL_0036: Expected O, but got Unknown
        //IL_003b: Expected O, but got Unknown
        ((Entity)this).Collidable = true;
        color = enabledColor;
        enabled = true;
        if (occluder == null)
        {
            LightOcclude val = new LightOcclude(1f);
            LightOcclude val2 = val;
            occluder = val;
            ((Entity)this).Add((Component)(object)val2);
        }
    }

    public override void Render()
    {
        //IL_0033: Unknown result type (might be due to invalid IL or missing references)
        if (renderShadow)
        {
            Draw.Rect(((Entity)this).X, ((Entity)this).Y, ((Entity)this).Width, ((Entity)this).Height + (float)(enabled ? 2 : 0), color);
        }
    }
}
