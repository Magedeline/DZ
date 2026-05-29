using Celeste.Cutscenes;
using System.Diagnostics;

namespace Celeste.Entities;
[CustomEntity("MaggyHelper/CharaBoost")]
[HotReloadable]
public class CustomCharaBoost : Entity
{
    public static ParticleType P_Ambience = new ParticleType
    {
        Source = GFX.Game["particles/shard"],
        Color = Color.Red,
        Color2 = Color.Lerp(Color.Red, Color.White, 0.5f),
        ColorMode = ParticleType.ColorModes.Choose,
        FadeMode = ParticleType.FadeModes.Late,
        LifeMin = 0.6f,
        LifeMax = 1.2f,
        Size = 1f,
        SpeedMin = 2f,
        SpeedMax = 16f,
        DirectionRange = (float)Math.PI * 2f
    };

    public static ParticleType P_Move = new ParticleType
    {
        Source = GFX.Game["particles/shard"],
        Color = Color.Red,
        DirectionRange = 0.6981317f,
        SpeedMin = 10f,
        SpeedMax = 20f,
        SpeedMultiplier = 0.2f,
        LifeMin = 0.6f,
        LifeMax = 1.2f
    };

    private const float MoveSpeed = 320f;

    private Sprite sprite;

    private Image stretch;

    private Wiggler wiggler;

    private VertexLight light;

    private BloomPoint bloom;

    private bool canSkip;

    private bool finalCh19Boost;

    private bool finalCh19GoldenBoost;

    private bool finalCh19PPBoost;

    private string finalCh19Dialog;

    private Vector2[] nodes;

    private int nodeIndex;

    private bool travelling;

    private global::Celeste.Player holding;

    private SoundSource relocateSfx;

    private readonly TimeRateModifier timeRateModifier;

    public FMOD.Studio.EventInstance Ch19FinalBoostSfx;

    public CustomCharaBoost(Vector2[] nodes, bool lockCamera, bool canSkip = false, bool finalCh19Boost = false, bool finalCh19GoldenBoost = false, bool finalCh19PPBoost = false, string finalCh19Dialog = null)
        : base(nodes[0])
    {
        base.Depth = -1000000;
        this.nodes = nodes;
        this.canSkip = canSkip;
        this.finalCh19Boost = finalCh19Boost;
        this.finalCh19GoldenBoost = finalCh19GoldenBoost;
        this.finalCh19PPBoost = finalCh19PPBoost;
        this.finalCh19Dialog = finalCh19Dialog;
        base.Collider = new Circle(16f);
        Add(new PlayerCollider(OnPlayer));
        Add(sprite = GFX.SpriteBank.Create("charaboost"));
        Add(stretch = new Image(GFX.Game["objects/charaboost/stretch"]));
        stretch.Visible = false;
        stretch.CenterOrigin();
        Add(light = new VertexLight(Color.White, 0.7f, 12, 20));
        Add(bloom = new BloomPoint(0.5f, 12f));
        Add(wiggler = Wiggler.Create(0.4f, 3f, (float f) =>
        {
            sprite.Scale = Vector2.One * (1f + wiggler.Value * 0.4f);
        }));
        if (lockCamera)
        {
            Add(new CameraLocker(Level.CameraLockModes.BoostSequence, 0f, 160f));
        }
        Add(timeRateModifier = new TimeRateModifier(1f, false));
        Add(relocateSfx = new SoundSource());
    }

