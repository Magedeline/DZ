using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

#nullable disable
namespace MaggyHelper;

[CustomEntity(ids: "MaggyHelper/StrawberryRemix")]
[Tracked]
public class StrawberryRemix : Entity
{
    public EntityID ID;
    public Follower Follower;

    private Sprite sprite;
    private Wiggler scaleWiggler;
    private Wiggler rotateWiggler;
    private BloomPoint bloom;
    private VertexLight light;
    private Tween lightPulse;

    private Vector2 home;
    private float wobble;
    private float collectTimer;
    private float flapSpeed;
    private float particleTimer;

    private bool collected;
    private bool isGhost;
    private bool flyingAway;

    private readonly float bobAmplitude;
    private readonly float bobSpeed;
    private readonly float collectDelay;
    private readonly float glowInterval;

    public bool ReturnHomeWhenLost = true;
    public bool WaitingOnSeeds;

    public bool Winged { get; private set; }
    public bool Golden { get; private set; }
    public bool Moon { get; private set; }
    public bool Pink { get; private set; }
    public bool PopStar { get; private set; }

    public StrawberryRemix(EntityData data, Vector2 offset, EntityID gid)
    {
        ID = gid;
        Position = home = data.Position + offset;

        Winged = data.Bool("winged");
        Golden = data.Bool("golden") || data.Name == "goldenBerry";
        Pink = data.Bool("pink");
        Moon = data.Bool("moon");
        PopStar = data.Bool("popstar");

        bobAmplitude = data.Float("bobAmplitude", 2f);
        bobSpeed = data.Float("bobSpeed", 4f);
        collectDelay = data.Float("collectDelay", 0.15f);
        glowInterval = data.Float("glowInterval", 0.08f);

        isGhost = SaveData.Instance.CheckStrawberry(ID);

        Depth = -100;
        Collider = new Hitbox(14f, 14f, -7f, -7f);

        Add(new PlayerCollider(OnPlayer));
        Add(new MirrorReflection());
        Add(Follower = new Follower(ID, OnLoseLeader));

        Follower.FollowDelay = 0.3f;

        if (Winged)
        {
            Add(new DashListener { OnDash = OnDash });
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        sprite = GFX.SpriteBank.Create(GetSpriteName(isGhost));
        if (isGhost)
        {
            sprite.Color = Color.White * 0.8f;
        }

        Add(sprite);

        if (Winged)
        {
            sprite.Play("flap");
        }

        sprite.OnFrameChange = OnAnimate;

        Add(scaleWiggler = Wiggler.Create(0.4f, 4f, v => sprite.Scale = Vector2.One * (1f + v * 0.35f)));
        Add(rotateWiggler = Wiggler.Create(0.5f, 4f, v => sprite.Rotation = v * Calc.DegToRad * 30f));

        float bloomAlpha = (Golden || Pink || Moon || PopStar || isGhost) ? 0.5f : 1f;
        Add(bloom = new BloomPoint(bloomAlpha, 12f));
        Add(light = new VertexLight(Color.White, 1f, 16, 24));
        Add(lightPulse = light.CreatePulseTween());

        Level level = scene as Level;
        if (level != null && level.Session.BloomBaseAdd > 0.1f)
        {
            bloom.Alpha *= 0.5f;
        }
    }

    public override void Update()
    {
        if (WaitingOnSeeds)
        {
            return;
        }

        if (!collected)
        {
            UpdateMovement();
            UpdateCollection();
        }

        base.Update();
        EmitGlowParticles();
    }

    private void UpdateMovement()
    {
        if (!Winged)
        {
            wobble += Engine.DeltaTime * bobSpeed;
            float y = (float)Math.Sin(wobble) * bobAmplitude;
            sprite.Y = bloom.Y = light.Y = y;
            return;
        }

        Y += flapSpeed * Engine.DeltaTime;

        if (flyingAway)
        {
            if (Y < SceneAs<Level>().Bounds.Top - 16)
            {
                RemoveSelf();
            }
            return;
        }

        flapSpeed = Calc.Approach(flapSpeed, 20f, 170f * Engine.DeltaTime);

        if (Y < home.Y - 5f)
        {
            Y = home.Y - 5f;
        }
        else if (Y > home.Y + 5f)
        {
            Y = home.Y + 5f;
        }
    }

    private void UpdateCollection()
    {
        int followIndex = Follower.FollowIndex;

        if (Follower.Leader == null || Follower.DelayTimer > 0f || !IsFirstNonGoldenBerry())
        {
            if (followIndex > 0)
            {
                collectTimer = -collectDelay;
            }
            return;
        }

        Player player = Follower.Leader.Entity as Player;
        bool canCollect = false;

        if (player != null && player.Scene != null && !player.StrawberriesBlocked)
        {
            if (Golden)
            {
                canCollect = player.CollideCheck<GoldBerryCollectTrigger>() || (Scene as Level).Completed;
            }
            else
            {
                canCollect = player.OnSafeGround && (!Moon || player.StateMachine.State != 13);
            }
        }

        if (!canCollect)
        {
            collectTimer = Math.Min(collectTimer, 0f);
            return;
        }

        collectTimer += Engine.DeltaTime;
        if (collectTimer > collectDelay)
        {
            OnCollect();
        }
    }

    private void EmitGlowParticles()
    {
        if (Follower.Leader == null)
        {
            return;
        }

        particleTimer += Engine.DeltaTime;
        if (particleTimer < glowInterval)
        {
            return;
        }

        particleTimer = 0f;
        SceneAs<Level>().ParticlesFG.Emit(GetGlowParticleType(), Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
    }

    private ParticleType GetGlowParticleType()
    {
        if (isGhost)
        {
            return Strawberry.P_GhostGlow;
        }

        if (Golden)
        {
            return Strawberry.P_GoldGlow;
        }

        if (Moon)
        {
            return Strawberry.P_MoonGlow;
        }

        return Strawberry.P_Glow;
    }

    private bool IsFirstNonGoldenBerry()
    {
        if (Follower.Leader == null)
        {
            return false;
        }

        for (int i = Follower.FollowIndex - 1; i >= 0; i--)
        {
            if (Follower.Leader.Followers[i].Entity is Strawberry other && !other.Golden)
            {
                return false;
            }
        }

        return true;
    }

    private void OnDash(Vector2 _)
    {
        if (flyingAway || !Winged || WaitingOnSeeds)
        {
            return;
        }

        Depth = -1000000;
        Add(new Coroutine(FlyAwayRoutine()));
        flyingAway = true;
    }

    private IEnumerator FlyAwayRoutine()
    {
        rotateWiggler.Start();
        flapSpeed = -200f;

        Tween grow = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.2f, true);
        grow.OnUpdate = t => sprite.Scale = Vector2.One * (1f + t.Eased * 0.45f);
        Add(grow);

        yield return 0.1f;
        Audio.Play("event:/game/general/strawberry_laugh", Position);

        yield return 0.2f;
        if (!Follower.HasLeader)
        {
            Audio.Play("event:/game/general/strawberry_flyaway", Position);
        }

        Tween settle = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 0.5f, true);
        settle.OnUpdate = t => sprite.Scale = Vector2.One * (1.45f - t.Eased * 0.45f);
        Add(settle);
    }

