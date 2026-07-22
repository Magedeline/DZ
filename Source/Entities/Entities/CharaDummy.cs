using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities;

public class CharaDummy : Entity
{
    public Sprite Sprite;

    public PlayerHair Hair;

    public global::Celeste.Mod.DZ.CharaAutoAnimator AutoAnimator;

    public float Float;

    public bool AutoAnimateEnabled
    {
        get => AutoAnimator.Enabled;
        set => AutoAnimator.Enabled = value;
    }

    public SineWave Wave;

    public VertexLight Light;

    public float FloatSpeed = 120f;

    public float FloatAccel = 240f;

    public float Floatness = 2f;

    public Vector2 floatNormal = new Vector2(0f, 1f);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CharaDummy(Vector2 position)
        : base(position)
    {
        base.Collider = new Hitbox(6f, 6f, -3f, -7f);
        Sprite = GFX.SpriteBank.Create("chara");
        Sprite.Play("fallSlow");
        Sprite.Scale.X = -1f;
        Add(Sprite);
        Add(AutoAnimator = new global::Celeste.Mod.DZ.CharaAutoAnimator());
        Sprite.OnFrameChange = [MethodImpl(MethodImplOptions.NoInlining)] (string anim) =>
        {
            int currentAnimationFrame = Sprite.CurrentAnimationFrame;
            if ((anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runSlow" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runFast" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)))
            {
                Audio.Play("event:/char/badeline/footstep", Position);
            }
        };
        Add(Wave = new SineWave(0.25f, 0f));
        Wave.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (float f) =>
        {
            Sprite.Position = floatNormal * f * Floatness;
        };
        Add(Light = new VertexLight(new Vector2(0f, -8f), Color.PaleVioletRed, 1f, 20, 60));
    }

    public void Appear(Level level, bool silent = false)
    {
        if (!silent)
        {
            Audio.Play("event:/char/badeline/appear", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }
        level.Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
        level.Particles.Emit(CharaChaser.P_Vanish, 12, base.Center, Vector2.One * 6f);
    }

    public void Vanish()
    {
        Audio.Play("event:/char/badeline/disappear", Position);
        Shockwave();
        SceneAs<Level>().Particles.Emit(CharaChaser.P_Vanish, 12, base.Center, Vector2.One * 6f);
        RemoveSelf();
    }

    private void Shockwave()
    {
        SceneAs<Level>().Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
    }

    public IEnumerator FloatTo(Vector2 target, int? turnAtEndTo = null, bool faceDirection = true, bool fadeLight = false, bool quickEnd = false)
    {
        Sprite.Play("fallSlow");
        if (faceDirection && Math.Sign(target.X - X) != 0)
        {
            Sprite.Scale.X = Math.Sign(target.X - X);
        }
        Vector2 vector = (target - Position).SafeNormalize();
        Vector2 perp = new Vector2(0f - vector.Y, vector.X);
        float speed = 0f;
        while (Position != target)
        {
            speed = Calc.Approach(speed, FloatSpeed, FloatAccel * Engine.DeltaTime);
            Position = Calc.Approach(Position, target, speed * Engine.DeltaTime);
            Floatness = Calc.Approach(Floatness, 4f, 8f * Engine.DeltaTime);
            floatNormal = Calc.Approach(floatNormal, perp, Engine.DeltaTime * 12f);
            if (fadeLight)
            {
                Light.Alpha = Calc.Approach(Light.Alpha, 0f, Engine.DeltaTime * 2f);
            }
            yield return null;
        }
        if (quickEnd)
        {
            Floatness = 2f;
        }
        else
        {
            while (Floatness != 2f)
            {
                Floatness = Calc.Approach(Floatness, 2f, 8f * Engine.DeltaTime);
                yield return null;
            }
        }
        if (turnAtEndTo.HasValue)
        {
            Sprite.Scale.X = turnAtEndTo.Value;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IEnumerator WalkTo(float x, float speed = 64f)
    {
        Floatness = 0f;
        Sprite.Play("walk");
        if (Math.Sign(x - X) != 0)
        {
            Sprite.Scale.X = Math.Sign(x - X);
        }
        while (X != x)
        {
            X = Calc.Approach(X, x, Engine.DeltaTime * speed);
            yield return null;
        }
        Sprite.Play("idle");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IEnumerator SmashBlock(Vector2 target)
    {
        SceneAs<Level>().Displacement.AddBurst(Position, 0.5f, 24f, 96f);
        Sprite.Play("dreamDashLoop");
        Vector2 from = Position;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime * 6f)
        {
            Position = from + (target - from) * Ease.CubeOut(p);
            yield return null;
        }
        Scene.Entities.FindFirst<DashBlock>().Break(Position, new Vector2(0f, -1f), false, true);
        Sprite.Play("idle");
        for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
        {
            Position = target + (from - target) * Ease.CubeOut(p);
            yield return null;
        }
        Sprite.Play("fallSlow");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        if (Hair != null && Sprite.Scale.X != 0f)
        {
            Hair.Facing = (Facings)Math.Sign(Sprite.Scale.X);
        }
        base.Update();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        Vector2 renderPosition = Sprite.RenderPosition;
        Sprite.RenderPosition = Sprite.RenderPosition.Floor();
        base.Render();
        Sprite.RenderPosition = renderPosition;
    }
}