    public CustomCharaBoost(EntityData data, Vector2 offset)
        : this(
            data.NodesWithPosition(offset),
            data.Bool("lockCamera", defaultValue: true),
            data.Bool("canSkip", defaultValue: false),
            data.Bool("finalCh19Boost", defaultValue: false),
            data.Bool("finalCh19GoldenBoost", defaultValue: false),
            data.Bool("finalCh19PPBoost", defaultValue: false),
            data.Attr("finalCh19Dialog", "") is string d && d.Length > 0 ? d : null
        )
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (CollideCheck<FakeWall>())
        {
            base.Depth = -12500;
        }
    }

    private void OnPlayer(global::Celeste.Player player)
    {
        Add(new Coroutine(BoostRoutine(player)));
    }

    private IEnumerator BoostRoutine(global::Celeste.Player player)
    {
        holding = player;
        travelling = true;
        nodeIndex++;
        sprite.Visible = false;
        sprite.Position = Vector2.Zero;
        Collidable = false;
        bool finalBoost = nodeIndex >= nodes.Length;
        Level level = Scene as Level;
        bool endLevel;
        if (finalBoost && finalCh19GoldenBoost && finalCh19PPBoost)
        {
            endLevel = true;
        }
        else
        {
            bool hasRegularStrawberry = false;
            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is Strawberry { Golden: false })
                {
                    hasRegularStrawberry = true;
                    break;
                }
            }
            bool hasPinkPlatinumBerry = false;
            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is PinkPlatinumBerry)
                {
                    hasPinkPlatinumBerry = true;
                    break;
                }
            }
            endLevel = finalBoost
                && (!finalCh19Boost || !hasRegularStrawberry)
                && (!finalCh19PPBoost || !hasPinkPlatinumBerry);
        }
        Stopwatch sw = new Stopwatch();
        sw.Start();
        if (finalCh19Boost)
        {
            if (!string.IsNullOrEmpty(finalCh19Dialog))
            {
                level.Session.SetFlag(finalCh19Dialog, true);
            }
            Audio.Play("event:/pusheen/final_content/char/chara/booster_finalfinal_part1", Position);
        }
        else if (!finalBoost)
        {
            Audio.Play("event:/char/badeline/booster_begin", Position);
        }
        else
        {
            Audio.Play("event:/char/badeline/booster_final", Position);
        }
        if (player.Holding != null)
        {
            player.Drop();
        }
        player.StateMachine.State = Player.StDummy;
        player.DummyAutoAnimate = false;
        player.DummyGravity = false;
        if (player.Inventory.Dashes > 1)
        {
            player.Dashes = 1;
        }
        else
        {
            player.RefillDash();
        }
        player.RefillStamina();
        player.Speed = Vector2.Zero;
        int num = Math.Sign(player.X - X);
        if (num == 0)
        {
            num = -1;
        }
        CharaDummy chara = new CharaDummy(Position, finalCh19Dialog);
        Scene.Add(chara);
        player.Facing = (Facings)(-num);
        if (chara.Sprite != null)
        {
            chara.Sprite.Scale.X = num;
        }
        Vector2 playerFrom = player.Position;
        Vector2 playerTo = Position + new Vector2(num * 4, -3f);
        Vector2 charaFrom = chara.Position;
        Vector2 charaTo = Position + new Vector2(-num * 4, 3f);
        for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.2f)
        {
            Vector2 vector = Vector2.Lerp(playerFrom, playerTo, p);
            if (player.Scene != null)
            {
                player.MoveToX(vector.X);
            }
            if (player.Scene != null)
            {
                player.MoveToY(vector.Y);
            }
            chara.Position = Vector2.Lerp(charaFrom, charaTo, p);
            yield return null;
        }
        if (finalBoost)
        {
            Vector2 screenSpaceFocusPoint = new Vector2(Calc.Clamp(player.X - level.Camera.X, 120f, 200f), Calc.Clamp(player.Y - level.Camera.Y, 60f, 120f));
            Add(new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.5f, 0.18f)));
            timeRateModifier.SetTimeRateMultiplier(0.5f);
        }
        else
        {
            Audio.Play("event:/char/badeline/booster_throw", Position);
        }
        if (chara.Sprite != null)
        {
            chara.Sprite.Play("boost");
        }
        yield return 0.1f;
        if (!player.Dead)
        {
            player.MoveV(5f);
        }
        yield return 0.1f;
        if (endLevel)
        {
            level.TimerStopped = true;
            level.RegisterAreaComplete();
        }
        if (finalBoost && finalCh19Boost)
        {
            Scene.Add(new CS19_FinalLaunch(player, this, finalCh19GoldenBoost, finalCh19PPBoost));
            player.Active = false;
            chara.Active = false;
            Active = false;
            yield return true;
            player.Active = true;
            chara.Active = true;
        }
        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
        {
            if (player.Dashes < player.Inventory.Dashes)
            {
                player.Dashes++;
            }
            Scene.Remove(chara);
            (Scene as Level).Displacement.AddBurst(chara.Position, 0.25f, 8f, 32f, 0.5f);
        }, 0.15f, start: true));
        (Scene as Level).Shake();
        holding = null;
        if (!finalBoost)
        {
            player.BadelineBoostLaunch(CenterX);
            Vector2 from = Position;
            Vector2 to = nodes[nodeIndex];
            float val = Vector2.Distance(from, to) / 320f;
            val = Math.Min(3f, val);
            stretch.Visible = true;
            stretch.Rotation = (to - from).Angle();
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, val, start: true);
            tween.OnUpdate = (Tween t) =>
            {
                Position = Vector2.Lerp(from, to, t.Eased);
                stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
                stretch.Scale.Y = 1f - Calc.YoYo(t.Eased) * 0.75f;
                if (t.Eased < 0.9f && Scene.OnInterval(0.03f))
                {
                    TrailManager.Add(this, global::Celeste.Player.TwoDashesHairColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
                    level.ParticlesFG.Emit(P_Move, 1, Center, Vector2.One * 4f);
                }
            };
            tween.OnComplete = (Tween t) =>
            {
                if (X >= (float)level.Bounds.Right)
                {
                    RemoveSelf();
                }
                else
                {
                    travelling = false;
                    stretch.Visible = false;
                    sprite.Visible = true;
                    Collidable = true;
                    Audio.Play("event:/char/badeline/booster_reappear", Position);
                }
            };
            Add(tween);
            relocateSfx.Play("event:/char/badeline/booster_relocate");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.DirectionalShake(-Vector2.UnitY);
            level.Displacement.AddBurst(Center, 0.4f, 8f, 32f, 0.5f);
        }
        else
        {
            if (finalCh19Boost)
            {
                Ch19FinalBoostSfx = Audio.Play("event:/pusheen/final_content/char/chara/booster_finalfinal_part2", Position);
            }
            Engine.FreezeTimer = 0.1f;
            yield return null;
            if (endLevel)
            {
                level.TimerHidden = true;
            }
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            level.Flash(Color.White * 0.5f, drawPlayerOver: true);
            level.DirectionalShake(-Vector2.UnitY, 0.6f);
            level.Displacement.AddBurst(Center, 0.6f, 8f, 64f, 0.5f);
            level.ResetZoom();
            player.SummitLaunch(X);
            timeRateModifier.ResetTimeRateMultiplier();
            Finish();
        }
    }

    private void Skip()
    {
        travelling = true;
        nodeIndex++;
        Collidable = false;
        Level level = SceneAs<Level>();
        Vector2 from = Position;
        Vector2 to = nodes[nodeIndex];
        float val = Vector2.Distance(from, to) / 320f;
        val = Math.Min(3f, val);
        stretch.Visible = true;
        stretch.Rotation = (to - from).Angle();
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, val, start: true);
        tween.OnUpdate = (Tween t) =>
        {
            Position = Vector2.Lerp(from, to, t.Eased);
            stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
            stretch.Scale.Y = 1f - Calc.YoYo(t.Eased) * 0.75f;
            if (t.Eased < 0.9f && Scene.OnInterval(0.03f))
            {
                TrailManager.Add(this, global::Celeste.Player.TwoDashesHairColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
                level.ParticlesFG.Emit(P_Move, 1, Center, Vector2.One * 4f);
            }
        };
        tween.OnComplete = (Tween t) =>
        {
            if (X >= (float)level.Bounds.Right)
            {
                RemoveSelf();
            }
            else
            {
                travelling = false;
                stretch.Visible = false;
                sprite.Visible = true;
                Collidable = true;
                Audio.Play("event:/char/badeline/booster_reappear", Position);
            }
        };
        Add(tween);
        relocateSfx.Play("event:/char/badeline/booster_relocate");
        level.Displacement.AddBurst(base.Center, 0.4f, 8f, 32f, 0.5f);
    }

    public void Wiggle()
    {
        wiggler.Start();
        (base.Scene as Level).Displacement.AddBurst(Position, 0.3f, 4f, 16f, 0.25f);
        Audio.Play("event:/game/general/crystalheart_pulse", Position);
    }

    public override void Update()
    {
        if (sprite.Visible && base.Scene.OnInterval(0.05f))
        {
            SceneAs<Level>().ParticlesBG.Emit(P_Ambience, 1, base.Center, Vector2.One * 3f);
        }
        if (holding != null)
        {
            holding.Speed = Vector2.Zero;
        }
        if (!travelling)
        {
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                float num = Calc.ClampedMap((entity.Center - Position).Length(), 16f, 64f, 12f, 0f);
                Vector2 vector = (entity.Center - Position).SafeNormalize();
                sprite.Position = Calc.Approach(sprite.Position, vector * num, 32f * Engine.DeltaTime);
                if (canSkip && entity.Position.X - base.X >= 100f && nodeIndex + 1 < nodes.Length)
                {
                    Skip();
                }
            }
        }
        light.Visible = (bloom.Visible = sprite.Visible || stretch.Visible);
        base.Update();
    }

    private void Finish()
    {
        SceneAs<Level>().Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
        SceneAs<Level>().Particles.Emit(CharaChaser.P_Vanish, 12, base.Center, Vector2.One * 6f);
        SceneAs<Level>().CameraLockMode = Level.CameraLockModes.None;
        SceneAs<Level>().CameraOffset = new Vector2(0f, -16f);
        RemoveSelf();
    }
}




