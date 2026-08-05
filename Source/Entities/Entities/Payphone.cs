using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Entities;

[CustomEntity("DZ/Payphone")]
[Tracked]
[HotReloadable]
public class Payphone : Entity
{
    public static ParticleType P_Snow;

    public static ParticleType P_SnowB;

    static Payphone()
    {
        P_Snow = new ParticleType
        {
            Size = 1f,
            Color = Color.White,
            Color2 = Color.LightGray,
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 1.5f,
            LifeMax = 3.0f,
            SpeedMin = 10f,
            SpeedMax = 30f,
            DirectionRange = (float)Math.PI * 0.5f,
            Acceleration = new Vector2(0f, 20f),
            SpinMin = -1f,
            SpinMax = 1f
        };

        P_SnowB = new ParticleType
        {
            Size = 0.7f,
            Color = Color.LightBlue,
            Color2 = Color.White,
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 1.0f,
            LifeMax = 2.0f,
            SpeedMin = 8f,
            SpeedMax = 20f,
            DirectionRange = (float)Math.PI * 0.4f,
            Acceleration = new Vector2(0f, 15f),
            SpinMin = -0.5f,
            SpinMax = 0.5f
        };
    }

    public bool Broken;

    public Sprite Sprite;

    public Image Blink;

    private VertexLight light;

    private BloomPoint bloom;

    private float lightFlickerTimer;

    private float lightFlickerFor = 0.1f;

    private int lastFrame;

    private SoundSource buzzSfx;

    public Payphone(EntityData data, Vector2 offset)
        : this(data.Position + offset)
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Payphone(Vector2 pos)
        : base(pos)
    {
        base.Depth = -1;
        Add(Sprite = new Sprite(GFX.Game, "cutscenes/DZ/payphone/"));
        Sprite.AddLoop("idle", "phone", 0.1f, 0);
        Sprite.Add("pickUp", "phone", 0.08f, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
        Sprite.AddLoop("talkPhone", "phone", 0.08f, 11);
        Sprite.Add("jumpBack", "phone", 0.08f, 12, 13, 14, 15, 16, 17);
        Sprite.Add("scare", "phone", 0.08f, 18, 19, 20, 21);
        Sprite.Add("transform", "phone", 0.08f, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45);
        Sprite.Add("eat", "phone", 0.08f, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 83, 83, 84, 84, 85, 85, 86, 86, 87, 87, 74, 75, 76, 77, 78, 79, 80, 81, 82);
        Sprite.AddLoop("monsterIdle", "phone", 0.2f, 83, 84, 85, 86, 87);
        Sprite.Play("idle");
        Add(Blink = new Image(GFX.Game["cutscenes/DZ/payphone/blink"]));
        Blink.Origin = Sprite.Origin;
        Blink.Visible = false;
        Add(light = new VertexLight(new Vector2(-6f, -45f), Color.White, 1f, 8, 96));
        light.Spotlight = true;
        light.SpotlightDirection = new Vector2(0f, 1f).Angle();
        Add(bloom = new BloomPoint(new Vector2(-6f, -45f), 0.8f, 8f));
        Add(buzzSfx = new SoundSource());
        buzzSfx.Play("event:/env/local/02_old_site/phone_lamp");
        buzzSfx.Param("on", 1f);
    }

    public override void Removed(Scene scene)
    {
        buzzSfx.Stop();
        base.Removed(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        if (!Broken)
        {
            lightFlickerTimer -= Engine.DeltaTime;
            if (lightFlickerTimer <= 0f)
            {
                if (base.Scene.OnInterval(0.025f))
                {
                    bool flag = Calc.Random.NextFloat() > 0.5f;
                    light.Visible = flag;
                    bloom.Visible = flag;
                    Blink.Visible = !flag;
                    buzzSfx.Param("on", flag ? 1 : 0);
                }
                if (lightFlickerTimer < 0f - lightFlickerFor)
                {
                    lightFlickerTimer = Calc.Random.Choose(0.4f, 0.6f, 0.8f, 1f);
                    lightFlickerFor = Calc.Random.Choose(0.1f, 0.2f, 0.05f);
                    light.Visible = true;
                    bloom.Visible = true;
                    Blink.Visible = false;
                    buzzSfx.Param("on", 1f);
                }
            }
        }
        else
        {
            Blink.Visible = (bloom.Visible = (light.Visible = false));
            buzzSfx.Param("on", 0f);
        }
        if (Sprite.CurrentAnimationID == "eat" && Sprite.CurrentAnimationFrame == 5 && lastFrame != Sprite.CurrentAnimationFrame)
        {
            Level level = SceneAs<Level>();
            level.ParticlesFG.Emit(P_Snow, 10, level.Camera.Position + new Vector2(236f, 152f), new Vector2(10f, 0f));
            level.ParticlesFG.Emit(P_SnowB, 8, level.Camera.Position + new Vector2(236f, 152f), new Vector2(6f, 0f));
            level.DirectionalShake(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        }
        if (Sprite.CurrentAnimationID == "eat" && Sprite.CurrentAnimationFrame == Sprite.CurrentAnimationTotalFrames - 5 && lastFrame != Sprite.CurrentAnimationFrame)
        {
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
        }
        lastFrame = Sprite.CurrentAnimationFrame;
    }
}
