using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace DZ;

[CustomBackdrop("DZ/RainbowBlackholeBG")]
[HotReloadable]
public class RainbowBlackholeBg : Backdrop
{
    public enum Strengths
    {
        Mild,
        Medium,
        High,
        Wild,
        Insane
    }

    private struct StreamParticle
    {
        public int Color;

        public MTexture Texture;

        public float Percent;

        public float Speed;

        public Vector2 Normal;
    }

    private struct Particle
    {
        public int Color;

        public Vector2 Normal;

        public float Percent;
    }

    private struct SpiralDebris
    {
        public int Color;

        public float Percent;

        public float Offset;
    }

    private const string STRENGTH_FLAG = "rainbow_blackhole_strength";

    private const int BG_STEPS = 20;

    private const int STREAM_MIN_COUNT = 30;

    private const int STREAM_MAX_COUNT = 50;

    private const int PARTICLE_MIN_COUNT = 150;

    private const int PARTICLE_MAX_COUNT = 220;

    private const int SPIRAL_MIN_COUNT = 0;

    private const int SPIRAL_MAX_COUNT = 10;

    private const int SPIRAL_SEGMENTS = 12;

    private Color[] colorsMild = new Color[3]
    {
        Calc.HexToColor("6e3199") * 0.8f,
        Calc.HexToColor("851f91") * 0.8f,
        Calc.HexToColor("3026b0") * 0.8f
    };

    private Color[] colorsWild = new Color[3]
    {
        Calc.HexToColor("ca4ca7"),
        Calc.HexToColor("b14cca"),
        Calc.HexToColor("ca4ca7")
    };

    private Color[] colorsRed = new Color[3]
    {
        Calc.HexToColor("ff2a2a"),
        Calc.HexToColor("d40000"),
        Calc.HexToColor("ff5533")
    };

    private Color[] colorsRainbow = new Color[7]
    {
        Calc.HexToColor("ff0000"),
        Calc.HexToColor("ff7f00"),
        Calc.HexToColor("ffff00"),
        Calc.HexToColor("00ff00"),
        Calc.HexToColor("0000ff"),
        Calc.HexToColor("4b0082"),
        Calc.HexToColor("9400d3")
    };

    private Color[] colorsLerp;

    private Color[,] colorsLerpBlack;

    private Color[,] colorsLerpTransparent;

    private const int colorSteps = 20;

    public float Alpha = 1f;

    public bool RainbowMode = false;

    public int RainbowDepth = -99999999;

    public float Scale = 1f;

    public float Direction = 1f;

    public float StrengthMultiplier = 1f;

    public Vector2 CenterOffset;

    public Vector2 OffsetOffset;

    private Strengths strength;

    private readonly Color bgColorInner = Calc.HexToColor("000000");

    private readonly Color bgColorOuterMild = Calc.HexToColor("512a8b");

    private readonly Color bgColorOuterWild = Calc.HexToColor("bd2192");

    private readonly MTexture bgTexture;

    private StreamParticle[] streams = new StreamParticle[50];

    private VertexPositionColorTexture[] streamVerts = new VertexPositionColorTexture[300];

    private Particle[] particles = new Particle[220];

    private SpiralDebris[] spirals = new SpiralDebris[10];

    private VertexPositionColorTexture[] spiralVerts = new VertexPositionColorTexture[720];

    private VirtualRenderTarget buffer;

    private Vector2 center;

    private Vector2 offset;

    private Vector2 shake;

    private float spinTime;

    private bool checkedFlag;