    private void OnAnimate(string id)
    {
        if (!flyingAway && id == "flap" && sprite.CurrentAnimationFrame % 9 == 4)
        {
            Audio.Play("event:/game/general/strawberry_wingflap", Position);
            flapSpeed = -50f;
        }

        int pulseFrame = id == "flap" ? 25 : (Golden || Moon ? 30 : 35);
        if (sprite.CurrentAnimationFrame != pulseFrame)
        {
            return;
        }

        lightPulse.Start();
        Audio.Play("event:/game/general/strawberry_pulse", Position);

        float burst = (!collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>())) ? 0.1f : 0.2f;
        SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, burst);
    }

    private void OnPlayer(Player player)
    {
        if (Follower.Leader != null || collected || WaitingOnSeeds)
        {
            return;
        }

        ReturnHomeWhenLost = true;

        if (Winged)
        {
            Level level = SceneAs<Level>();
            Winged = false;
            sprite.Rate = 0f;

            Alarm.Set(this, Follower.FollowDelay, () =>
            {
                sprite.Rate = 1f;
                sprite.Play("idle");
                level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position + new Vector2(8f, 0f), new Vector2(4f, 2f));
                level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position - new Vector2(8f, 0f), new Vector2(4f, 2f));
            });
        }

        if (Golden)
        {
            (Scene as Level).Session.GrabbedGolden = true;
        }

        Audio.Play(isGhost ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);
        player.Leader.GainFollower(Follower);
        scaleWiggler.Start();
        Depth = -1000000;
    }

    private void OnCollect()
    {
        if (collected)
        {
            return;
        }

        int collectIndex = 0;
        collected = true;

        if (Follower.Leader != null)
        {
            Player player = Follower.Leader.Entity as Player;
            collectIndex = player.StrawberryCollectIndex;
            player.StrawberryCollectIndex++;
            player.StrawberryCollectResetTimer = 2.5f;
            Follower.Leader.LoseFollower(Follower);
        }

        if (Moon)
        {
            Achievements.Register(Achievement.WOW);
        }

        SaveData.Instance.AddStrawberry(ID, Golden);

        Session session = (Scene as Level).Session;
        session.DoNotLoad.Add(ID);
        session.Strawberries.Add(ID);
        session.UpdateLevelStartDashes();

        Add(new Coroutine(CollectRoutine(collectIndex)));
    }

    private IEnumerator CollectRoutine(int collectIndex)
    {
        Tag = (int)Tags.TransitionUpdate;
        Depth = -2000010;

        int color = Moon ? 3 : (isGhost ? 1 : (Golden ? 2 : 0));
        Audio.Play("event:/game/general/strawberry_get", Position, "colour", color, "count", collectIndex);

        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        sprite.Play("collect");

        while (sprite.Animating)
        {
            yield return null;
        }

        Scene.Add(new StrawberryPoints(Position, isGhost, collectIndex, Moon));
        RemoveSelf();
    }

    private void OnLoseLeader()
    {
        if (collected || !ReturnHomeWhenLost)
        {
            return;
        }

        Alarm.Set(this, 0.15f, () =>
        {
            Vector2 dir = (home - Position).SafeNormalize();
            float dist = Vector2.Distance(Position, home);
            float arc = Calc.ClampedMap(dist, 16f, 120f, 16f, 96f);
            SimpleCurve curve = new SimpleCurve(Position, home, home + dir * 16f + dir.Perpendicular() * arc * Calc.Random.Choose(1, -1));

            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(dist / 100f, 0.4f), true);
            tween.OnUpdate = t => Position = curve.GetPoint(t.Eased);
            tween.OnComplete = _ => Depth = -100;
            Add(tween);
        });
    }

    private string GetSpriteName(bool ghostBerry)
    {
        if (PopStar)
        {
            return ghostBerry ? "popstarghostberry" : "popstarberry";
        }

        if (Moon)
        {
            return ghostBerry ? "moonghostberry" : "moonberry";
        }

        if (Pink)
        {
            return ghostBerry ? "ghostpinkplatinumberry" : "pinkplatinumberry";
        }

        if (Golden)
        {
            return ghostBerry ? "goldghostberry" : "goldberry";
        }

        return ghostBerry ? "ghostberry" : "strawberry";
    }
}