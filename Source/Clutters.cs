using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaggyHelper;

public class CustomClutter : Entity
{
    public string BlockColor;

    public Image Image;

    public HashSet<CustomClutter> HasBelow = new HashSet<CustomClutter>();

    public List<CustomClutter> Below = new List<CustomClutter>();

    public List<CustomClutter> Above = new List<CustomClutter>();

    public bool OnTheGround;

    public bool TopSideOpen;

    public bool LeftSideOpen;

    public bool RightSideOpen;

    private float floatTarget;

    private float floatDelay;

    private float floatTimer;

    private int makeInvis;

    private float WaveTarget => 0f - ((float)Math.Sin((float)((int)base.Position.X / 16) * 0.25f + floatTimer * 2f) + 1f) / 2f - 1f;

    public CustomClutter(Vector2 position, MTexture texture, string color)
        : base(position)
    {
        //IL_0022: Unknown result type (might be due to invalid IL or missing references)
        //IL_0034: Unknown result type (might be due to invalid IL or missing references)
        //IL_0039: Unknown result type (might be due to invalid IL or missing references)
        //IL_003b: Expected O, but got Unknown
        //IL_0040: Expected O, but got Unknown
        //IL_0060: Unknown result type (might be due to invalid IL or missing references)
        //IL_006a: Expected O, but got Unknown
        BlockColor = color;
        Image val = new Image(texture);
        Image val2 = val;
        Image = val;
        ((Entity)this).Add((Component)(object)val2);
        ((Entity)this).Collider = (Collider)new Hitbox((float)texture.Width, (float)texture.Height, 0f, 0f);
        ((Entity)this).Depth = -9998;
    }

    public void WeightDown()
    {
        foreach (CustomClutter item in Below)
        {
            item.WeightDown();
        }
        floatTarget = 0f;
        floatDelay = 0.1f;
    }

    public override void Update()
    {
        ((Entity)this).Update();
        if (makeInvis > 0)
        {
            makeInvis--;
            if (makeInvis == 0)
            {
                base.Visible = false;
            }
        }
        if (OnTheGround)
        {
            return;
        }
        if (floatDelay <= 0f)
        {
            Player entity = ((Entity)this).Scene.Tracker.GetEntity<Player>();
            if (entity != null && ((TopSideOpen && ((Entity)entity).Right > ((Entity)this).Left && ((Entity)entity).Left < ((Entity)this).Right && ((Entity)entity).Bottom >= ((Entity)this).Top - 1f && ((Entity)entity).Bottom <= ((Entity)this).Top + 4f) | (entity.StateMachine.State == 1 && LeftSideOpen && ((Entity)entity).Right >= ((Entity)this).Left - 1f && ((Entity)entity).Right < ((Entity)this).Left + 4f && ((Entity)entity).Bottom > ((Entity)this).Top && ((Entity)entity).Top < ((Entity)this).Bottom) | (entity.StateMachine.State == 1 && RightSideOpen && ((Entity)entity).Left <= ((Entity)this).Right + 1f && ((Entity)entity).Left > ((Entity)this).Right - 4f && ((Entity)entity).Bottom > ((Entity)this).Top && ((Entity)entity).Top < ((Entity)this).Bottom)))
            {
                WeightDown();
            }
        }
        floatTimer += Engine.DeltaTime;
        floatDelay -= Engine.DeltaTime;
        if (floatDelay <= 0f)
        {
            floatTarget = Calc.Approach(floatTarget, WaveTarget, Engine.DeltaTime * 4f);
        }
        ((GraphicsComponent)Image).Y = floatTarget;
    }

    public void Absorb(ClutterAbsorbEffect effect)
    {
        //IL_0003: Unknown result type (might be due to invalid IL or missing references)
        //IL_0031: Unknown result type (might be due to invalid IL or missing references)
        //IL_0036: Unknown result type (might be due to invalid IL or missing references)
        effect.FlyClutter(base.Position + new Vector2(Image.Width * 0.5f, Image.Height * 0.5f + floatTarget), Image.Texture, true, Calc.NextFloat(Calc.Random, 0.5f));
        makeInvis = 2;
    }
}