    public int StreamCount
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get
        {
            return (int)MathHelper.Lerp(30f, 50f, Calc.Clamp((StrengthMultiplier - 1f) / 3f, 0f, 1f));
        }
    }

    public int ParticleCount
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get
        {
            return (int)MathHelper.Lerp(150f, 220f, Calc.Clamp((StrengthMultiplier - 1f) / 3f, 0f, 1f));
        }
    }

    public int SpiralCount
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get
        {
            return (int)MathHelper.Lerp(0f, 10f, Calc.Clamp((StrengthMultiplier - 1f) / 3f, 0f, 1f));
        }
    }

    public RainbowBlackholeBg(BinaryPacker.Element data) : this(data.AttrBool("rainbowMode", false))
    {
        Alpha = MathHelper.Clamp(data.AttrFloat("alpha", 1f), 0f, 1f);
        Scale = data.AttrFloat("scale", 1f);
        Direction = data.AttrFloat("direction", 1f);
        string strengthStr = data.Attr("strength", "Mild");
        if (Enum.TryParse<Strengths>(strengthStr, true, out var parsedStrength))
        {
            SetStrength(parsedStrength);
        }
        CenterOffset = new Vector2(data.AttrFloat("centerOffsetX", 0f), data.AttrFloat("centerOffsetY", 0f));
        OffsetOffset = new Vector2(data.AttrFloat("offsetOffsetX", 0f), data.AttrFloat("offsetOffsetY", 0f));
    }

    public RainbowBlackholeBg() : this(false) { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private RainbowBlackholeBg(bool rainbowMode)
    {
        RainbowMode = rainbowMode;
        bgTexture = GFX.Game["objects/temple/portal/portal"];
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("bgs/DZ/19/say_goodbye/blackhole/particle");
        if (atlasSubtextures.Count == 0)
            atlasSubtextures = GFX.Game.GetAtlasSubtextures("bgs/10/blackhole/particle");
        Color[] activeColors = RainbowMode ? colorsRainbow : colorsMild;
        int num = 0;
        for (int i = 0; i < 50; i++)
        {
            if (atlasSubtextures.Count == 0) break;
            MTexture mTexture = (streams[i].Texture = Calc.Random.Choose(atlasSubtextures));
            streams[i].Percent = Calc.Random.NextFloat();
            streams[i].Speed = Calc.Random.Range(0.2f, 0.4f);
            streams[i].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (MathF.PI * 2f), 1f);
            streams[i].Color = Calc.Random.Next(activeColors.Length);
            streamVerts[num].TextureCoordinate = new Vector2(mTexture.LeftUV, mTexture.TopUV);
            streamVerts[num + 1].TextureCoordinate = new Vector2(mTexture.RightUV, mTexture.TopUV);
            streamVerts[num + 2].TextureCoordinate = new Vector2(mTexture.RightUV, mTexture.BottomUV);
            streamVerts[num + 3].TextureCoordinate = new Vector2(mTexture.LeftUV, mTexture.TopUV);
            streamVerts[num + 4].TextureCoordinate = new Vector2(mTexture.RightUV, mTexture.BottomUV);
            streamVerts[num + 5].TextureCoordinate = new Vector2(mTexture.LeftUV, mTexture.BottomUV);
            num += 6;
        }
        int num2 = 0;
        for (int j = 0; j < 10; j++)
        {
            if (atlasSubtextures.Count == 0) break;
            MTexture mTexture2 = (streams[j].Texture = Calc.Random.Choose(atlasSubtextures));
            spirals[j].Percent = Calc.Random.NextFloat();
            spirals[j].Offset = Calc.Random.NextFloat(MathF.PI * 2f);
            spirals[j].Color = Calc.Random.Next(activeColors.Length);
            for (int k = 0; k < 12; k++)
            {
                float x = MathHelper.Lerp(mTexture2.LeftUV, mTexture2.RightUV, (float)k / 12f);
                float x2 = MathHelper.Lerp(mTexture2.LeftUV, mTexture2.RightUV, (float)(k + 1) / 12f);
                spiralVerts[num2].TextureCoordinate = new Vector2(x, mTexture2.TopUV);
                spiralVerts[num2 + 1].TextureCoordinate = new Vector2(x2, mTexture2.TopUV);
                spiralVerts[num2 + 2].TextureCoordinate = new Vector2(x2, mTexture2.BottomUV);
                spiralVerts[num2 + 3].TextureCoordinate = new Vector2(x, mTexture2.TopUV);
                spiralVerts[num2 + 4].TextureCoordinate = new Vector2(x2, mTexture2.BottomUV);
                spiralVerts[num2 + 5].TextureCoordinate = new Vector2(x, mTexture2.BottomUV);
                num2 += 6;
            }
        }
        for (int l = 0; l < 220; l++)
        {
            particles[l].Percent = Calc.Random.NextFloat();
            particles[l].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (MathF.PI * 2f), 1f);
            particles[l].Color = Calc.Random.Next(activeColors.Length);
        }
        center = new Vector2(320f, 180f) / 2f;
        offset = Vector2.Zero;
        colorsLerp = new Color[activeColors.Length];
        colorsLerpBlack = new Color[activeColors.Length, 20];
        colorsLerpTransparent = new Color[activeColors.Length, 20];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void SnapStrength(Level level, Strengths strength)
    {
        this.strength = strength;
        StrengthMultiplier = 1f + (float)strength;
        level.Session.SetCounter("blackhole_strength", (int)strength);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void NextStrength(Level level, Strengths strength)
    {
        this.strength = strength;
        level.Session.SetCounter("blackhole_strength", (int)strength);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void SetStrength(Strengths strength)
    {
        this.strength = strength;
        StrengthMultiplier = 1f + (float)strength;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void SetGeneratorBreakProgress(float progress)
    {
        // Map generator break progress (0-1) to strength multiplier
        // As more generators break, the blackhole gets stronger
        StrengthMultiplier = 1f + progress * 3f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update(Scene scene)
    {
        base.Update(scene);
        if (!checkedFlag)
        {
            int counter = (scene as Level).Session.GetCounter("blackhole_strength");
            if (counter > 0)
            {
                SnapStrength(scene as Level, (Strengths)counter);
            }
            checkedFlag = true;
        }
        if (!Visible)
        {
            return;
        }
        StrengthMultiplier = Calc.Approach(StrengthMultiplier, 1f + (float)strength, Engine.DeltaTime * 0.1f);
        if (scene.OnInterval(0.05f))
        {
            for (int i = 0; i < colorsLerp.Length; i++)
            {
                colorsLerp[i] = GetPhaseColor(i);
                for (int j = 0; j < 20; j++)
                {
                    colorsLerpBlack[i, j] = Color.Lerp(colorsLerp[i], Color.Black, (float)j / 19f) * FadeAlphaMultiplier;
                    colorsLerpTransparent[i, j] = Color.Lerp(colorsLerp[i], Color.Transparent, (float)j / 19f) * FadeAlphaMultiplier;
                }
            }
        }
        float num = 1f + (StrengthMultiplier - 1f) * 0.7f;
        int streamCount = StreamCount;
        int num2 = 0;
        for (int k = 0; k < streamCount; k++)
        {
            streams[k].Percent += streams[k].Speed * Engine.DeltaTime * num * Direction;
            if (streams[k].Percent >= 1f && Direction > 0f)
            {
                streams[k].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (MathF.PI * 2f), 1f);
                streams[k].Percent -= 1f;
            }
            else if (streams[k].Percent < 0f && Direction < 0f)
            {
                streams[k].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (MathF.PI * 2f), 1f);
                streams[k].Percent += 1f;
            }
            float percent = streams[k].Percent;
            float num3 = Ease.CubeIn(Calc.ClampedMap(percent, 0f, 0.8f));
            float num4 = Ease.CubeIn(Calc.ClampedMap(percent, 0.2f, 1f));
            Vector2 normal = streams[k].Normal;
            Vector2 vector = normal.Perpendicular();
            Vector2 vector2 = normal * 16f + normal * (1f - num3) * 200f;
            float num5 = (1f - num3) * 8f;
            Color a = colorsLerpBlack[streams[k].Color, (int)(num3 * 0.6f * 19f)];
            Vector2 vector3 = normal * 16f + normal * (1f - num4) * 280f;
            float num6 = (1f - num4) * 8f;
            Color c = colorsLerpBlack[streams[k].Color, (int)(num4 * 0.6f * 19f)];
            Vector2 a2 = vector2 - vector * num5;
            Vector2 b = vector2 + vector * num5;
            Vector2 c2 = vector3 + vector * num6;
            Vector2 d = vector3 - vector * num6;
            AssignVertColors(streamVerts, num2, ref a, ref a, ref c, ref c);
            AssignVertPosition(streamVerts, num2, ref a2, ref b, ref c2, ref d);
            num2 += 6;
        }
        float num7 = StrengthMultiplier * 0.25f;
        int particleCount = ParticleCount;
        for (int l = 0; l < particleCount; l++)
        {
            particles[l].Percent += Engine.DeltaTime * num7 * Direction;
            if (particles[l].Percent >= 1f && Direction > 0f)
            {
                particles[l].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (MathF.PI * 2f), 1f);
                particles[l].Percent -= 1f;
            }
            else if (particles[l].Percent < 0f && Direction < 0f)
            {
                particles[l].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * (MathF.PI * 2f), 1f);
                particles[l].Percent += 1f;
            }
        }
        float num8 = 0.2f + (StrengthMultiplier - 1f) * 0.1f;
        int spiralCount = SpiralCount;
        Color value = Color.Lerp(GetOuterBgColor(), Color.White, 0.1f) * 0.8f;
        int num9 = 0;
        for (int m = 0; m < spiralCount; m++)
        {
            spirals[m].Percent += streams[m].Speed * Engine.DeltaTime * num8 * Direction;
            if (spirals[m].Percent >= 1f && Direction > 0f)
            {
                spirals[m].Offset = Calc.Random.NextFloat(MathF.PI * 2f);
                spirals[m].Percent -= 1f;
            }
            else if (spirals[m].Percent < 0f && Direction < 0f)
            {
                spirals[m].Offset = Calc.Random.NextFloat(MathF.PI * 2f);
                spirals[m].Percent += 1f;
            }
            float percent2 = spirals[m].Percent;
            float num10 = spirals[m].Offset;
            float value2 = Calc.ClampedMap(percent2, 0f, 0.8f);
            float value3 = Calc.ClampedMap(percent2, 0f, 1f);
            for (int n = 0; n < 12; n++)
            {
                float num11 = 1f - MathHelper.Lerp(value2, value3, (float)n / 12f);
                float num12 = 1f - MathHelper.Lerp(value2, value3, (float)(n + 1) / 12f);
                Vector2 vector4 = Calc.AngleToVector(num11 * (20f + (float)n * 0.2f) + num10, 1f);
                Vector2 vector5 = vector4 * num11 * 200f;
                float num13 = num11 * (4f + StrengthMultiplier * 4f);
                Vector2 vector6 = Calc.AngleToVector(num12 * (20f + (float)(n + 1) * 0.2f) + num10, 1f);
                Vector2 vector7 = vector6 * num12 * 200f;
                float num14 = num12 * (4f + StrengthMultiplier * 4f);
                Color a3 = Color.Lerp(value, Color.Black, (1f - num11) * 0.5f);
                Color b2 = Color.Lerp(value, Color.Black, (1f - num12) * 0.5f);
                Vector2 a4 = vector5 + vector4 * num13;
                Vector2 b3 = vector7 + vector6 * num14;
                Vector2 c3 = vector7 - vector6 * num14;
                Vector2 d2 = vector5 - vector4 * num13;
                AssignVertColors(spiralVerts, num9, ref a3, ref b2, ref b2, ref a3);
                AssignVertPosition(spiralVerts, num9, ref a4, ref b3, ref c3, ref d2);
                num9 += 6;
            }
        }
        Vector2 wind = (scene as Level).Wind;
        Vector2 vector8 = new Vector2(320f, 180f) / 2f + wind * 0.15f + CenterOffset;
        center += (vector8 - center) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
        Vector2 vector9 = -wind * 0.25f + OffsetOffset;
        offset += (vector9 - offset) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
        if (scene.OnInterval(0.025f))
        {
            shake = Calc.AngleToVector(Calc.Random.NextFloat(MathF.PI * 2f), 2f * (StrengthMultiplier - 1f));
        }
        spinTime += (2f + StrengthMultiplier) * Engine.DeltaTime;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Color GetPhaseColor(int i)
    {
        // Strength-based phases: mild (purple) -> wild (pink) -> red -> rainbow
        float t = StrengthMultiplier - 1f;
        Color baseColor;
        if (t <= 2f)
            baseColor = Color.Lerp(colorsMild[i % colorsMild.Length], colorsWild[i % colorsWild.Length], t / 2f);
        else
            baseColor = Color.Lerp(colorsWild[i % colorsWild.Length], colorsRed[i % colorsRed.Length], Calc.ClampedMap(t, 2f, 3f));
        if (RainbowMode || t > 3f)
        {
            Color rainbow = HueToColor((spinTime * 0.15f + (float)i / colorsLerp.Length) % 1f);
            baseColor = RainbowMode ? rainbow : Color.Lerp(baseColor, rainbow, Calc.ClampedMap(t, 3f, 4f));
        }
        return baseColor;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Color GetOuterBgColor()
    {
        float t = StrengthMultiplier - 1f;
        Color color;
        if (t <= 2f)
            color = Color.Lerp(bgColorOuterMild, bgColorOuterWild, t / 2f);
        else
            color = Color.Lerp(bgColorOuterWild, colorsRed[1], Calc.ClampedMap(t, 2f, 3f));
        if (RainbowMode || t > 3f)
        {
            Color rainbow = HueToColor(spinTime * 0.1f % 1f);
            color = RainbowMode ? rainbow : Color.Lerp(color, rainbow, Calc.ClampedMap(t, 3f, 4f));
        }
        return color;
    }

    private static Color HueToColor(float hue)
    {
        float h = (hue % 1f + 1f) % 1f * 6f;
        float x = 1f - Math.Abs(h % 2f - 1f);
        return h switch
        {
            < 1f => new Color(1f, x, 0f),
            < 2f => new Color(x, 1f, 0f),
            < 3f => new Color(0f, 1f, x),
            < 4f => new Color(0f, x, 1f),
            < 5f => new Color(x, 0f, 1f),
            _ => new Color(1f, 0f, x)
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AssignVertColors(VertexPositionColorTexture[] verts, int v, ref Color a, ref Color b, ref Color c, ref Color d)
    {
        verts[v].Color = a;
        verts[v + 1].Color = b;
        verts[v + 2].Color = c;
        verts[v + 3].Color = a;
        verts[v + 4].Color = c;
        verts[v + 5].Color = d;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AssignVertPosition(VertexPositionColorTexture[] verts, int v, ref Vector2 a, ref Vector2 b, ref Vector2 c, ref Vector2 d)
    {
        verts[v].Position = new Vector3(a, 0f);
        verts[v + 1].Position = new Vector3(b, 0f);
        verts[v + 2].Position = new Vector3(c, 0f);
        verts[v + 3].Position = new Vector3(a, 0f);
        verts[v + 4].Position = new Vector3(c, 0f);
        verts[v + 5].Position = new Vector3(d, 0f);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void BeforeRender(Scene scene)
    {
        if (buffer == null || buffer.IsDisposed)
        {
            buffer = VirtualContent.CreateRenderTarget("Black Hole", 320, 180);
        }
        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
        Engine.Graphics.GraphicsDevice.Clear(bgColorInner);
        Draw.SpriteBatch.Begin();
        Color value = GetOuterBgColor();
        for (int i = 0; i < 20; i++)
        {
            float num = (1f - spinTime % 1f) * 0.05f + (float)i / 20f;
            Color color = Color.Lerp(bgColorInner, value, Ease.SineOut(num));
            float scale = Calc.ClampedMap(num, 0f, 1f, 0.1f, 4f);
            float rotation = MathF.PI * 2f * num;
            bgTexture.DrawCentered(center + offset * num + shake * (1f - num), color, scale, rotation);
        }
        Draw.SpriteBatch.End();
        if (SpiralCount > 0)
        {
            Engine.Instance.GraphicsDevice.Textures[0] = GFX.Game.Sources[0].Texture_Safe;
            GFX.DrawVertices(Matrix.CreateTranslation(center.X, center.Y, 0f), spiralVerts, SpiralCount * 12 * 6, GFX.FxTexture);
        }
        if (StreamCount > 0)
        {
            Engine.Instance.GraphicsDevice.Textures[0] = GFX.Game.Sources[0].Texture_Safe;
            GFX.DrawVertices(Matrix.CreateTranslation(center.X, center.Y, 0f), streamVerts, StreamCount * 6, GFX.FxTexture);
        }
        Draw.SpriteBatch.Begin();
        int particleCount = ParticleCount;
        for (int j = 0; j < particleCount; j++)
        {
            float num2 = Ease.CubeIn(Calc.Clamp(particles[j].Percent, 0f, 1f));
            Vector2 vector = center + particles[j].Normal * Calc.ClampedMap(num2, 1f, 0f, 8f, 220f);
            Color color2 = colorsLerpTransparent[particles[j].Color, (int)(num2 * 19f)];
            float num3 = 1f + (1f - num2) * 1.5f;
            Draw.Rect(vector - new Vector2(num3, num3) / 2f, num3, num3, color2);
        }
        Draw.SpriteBatch.End();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Ended(Scene scene)
    {
        if (buffer != null)
        {
            buffer.Dispose();
            buffer = null;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render(Scene scene)
    {
        if (buffer != null && !buffer.IsDisposed)
        {
            Vector2 vector = new Vector2(buffer.Width, buffer.Height) / 2f;
            Draw.SpriteBatch.Draw((RenderTarget2D)buffer, vector, buffer.Bounds, Color.White * FadeAlphaMultiplier * Alpha, 0f, vector, Scale, SpriteEffects.None, 0f);
        }
    }
}
