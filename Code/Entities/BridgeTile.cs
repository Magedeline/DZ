using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

public class BridgeTile : JumpThru
{
    private List<Image> images;

    private Vector2 shakeOffset;

    private float shakeTimer;

    private float speedY;

    private float colorLerp;

    public bool Fallen { get; private set; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public BridgeTile(Vector2 position, Rectangle tileSize)
        : base(position, tileSize.Width, safe: false)
    {
        images = new List<Image>();
        if (tileSize.Width == 16)
        {
            int num = 24;
            int num2 = 0;
            while (num2 < tileSize.Height)
            {
                Image image;
                Add(image = new Image(GFX.Game["scenery/bridge"].GetSubtexture(tileSize.X, num2, tileSize.Width, num)));
                image.Origin = new Vector2(image.Width / 2f, 0f);
                image.X = image.Width / 2f;
                image.Y = num2 - 8;
                images.Add(image);
                num2 += num;
                num = 12;
            }
        }
        else
        {
            Image image2;
            Add(image2 = new Image(GFX.Game["scenery/bridge"].GetSubtexture(tileSize)));
            image2.Origin = new Vector2(image2.Width / 2f, 0f);
            image2.X = image2.Width / 2f;
            image2.Y = -8f;
            images.Add(image2);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        bool flag = images[0].Width == 16f;
        if (!Fallen)
        {
            return;
        }
        if (shakeTimer > 0f)
        {
            shakeTimer -= Engine.DeltaTime;
            if (base.Scene.OnInterval(0.02f))
            {
                shakeOffset = Calc.Random.ShakeVector();
            }
            if (shakeTimer <= 0f)
            {
                Collidable = false;
                SceneAs<Level>().Shake(0.1f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                if (flag)
                {
                    Audio.Play("event:/game/00_prologue/bridge_support_break", Position);
                    foreach (Image image in images)
                    {
                        if (image.RenderPosition.Y > base.Y + 4f)
                        {
                            Dust.Burst(image.RenderPosition, -MathF.PI / 2f, 8, null);
                        }
                    }
                }
            }
            images[0].Position = new Vector2(images[0].Width / 2f, -8f) + shakeOffset;
            return;
        }
        colorLerp = Calc.Approach(colorLerp, 1f, 10f * Engine.DeltaTime);
        images[0].Color = Color.Lerp(Color.White, Color.Gray, colorLerp);
        shakeOffset = Vector2.Zero;
        if (flag)
        {
            int num = 0;
            foreach (Image image2 in images)
            {
                image2.Rotation -= (float)((num % 2 != 0) ? 1 : (-1)) * Engine.DeltaTime * (float)num * 2f;
                image2.Y += (float)num * Engine.DeltaTime * 16f;
                num++;
            }
            speedY = Calc.Approach(speedY, 120f, 600f * Engine.DeltaTime);
        }
        else
        {
            speedY = Calc.Approach(speedY, 200f, 900f * Engine.DeltaTime);
        }
        MoveV(speedY * Engine.DeltaTime);
        if (base.Top > 220f)
        {
            RemoveSelf();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Fall(float timer = 0.2f)
    {
        if (!Fallen)
        {
            Fallen = true;
            shakeTimer = timer;
        }
    }
}
