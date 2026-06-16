using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities;

[CustomEntity(ids: "DZ/FinalTitanSummitBackgroundManager")]
[Tracked(true)]
[HotReloadable]
public class FinalTitanSummitBackgroundManager : Entity
{
    private const string BeginSwapFlag = "finaltitansummit_beginswap_";
    private const string BgSwapFlag = "finaltitansummit_bgswap_";
    private const string PixelLabAssetRoot = "bgs/maggy/21/finaltitansummit/";
    private const float ScreenWidth = 320f;
    private const float ScreenHeight = 180f;
    private const string ThunderSfx = "event:/new_content/game/pusheen/21_desolo_zantas/multiple_lightning_strike";

    private static readonly string[] BirdGonerNames =
    {
        "clover",
        "cody",
        "emily",
        "odin",
        "robin",
        "sabel"
    };

    private static readonly Color[] SoulColors =
    {
        Calc.HexToColor("4fc3ff"),
        Calc.HexToColor("4a6bff"),
        Calc.HexToColor("b48cff"),
        Calc.HexToColor("6de07b"),
        Calc.HexToColor("ffd84f"),
        Calc.HexToColor("ffad47"),
        Calc.HexToColor("ff5f6d")
    };

    private struct CloudParticle
    {
        public Vector2 Position;
        public float Speed;
        public int TextureIndex;
        public float Alpha;
        public float Scale;
        public float WaveOffset;
        public float Depth;
    }

    private struct DebrisParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float Spin;
        public float Alpha;
        public float Scale;
        public int TextureIndex;
        public float Depth;
    }

    private struct CreaturePass
    {
        public Vector2 Position;
        public float Speed;
        public float BobOffset;
        public float Alpha;
        public float Scale;
        public int TextureIndex;
        public float Depth;
    }

    private struct GiygasBand
    {
        public float Offset;
        public float Speed;
        public float Amplitude;
        public float ScaleX;
        public float ScaleY;
        public float Phase;
        public float Alpha;
        public int TextureIndex;
        public float Depth;
    }

    private struct BirdGonerPass
    {
        public Vector2 Position;
        public float Speed;
        public float Drift;
        public float Alpha;
        public float Scale;
        public float AnimationFrame;
        public float BobOffset;
        public int AnimationIndex;
        public float Depth;
        public bool FlipX;
    }

    private struct SoulPass
    {
        public Vector2 Position;
        public float Speed;
        public float Drift;
        public float Alpha;
        public float Scale;
        public float Pulse;
        public int ColorIndex;
        public float Depth;
    }

    private struct StageVisualProfile
    {
        public string ThemeName;
        public int CloudCount;
        public int DebrisCount;
        public int CreatureCount;
        public int GiygasBandCount;
        public float CloudStrength;
        public float DebrisStrength;
        public float CreatureStrength;
        public float GiygasStrength;
        public float ThunderStrength;
        public bool ThunderEnabled;
    }

    private static readonly string[] ActorCycle =
    {
        "asriel_kid",
        "badeline",
        "seven_souls",
        "bird",
        "seven_goner_birds",
        "madeline",
        "player"
    };

    private static readonly StageVisualProfile[] StageProfiles =
    {
        new StageVisualProfile { ThemeName = "calm-cloudbank", CloudCount = 16, DebrisCount = 0, CreatureCount = 0, GiygasBandCount = 4, CloudStrength = 0.95f, DebrisStrength = 0f, CreatureStrength = 0f, GiygasStrength = 0.18f, ThunderStrength = 0f, ThunderEnabled = false },
        new StageVisualProfile { ThemeName = "windpicked-cloudbank", CloudCount = 18, DebrisCount = 2, CreatureCount = 0, GiygasBandCount = 5, CloudStrength = 1f, DebrisStrength = 0.2f, CreatureStrength = 0f, GiygasStrength = 0.24f, ThunderStrength = 0f, ThunderEnabled = false },
        new StageVisualProfile { ThemeName = "violet-drift", CloudCount = 19, DebrisCount = 4, CreatureCount = 1, GiygasBandCount = 5, CloudStrength = 1.05f, DebrisStrength = 0.3f, CreatureStrength = 0.18f, GiygasStrength = 0.3f, ThunderStrength = 0f, ThunderEnabled = false },
        new StageVisualProfile { ThemeName = "orbital-scrap", CloudCount = 20, DebrisCount = 7, CreatureCount = 1, GiygasBandCount = 6, CloudStrength = 1.08f, DebrisStrength = 0.42f, CreatureStrength = 0.22f, GiygasStrength = 0.36f, ThunderStrength = 0f, ThunderEnabled = false },
        new StageVisualProfile { ThemeName = "hollow-sky", CloudCount = 20, DebrisCount = 10, CreatureCount = 2, GiygasBandCount = 6, CloudStrength = 1f, DebrisStrength = 0.55f, CreatureStrength = 0.32f, GiygasStrength = 0.44f, ThunderStrength = 0f, ThunderEnabled = false },
        new StageVisualProfile { ThemeName = "abyssal-approach", CloudCount = 21, DebrisCount = 13, CreatureCount = 3, GiygasBandCount = 7, CloudStrength = 0.96f, DebrisStrength = 0.68f, CreatureStrength = 0.42f, GiygasStrength = 0.52f, ThunderStrength = 0f, ThunderEnabled = false },
        new StageVisualProfile { ThemeName = "storm-threshold", CloudCount = 21, DebrisCount = 16, CreatureCount = 3, GiygasBandCount = 7, CloudStrength = 0.92f, DebrisStrength = 0.76f, CreatureStrength = 0.5f, GiygasStrength = 0.62f, ThunderStrength = 0.65f, ThunderEnabled = true },
        new StageVisualProfile { ThemeName = "red-static", CloudCount = 22, DebrisCount = 19, CreatureCount = 4, GiygasBandCount = 8, CloudStrength = 0.88f, DebrisStrength = 0.84f, CreatureStrength = 0.58f, GiygasStrength = 0.72f, ThunderStrength = 0.78f, ThunderEnabled = true },
        new StageVisualProfile { ThemeName = "void-traffic", CloudCount = 22, DebrisCount = 22, CreatureCount = 4, GiygasBandCount = 8, CloudStrength = 0.82f, DebrisStrength = 0.9f, CreatureStrength = 0.64f, GiygasStrength = 0.82f, ThunderStrength = 0.86f, ThunderEnabled = true },
        new StageVisualProfile { ThemeName = "giygas-wake", CloudCount = 23, DebrisCount = 25, CreatureCount = 5, GiygasBandCount = 9, CloudStrength = 0.78f, DebrisStrength = 0.98f, CreatureStrength = 0.72f, GiygasStrength = 0.92f, ThunderStrength = 0.94f, ThunderEnabled = true },
        new StageVisualProfile { ThemeName = "eldritch-updraft", CloudCount = 24, DebrisCount = 28, CreatureCount = 6, GiygasBandCount = 10, CloudStrength = 0.76f, DebrisStrength = 1.05f, CreatureStrength = 0.82f, GiygasStrength = 1.02f, ThunderStrength = 1.05f, ThunderEnabled = true },
        new StageVisualProfile { ThemeName = "hypergoner-rift", CloudCount = 25, DebrisCount = 31, CreatureCount = 6, GiygasBandCount = 10, CloudStrength = 0.72f, DebrisStrength = 1.12f, CreatureStrength = 0.92f, GiygasStrength = 1.12f, ThunderStrength = 1.15f, ThunderEnabled = true },
        new StageVisualProfile { ThemeName = "final-titan-overload", CloudCount = 26, DebrisCount = 34, CreatureCount = 7, GiygasBandCount = 11, CloudStrength = 0.68f, DebrisStrength = 1.2f, CreatureStrength = 1f, GiygasStrength = 1.25f, ThunderStrength = 1.3f, ThunderEnabled = true }
    };

    private readonly bool dark;
    private readonly bool introLaunch;
    private readonly int index;
    private readonly int progress;
    private readonly StageVisualProfile stageProfile;
    private readonly string cutscene;
    private readonly string ambience;
    private readonly float progression;
    private readonly float cloudStrengthMultiplier;
    private readonly float debrisStrengthMultiplier;
    private readonly float creatureStrengthMultiplier;
    private readonly float giygasStrengthMultiplier;
    private readonly float thunderStrengthMultiplier;
    private readonly Color accentColor;
    private readonly Color creatureColor;
    private readonly Color thunderColor;
    private readonly MTexture customBackgroundTexture;
    private readonly List<MTexture> cloudTextures;
    private readonly List<MTexture> foregroundTextures;
    private readonly List<MTexture> giygasTextures;
    private readonly List<MTexture> debrisTextures;
    private readonly List<MTexture> creatureTextures;
    private readonly List<List<MTexture>> birdGonerAnimations;
    private readonly List<MTexture> soulTextures;
    private readonly MTexture blobTexture;
    private readonly CloudParticle[] clouds;
    private readonly DebrisParticle[] debris;
    private readonly CreaturePass[] creatures;
    private readonly GiygasBand[] giygasBands;
    private readonly BirdGonerPass[] birdGoners;
    private readonly SoulPass[] souls;
    private readonly Vector2[] thunderPath;

    private Level level;
    private global::Celeste.Player player;
    private float fade;
    private float backgroundPulse;
    private float giygasPulse;
    private float thunderCooldown;
    private float thunderFlash;
    private float thunderAlpha;
    private bool outTheTop;
    private Color background;

    public FinalTitanSummitBackgroundManager(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Tag = (int)Tags.TransitionUpdate;
        Depth = 8900;
        Collider = new Hitbox(data.Width, data.Height);

        index = data.Int(nameof(index));
        progress = Math.Clamp(index, 0, 12);
        stageProfile = GetStageProfile(progress);
        progression = progress / 12f;
        cloudStrengthMultiplier = Math.Max(0f, data.Float("cloudStrengthMultiplier", 1f));
        debrisStrengthMultiplier = Math.Max(0f, data.Float("debrisStrengthMultiplier", 1f));
        creatureStrengthMultiplier = Math.Max(0f, data.Float("creatureStrengthMultiplier", 1f));
        giygasStrengthMultiplier = Math.Max(0f, data.Float("giygasStrengthMultiplier", 1f));
        thunderStrengthMultiplier = Math.Max(0f, data.Float("thunderStrengthMultiplier", 1f));
        cutscene = data.Attr(nameof(cutscene));
        introLaunch = data.Bool("intro_launch");
        dark = data.Bool("dark");
        ambience = data.Attr(nameof(ambience));
        background = dark ? Color.Black : Calc.HexToColor("1f2f4d");
        accentColor = dark
            ? Color.Lerp(Calc.HexToColor("2b0839"), Calc.HexToColor("6a0f3d"), progression)
            : Color.Lerp(Calc.HexToColor("3b5e89"), Calc.HexToColor("8d4462"), progression);
        creatureColor = dark
            ? Color.Lerp(Calc.HexToColor("460014"), Calc.HexToColor("ff6b7a"), progression)
            : Color.Lerp(Calc.HexToColor("40233d"), Calc.HexToColor("f0a8c9"), progression);
        thunderColor = dark ? Color.Lerp(Color.White, Calc.HexToColor("8fbaff"), 0.35f) : Calc.HexToColor("dceeff");

        customBackgroundTexture = TryGetTexture(PixelLabAssetRoot + "bg00");
        cloudTextures = GetAtlasSubtexturesWithFallback(
            PixelLabAssetRoot + "cloud",
            "scenery/launch/cloud"
        );
        foregroundTextures = GetCombinedTextures(
            PixelLabAssetRoot + "slice00",
            PixelLabAssetRoot + "slice01",
            PixelLabAssetRoot + "slice02"
        );
        giygasTextures = GetAtlasSubtexturesWithFallback(
            PixelLabAssetRoot + "giygas",
            "bgs/maggy/21/finaltitansummit/giygasbg"
        );
        if (giygasTextures.Count == 0)
        {
            giygasTextures = GetCombinedAtlasSubtextures(
                PixelLabAssetRoot + "glitch_a_",
                PixelLabAssetRoot + "glitch_b_",
                PixelLabAssetRoot + "glitch_c",
                PixelLabAssetRoot + "glitch_d",
                PixelLabAssetRoot + "glitch_e",
                PixelLabAssetRoot + "glitch_f",
                PixelLabAssetRoot + "glitch_g"
            );
        }
        debrisTextures = GetCombinedAtlasSubtextures(
            PixelLabAssetRoot + "debris",
            "characters/asrielgodboss/hg_debris",
            "characters/asrielgodboss/hg_debrisB",
            "characters/asrielgodboss/hg_debrisC"
        );
        if (debrisTextures.Count == 0)
        {
            debrisTextures = GetCombinedTextures(
                PixelLabAssetRoot + "Bed",
                PixelLabAssetRoot + "Car",
                PixelLabAssetRoot + "Temple",
                PixelLabAssetRoot + "Tower",
                PixelLabAssetRoot + "Cliffside",
                PixelLabAssetRoot + "Reflection",
                PixelLabAssetRoot + "floating house",
                PixelLabAssetRoot + "GiantCassete",
                PixelLabAssetRoot + "dead_creature_a",
                PixelLabAssetRoot + "dead_creature_b",
                PixelLabAssetRoot + "big_dead_creature"
            );
        }
        creatureTextures = GetCombinedAtlasSubtextures(
            PixelLabAssetRoot + "creature",
            "characters/asrielgodboss/hypergoner_mainface"
        );
        if (creatureTextures.Count == 0)
        {
            creatureTextures = GetCombinedAtlasSubtextures(
                PixelLabAssetRoot + "creature_a",
                PixelLabAssetRoot + "creature_b",
                PixelLabAssetRoot + "creature_c",
                PixelLabAssetRoot + "creature_d",
                PixelLabAssetRoot + "creature_e",
                PixelLabAssetRoot + "creature_f"
            );
        }
        birdGonerAnimations = GetBirdGonerAnimations();
        soulTextures = GetCombinedTextures(
            "characters/soul/soul/vessel_soulA",
            "characters/soul/soul/vessel_soulB",
            "characters/soul/soul/vessel_soulC",
            "characters/soul/soul/vessel_soulD",
            "characters/soul/soul/vessel_soulE",
            "characters/soul/soul/vessel_soulF",
            "characters/soul/soul/vessel_soulG"
        );
        blobTexture = GFX.Game["particles/blob"];

        clouds = new CloudParticle[stageProfile.CloudCount];
        debris = new DebrisParticle[stageProfile.DebrisCount];
        creatures = new CreaturePass[stageProfile.CreatureCount];
        giygasBands = new GiygasBand[stageProfile.GiygasBandCount];
        birdGoners = new BirdGonerPass[7];
        souls = new SoulPass[7];
        thunderPath = new Vector2[6];

        InitializeClouds();
        InitializeDebris();
        InitializeCreatures();
        InitializeGiygasBands();
        InitializeBirdGoners();
        InitializeSouls();
        thunderCooldown = Calc.Random.Range(1.6f, 4.2f);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = Scene as Level;
        Add(new Coroutine(Routine()));
    }

    private IEnumerator Routine()
    {
        FinalTitanSummitBackgroundManager manager = this;
        global::Celeste.Player currentPlayer;
        
        while ((currentPlayer = manager.Scene.Tracker.GetEntity<global::Celeste.Player>()) == null || !manager.CollideCheck(currentPlayer))
        {
            yield return null;
        }

        player = currentPlayer;

        manager.level.Session.SetFlag(GetBeginSwapFlag(progress));
        manager.level.Session.SetFlag(GetActorFlag(progress));

        currentPlayer.Sprite.Play("launch");
        currentPlayer.Speed = Vector2.Zero;
        currentPlayer.StateMachine.State = Player.StDummy;
        currentPlayer.DummyGravity = false;
        currentPlayer.DummyAutoAnimate = false;
        currentPlayer.Facing = Facings.Right;

        if (!string.IsNullOrWhiteSpace(ambience))
        {
            if (ambience.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                Audio.SetAmbience(null);
            else
                Audio.SetAmbience(SFX.EventnameByHandle(ambience));
        }

        if (manager.introLaunch)
            yield return manager.FadeTo(1f, dark ? 1.2f : 0.8f);
        else
            yield return manager.FadeTo(1f, dark ? 0.8f : 0.5f);

        yield return manager.RunManagedAscendCutscene();

        yield return manager.LaunchToNextRoom();
    }

    public override void Update()
    {
        base.Update();

        backgroundPulse += Engine.DeltaTime * (0.55f + progression * 0.8f);
        giygasPulse += Engine.DeltaTime * (0.9f + progression * 1.1f);

        UpdateClouds();
        UpdateDebris();
        UpdateCreatures();
        UpdateGiygasBands();
        UpdateBirdGoners();
        UpdateSouls();
        UpdateThunder();
    }

    public override void Render()
    {
        if (level == null)
            return;

        Vector2 camera = level.Camera.Position;
        float alpha = Ease.SineInOut(fade);
        float pulse = (float)Math.Sin(backgroundPulse) * 0.5f + 0.5f;
        Color fill = Color.Lerp(background, accentColor, (0.18f + progression * 0.25f) * pulse);
        fill = Color.Lerp(fill, thunderColor, thunderFlash * 0.45f);

        Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, fill * alpha);
        RenderBackgroundBackdrop(camera, alpha);
        RenderGiygasBackground(camera, alpha);
        RenderClouds(camera, alpha);
        RenderDebris(camera, alpha);
        RenderCreatures(camera, alpha);
        RenderSouls(camera, alpha);
        RenderBirdGoners(camera, alpha);
        RenderForeground(camera, alpha);
        RenderThunder(camera, alpha);
    }

    public override void Removed(Scene scene)
    {
        FadeSnapTo(0f);
        level?.Session.SetFlag(GetBackgroundSwapFlag(progress), false);
        level?.Session.SetFlag(GetBeginSwapFlag(progress), false);
        level?.Session.SetFlag(GetActorFlag(progress), false);
        if (level != null)
            level.CanRetry = true;

        if (player != null)
        {
            player.DummyGravity = true;
            player.DummyAutoAnimate = true;
            if (!outTheTop && player.StateMachine.State == Player.StDummy)
                player.StateMachine.State = Player.StNormal;
        }

        if (outTheTop)
            ScreenWipe.WipeColor = dark ? Color.Black : Color.White;

        ScreenWipe.WipeColor = Color.Black;
        base.Removed(scene);
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        while ((fade = Calc.Approach(fade, target, Engine.DeltaTime / duration)) != target)
        {
            FadeSnapTo(fade);
            yield return null;
        }

        FadeSnapTo(target);
    }

    private IEnumerator RunManagedAscendCutscene()
    {
        string dialogId = GetAscendCutsceneId();
        if (string.IsNullOrWhiteSpace(dialogId))
        {
            yield return 0.5f;
            yield break;
        }

        yield return 0.25f;

        global::Celeste.Cutscenes.CS20_TrueAscend ascendCutscene = new(progress, dialogId, dark);
        level.Add(ascendCutscene);
        yield return null;

        while (ascendCutscene.Running)
            yield return null;
    }

    private IEnumerator LaunchToNextRoom()
    {
        if (level == null || player == null)
            yield break;

        level.CanRetry = false;
        player.Sprite.Play("launch");
        Audio.Play("event:/new_content/char/pusheen/kirby/flynext", player.Position);
        yield return 0.25f;

        Vector2 from = player.Position;
        for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / 1f)
        {
            player.Position = Vector2.Lerp(from, from + new Vector2(0f, 60f), Ease.CubeInOut(progress)) + Calc.Random.ShakeVector();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            yield return null;
        }

        LaunchFader launchFader = new(this);
        Scene.Add(launchFader);
        from = player.Position;
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

        for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / 0.5f)
        {
            float previousY = player.Y;
            player.Position = Vector2.Lerp(from, from + new Vector2(0f, -160f), Ease.SineIn(progress));
            if (progress == 0f || Calc.OnInterval(player.Y, previousY, 16f))
                level.Add(Engine.Pooler.Create<SpeedRing>().Init(player.Center, new Vector2(0f, -1f).Angle(), Color.White));

            launchFader.Fade = progress < 0.5f ? 0f : (progress - 0.5f) * 2f;
            yield return null;
        }

        level.CanRetry = true;
        outTheTop = true;
        player.Y = level.Bounds.Top;
        player.SummitLaunch(player.X);
        player.DummyGravity = true;
        player.DummyAutoAnimate = true;
        level.Session.SetFlag(GetBackgroundSwapFlag(progress));
        level.NextTransitionDuration = 0.05f;
    }


    private void FadeSnapTo(float target)
    {
        fade = target;
        if (level != null)
            level.Bloom.Base = AreaData.Get(level).BloomBase + fade * 0.1f;
    }

    private void InitializeClouds()
    {
        for (int i = 0; i < clouds.Length; i++)
        {
            clouds[i] = new CloudParticle
            {
                Position = new Vector2(
                    Calc.Random.NextFloat(ScreenWidth + 96f) - 48f,
                    Calc.Random.NextFloat(960f)
                ),
                Speed = Calc.Random.Range(40f, 120f) * (0.8f + progression * 1.3f),
                TextureIndex = cloudTextures.Count > 0 ? Calc.Random.Next(cloudTextures.Count) : 0,
                Alpha = (Calc.Random.Range(0.12f, 0.36f) + progression * 0.1f) * GetCloudStrength(),
                Scale = Calc.Random.Range(0.5f, 1.15f),
                WaveOffset = Calc.Random.NextFloat(MathHelper.TwoPi),
                Depth = Calc.Random.Range(0.05f, 0.28f)
            };
        }
    }

    private void InitializeDebris()
    {
        for (int i = 0; i < debris.Length; i++)
        {
            ResetDebris(ref debris[i], initial: true);
        }
    }

    private void InitializeCreatures()
    {
        for (int i = 0; i < creatures.Length; i++)
        {
            ResetCreature(ref creatures[i], initial: true);
        }
    }

    private void InitializeGiygasBands()
    {
        for (int i = 0; i < giygasBands.Length; i++)
        {
            float layer = i / (float)Math.Max(1, giygasBands.Length - 1);
            giygasBands[i] = new GiygasBand
            {
                Offset = Calc.Random.NextFloat(ScreenHeight + 80f) - 40f,
                Speed = Calc.Random.Range(14f, 40f) + progression * 28f,
                Amplitude = Calc.Random.Range(10f, 38f) + progression * 22f,
                ScaleX = Calc.Random.Range(0.8f, 1.5f),
                ScaleY = Calc.Random.Range(0.4f, 1.2f),
                Phase = Calc.Random.NextFloat(MathHelper.TwoPi),
                Alpha = (Calc.Random.Range(0.08f, 0.2f) + layer * 0.15f + progression * 0.08f) * GetGiygasStrength(),
                TextureIndex = giygasTextures.Count > 0 ? Calc.Random.Next(giygasTextures.Count) : 0,
                Depth = Calc.Random.Range(0.03f, 0.2f)
            };
        }
    }

    private void InitializeBirdGoners()
    {
        for (int i = 0; i < birdGoners.Length; i++)
        {
            ResetBirdGoner(ref birdGoners[i], i, initial: true);
        }
    }

    private void InitializeSouls()
    {
        for (int i = 0; i < souls.Length; i++)
        {
            ResetSoul(ref souls[i], i, initial: true);
        }
    }

    private void UpdateClouds()
    {
        for (int i = 0; i < clouds.Length; i++)
        {
            clouds[i].Position.Y += clouds[i].Speed * Engine.DeltaTime;
            clouds[i].WaveOffset += Engine.DeltaTime * (0.4f + clouds[i].Depth * 2f);
        }
    }

    private void UpdateDebris()
    {
        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].Position += debris[i].Velocity * Engine.DeltaTime;
            debris[i].Rotation += debris[i].Spin * Engine.DeltaTime;

            if (debris[i].Position.Y > 260f || debris[i].Position.X < -96f || debris[i].Position.X > 416f)
                ResetDebris(ref debris[i], initial: false);
        }
    }

    private void UpdateCreatures()
    {
        for (int i = 0; i < creatures.Length; i++)
        {
            creatures[i].Position.Y += creatures[i].Speed * Engine.DeltaTime;
            creatures[i].BobOffset += Engine.DeltaTime * (0.7f + creatures[i].Depth * 1.4f);

            if (creatures[i].Position.Y > 250f)
                ResetCreature(ref creatures[i], initial: false);
        }
    }

    private void UpdateGiygasBands()
    {
        for (int i = 0; i < giygasBands.Length; i++)
        {
            giygasBands[i].Offset += giygasBands[i].Speed * Engine.DeltaTime;
            giygasBands[i].Phase += Engine.DeltaTime * (0.4f + progression * 1.2f);
        }
    }

    private void UpdateBirdGoners()
    {
        for (int i = 0; i < birdGoners.Length; i++)
        {
            birdGoners[i].Position.Y -= birdGoners[i].Speed * Engine.DeltaTime;
            birdGoners[i].Position.X += (float)Math.Sin(birdGoners[i].BobOffset) * birdGoners[i].Drift * Engine.DeltaTime;
            birdGoners[i].BobOffset += Engine.DeltaTime * (0.9f + birdGoners[i].Depth * 1.3f);

            List<MTexture> animation = GetBirdAnimationFrames(birdGoners[i].AnimationIndex);
            if (animation.Count > 0)
                birdGoners[i].AnimationFrame += Engine.DeltaTime * (8f + progression * 6f + i * 0.15f);

            if (birdGoners[i].Position.Y < -96f)
                ResetBirdGoner(ref birdGoners[i], i, initial: false);
        }
    }

    private void UpdateSouls()
    {
        for (int i = 0; i < souls.Length; i++)
        {
            souls[i].Position.Y -= souls[i].Speed * Engine.DeltaTime;
            souls[i].Position.X += (float)Math.Sin(souls[i].Pulse) * souls[i].Drift * Engine.DeltaTime;
            souls[i].Pulse += Engine.DeltaTime * (1.2f + souls[i].Depth * 1.6f);

            if (souls[i].Position.Y < -72f)
                ResetSoul(ref souls[i], i, initial: false);
        }
    }

    private void UpdateThunder()
    {
        thunderFlash = Calc.Approach(thunderFlash, 0f, Engine.DeltaTime * 2.6f);
        thunderAlpha = Calc.Approach(thunderAlpha, 0f, Engine.DeltaTime * 1.8f);

        if (!stageProfile.ThunderEnabled || GetThunderStrength() <= 0f || fade <= 0f)
            return;

        thunderCooldown -= Engine.DeltaTime;
        if (thunderCooldown > 0f)
            return;

        float thunderStrength = GetThunderStrength();
        thunderCooldown = Calc.Random.Range(
            Math.Max(0.6f, (3.8f - progress * 0.18f) / Math.Max(0.4f, thunderStrength)),
            Math.Max(1.0f, (6.4f - progress * 0.2f) / Math.Max(0.4f, thunderStrength))
        );
        thunderFlash = Math.Min(1.2f, 0.85f + thunderStrength * 0.25f);
        thunderAlpha = Math.Min(1f, 0.55f + thunderStrength * 0.35f);
        GenerateThunderPath();

        if (fade > 0.35f)
            Audio.Play(ThunderSfx, level.Camera.Position + new Vector2(ScreenWidth * 0.5f, 24f));
    }

    private void RenderGiygasBackground(Vector2 camera, float alpha)
    {
        float intensity = GetGiygasStrength() * alpha;
        float pulse = (float)Math.Sin(giygasPulse * 0.8f) * 0.5f + 0.5f;

        for (int i = 0; i < giygasBands.Length; i++)
        {
            GiygasBand band = giygasBands[i];
            float screenY = Mod(band.Offset - camera.Y * band.Depth, ScreenHeight + 96f) - 48f;
            float screenX = ScreenWidth * 0.5f + (float)Math.Sin(band.Phase + screenY * 0.03f) * band.Amplitude;
            Vector2 worldPos = camera + new Vector2(screenX, screenY);
            Color bandColor = Color.Lerp(accentColor, creatureColor, (float)Math.Sin(band.Phase) * 0.5f + 0.5f) * band.Alpha * intensity;
            bandColor = Color.Lerp(bandColor, thunderColor * band.Alpha, thunderFlash * 0.35f);

            if (giygasTextures.Count > 0)
            {
                MTexture tex = giygasTextures[band.TextureIndex % giygasTextures.Count];
                Vector2 scale = new Vector2(band.ScaleX * (1f + pulse * 0.35f), band.ScaleY * (0.8f + progression * 0.4f));
                tex.DrawCentered(worldPos, bandColor, scale);
            }
            else
            {
                float width = 180f * band.ScaleX * (0.8f + pulse * 0.4f);
                float height = 14f * band.ScaleY;
                Draw.Rect(worldPos - new Vector2(width * 0.5f, height * 0.5f), width, height, bandColor);
            }
        }

        for (int i = 0; i < 5 + progress / 3; i++)
        {
            float y = camera.Y + i * 38f - 22f;
            float wave = (float)Math.Sin(giygasPulse * 0.9f + i * 0.8f) * (8f + progression * 16f);
            Draw.Line(
                new Vector2(camera.X - 12f, y + wave),
                new Vector2(camera.X + 332f, y - wave),
                accentColor * intensity * (0.08f + i * 0.018f),
                2f + progression * 2f
            );
        }
    }

    private void RenderBackgroundBackdrop(Vector2 camera, float alpha)
    {
        if (customBackgroundTexture == null)
            return;

        Vector2 position = camera + new Vector2(ScreenWidth * 0.5f, ScreenHeight * 0.5f);
        float scaleX = ScreenWidth / customBackgroundTexture.Width;
        float scaleY = ScreenHeight / customBackgroundTexture.Height;
        float pulse = 0.92f + ((float)Math.Sin(backgroundPulse * 0.45f) * 0.5f + 0.5f) * 0.12f;
        Color color = Color.White * alpha * (0.35f + GetGiygasStrength() * 0.25f);
        color = Color.Lerp(color, accentColor * alpha, 0.25f + progression * 0.2f);
        customBackgroundTexture.DrawCentered(position, color, new Vector2(scaleX * pulse, scaleY * pulse));
    }

    private void RenderClouds(Vector2 camera, float alpha)
    {
        if (cloudTextures.Count == 0)
            return;

        for (int i = 0; i < clouds.Length; i++)
        {
            CloudParticle cloud = clouds[i];
            float screenX = Mod(cloud.Position.X - camera.X * cloud.Depth + (float)Math.Sin(cloud.WaveOffset) * 18f, ScreenWidth + 96f) - 48f;
            float screenY = Mod(cloud.Position.Y - camera.Y * (0.04f + cloud.Depth * 0.6f), 980f) - 380f;
            Vector2 worldPos = camera + new Vector2(screenX, screenY);
            Color color = Color.Lerp(background, Color.White, 0.7f + progression * 0.2f);
            color = Color.Lerp(color, accentColor, (float)Math.Sin(backgroundPulse + i * 0.4f) * 0.5f + 0.5f);
            color *= cloud.Alpha * alpha * GetCloudStrength();
            color = Color.Lerp(color, thunderColor * cloud.Alpha * alpha, thunderFlash * 0.22f);

            MTexture tex = cloudTextures[cloud.TextureIndex % cloudTextures.Count];
            tex.DrawCentered(worldPos, color, cloud.Scale * (0.95f + progression * 0.2f));
        }
    }

    private void RenderDebris(Vector2 camera, float alpha)
    {
        int activeCount = stageProfile.DebrisCount;
        for (int i = 0; i < activeCount; i++)
        {
            DebrisParticle piece = debris[i];
            float screenX = Mod(piece.Position.X - camera.X * piece.Depth, ScreenWidth + 120f) - 60f;
            float screenY = piece.Position.Y - camera.Y * (0.1f + piece.Depth * 0.2f);
            Vector2 worldPos = camera + new Vector2(screenX, screenY);
            Color color = Color.Lerp(accentColor, thunderColor, thunderFlash * 0.3f) * piece.Alpha * alpha * GetDebrisStrength();

            if (debrisTextures.Count > 0)
            {
                MTexture tex = debrisTextures[piece.TextureIndex % debrisTextures.Count];
                tex.DrawCentered(worldPos, color, piece.Scale, piece.Rotation);
            }
            else
            {
                blobTexture.DrawCentered(worldPos, color, new Vector2(piece.Scale * 1.3f, piece.Scale * 0.7f), piece.Rotation);
            }
        }
    }

    private void RenderCreatures(Vector2 camera, float alpha)
    {
        int activeCount = stageProfile.CreatureCount;
        for (int i = 0; i < activeCount; i++)
        {
            CreaturePass creature = creatures[i];
            float bob = (float)Math.Sin(creature.BobOffset) * (6f + progression * 12f);
            float screenX = Mod(creature.Position.X - camera.X * creature.Depth, ScreenWidth + 180f) - 90f;
            float screenY = creature.Position.Y - camera.Y * creature.Depth + bob;
            Vector2 worldPos = camera + new Vector2(screenX, screenY);
            Color color = Color.Lerp(creatureColor, thunderColor, thunderFlash * 0.18f) * creature.Alpha * alpha * GetCreatureStrength();

            if (creatureTextures.Count > 0)
            {
                MTexture tex = creatureTextures[creature.TextureIndex % creatureTextures.Count];
                tex.DrawCentered(worldPos, color, creature.Scale);
            }
            else
            {
                blobTexture.DrawCentered(worldPos, color, new Vector2(creature.Scale * 2.8f, creature.Scale * 1.4f));
                Draw.Line(worldPos + new Vector2(-12f, -2f), worldPos + new Vector2(12f, -2f), Color.Black * creature.Alpha * alpha, 2f);
            }
        }
    }

    private void RenderThunder(Vector2 camera, float alpha)
    {
        if (thunderAlpha <= 0f)
            return;

        float strength = thunderAlpha * alpha;
        Color core = thunderColor * strength;
        Color glow = accentColor * strength * (0.35f + GetThunderStrength() * 0.1f);

        for (int i = 0; i < thunderPath.Length - 1; i++)
        {
            Vector2 start = camera + thunderPath[i];
            Vector2 end = camera + thunderPath[i + 1];
            Draw.Line(start, end, glow, 4f + GetThunderStrength());
            Draw.Line(start, end, core, 2f);
        }

        Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, thunderColor * strength * 0.08f);
    }

    private void RenderSouls(Vector2 camera, float alpha)
    {
        float intensity = GetCreatureStrength() * alpha;
        if (intensity <= 0f)
            return;

        for (int i = 0; i < souls.Length; i++)
        {
            SoulPass soul = souls[i];
            float pulse = (float)Math.Sin(soul.Pulse) * 0.5f + 0.5f;
            float screenX = Mod(soul.Position.X - camera.X * soul.Depth, ScreenWidth + 140f) - 70f;
            float screenY = soul.Position.Y - camera.Y * (0.08f + soul.Depth * 0.25f);
            Vector2 worldPos = camera + new Vector2(screenX, screenY);
            Color soulColor = SoulColors[soul.ColorIndex % SoulColors.Length] * soul.Alpha * intensity * (0.7f + pulse * 0.45f);
            soulColor = Color.Lerp(soulColor, thunderColor * soul.Alpha * intensity, thunderFlash * 0.28f);

            if (soulTextures.Count > 0)
            {
                MTexture soulTex = soulTextures[soul.ColorIndex % soulTextures.Count];
                soulTex.DrawCentered(worldPos, soulColor, soul.Scale * (0.9f + pulse * 0.15f));
            }
            else
            {
                blobTexture.DrawCentered(worldPos, soulColor, new Vector2(soul.Scale * 0.8f, soul.Scale * (1.1f + pulse * 0.45f)));
                blobTexture.DrawCentered(worldPos + new Vector2(0f, 3f + pulse * 2f), soulColor * 0.55f, new Vector2(soul.Scale * 0.45f, soul.Scale * 0.7f));
            }
        }
    }

    private void RenderBirdGoners(Vector2 camera, float alpha)
    {
        float intensity = GetCreatureStrength() * alpha;
        if (intensity <= 0f || birdGonerAnimations.Count == 0)
            return;

        for (int i = 0; i < birdGoners.Length; i++)
        {
            BirdGonerPass bird = birdGoners[i];
            List<MTexture> animation = GetBirdAnimationFrames(bird.AnimationIndex);
            if (animation.Count == 0)
                continue;

            float bob = (float)Math.Sin(bird.BobOffset) * (4f + bird.Depth * 18f);
            float screenX = Mod(bird.Position.X - camera.X * bird.Depth, ScreenWidth + 180f) - 90f;
            float screenY = bird.Position.Y - camera.Y * (0.1f + bird.Depth * 0.3f) + bob;
            Vector2 worldPos = camera + new Vector2(screenX, screenY);
            int frameIndex = (int)bird.AnimationFrame % animation.Count;
            MTexture frame = animation[frameIndex];
            Color color = Color.Lerp(Color.White, SoulColors[i % SoulColors.Length], 0.22f + progression * 0.2f) * bird.Alpha * intensity;
            color = Color.Lerp(color, thunderColor * bird.Alpha * intensity, thunderFlash * 0.2f);
            Vector2 scale = new Vector2(bird.FlipX ? -bird.Scale : bird.Scale, bird.Scale);

            frame.DrawCentered(worldPos, color, scale);
        }
    }

    private void RenderForeground(Vector2 camera, float alpha)
    {
        if (foregroundTextures.Count == 0)
            return;

        for (int i = 0; i < foregroundTextures.Count; i++)
        {
            MTexture texture = foregroundTextures[i];
            float wave = (float)Math.Sin(backgroundPulse * (0.7f + i * 0.08f) + i) * (6f + i * 2f);
            float offsetY = 124f + i * 12f + wave;
            Vector2 position = camera + new Vector2(ScreenWidth * 0.5f, offsetY);
            float scaleX = ScreenWidth / texture.Width;
            float scaleY = Math.Min(2.2f, (ScreenHeight * 0.55f) / texture.Height);
            Color color = Color.Lerp(Color.White, accentColor, 0.18f + i * 0.12f) * alpha * (0.28f + i * 0.08f);
            texture.DrawCentered(position, color, new Vector2(scaleX, scaleY));
        }
    }

    private void ResetDebris(ref DebrisParticle piece, bool initial)
    {
        float spawnWidth = ScreenWidth + 120f;
        piece.Position = new Vector2(
            Calc.Random.NextFloat(spawnWidth) - 60f,
            initial ? Calc.Random.NextFloat(260f) - 40f : Calc.Random.Range(-80f, -16f)
        );
        piece.Velocity = new Vector2(
            Calc.Random.Range(-34f, 34f) * (0.5f + progression * 0.6f),
            Calc.Random.Range(55f, 120f) * (0.8f + progression * 1.2f)
        );
        piece.Rotation = Calc.Random.NextFloat(MathHelper.TwoPi);
        piece.Spin = Calc.Random.Range(-1.8f, 1.8f);
        piece.Alpha = (Calc.Random.Range(0.18f, 0.4f) + progression * 0.12f) * GetDebrisStrength();
        piece.Scale = Calc.Random.Range(0.35f, 0.95f);
        piece.TextureIndex = debrisTextures.Count > 0 ? Calc.Random.Next(debrisTextures.Count) : 0;
        piece.Depth = Calc.Random.Range(0.04f, 0.22f);
    }

    private void ResetCreature(ref CreaturePass creature, bool initial)
    {
        creature.Position = new Vector2(
            Calc.Random.NextFloat(ScreenWidth + 160f) - 80f,
            initial ? Calc.Random.NextFloat(220f) - 40f : Calc.Random.Range(-90f, -24f)
        );
        creature.Speed = Calc.Random.Range(14f, 42f) * (0.7f + progression * 1.1f);
        creature.BobOffset = Calc.Random.NextFloat(MathHelper.TwoPi);
        creature.Alpha = (Calc.Random.Range(0.12f, 0.22f) + progression * 0.1f) * GetCreatureStrength();
        creature.Scale = Calc.Random.Range(0.7f, 1.35f) + progression * 0.3f;
        creature.TextureIndex = creatureTextures.Count > 0 ? Calc.Random.Next(creatureTextures.Count) : 0;
        creature.Depth = Calc.Random.Range(0.08f, 0.24f);
    }

    private void ResetBirdGoner(ref BirdGonerPass bird, int slot, bool initial)
    {
        bird.Position = new Vector2(
            Calc.Random.NextFloat(ScreenWidth + 200f) - 100f,
            initial ? Calc.Random.NextFloat(320f) - 60f : Calc.Random.Range(210f, 320f)
        );
        bird.Speed = Calc.Random.Range(26f, 64f) * (0.8f + progression * 0.8f);
        bird.Drift = Calc.Random.Range(8f, 24f);
        bird.Alpha = Calc.Random.Range(0.22f, 0.38f) * (0.5f + progression * 0.65f);
        bird.Scale = Calc.Random.Range(0.55f, 1.05f) + progression * 0.15f;
        bird.AnimationFrame = Calc.Random.NextFloat(16f);
        bird.BobOffset = Calc.Random.NextFloat(MathHelper.TwoPi);
        bird.AnimationIndex = birdGonerAnimations.Count > 0 ? slot % birdGonerAnimations.Count : 0;
        bird.Depth = Calc.Random.Range(0.1f, 0.26f);
        bird.FlipX = Calc.Random.Chance(0.5f);
    }

    private void ResetSoul(ref SoulPass soul, int slot, bool initial)
    {
        soul.Position = new Vector2(
            Calc.Random.NextFloat(ScreenWidth + 120f) - 60f,
            initial ? Calc.Random.NextFloat(300f) - 50f : Calc.Random.Range(210f, 300f)
        );
        soul.Speed = Calc.Random.Range(20f, 48f) * (0.75f + progression * 0.75f);
        soul.Drift = Calc.Random.Range(5f, 18f);
        soul.Alpha = Calc.Random.Range(0.18f, 0.34f) * (0.55f + progression * 0.55f);
        soul.Scale = Calc.Random.Range(0.7f, 1.15f);
        soul.Pulse = Calc.Random.NextFloat(MathHelper.TwoPi);
        soul.ColorIndex = slot % SoulColors.Length;
        soul.Depth = Calc.Random.Range(0.08f, 0.24f);
    }

    private void GenerateThunderPath()
    {
        float x = Calc.Random.Range(48f, ScreenWidth - 48f);
        thunderPath[0] = new Vector2(x, -12f);

        for (int i = 1; i < thunderPath.Length; i++)
        {
            Vector2 previous = thunderPath[i - 1];
            thunderPath[i] = new Vector2(
                Math.Clamp(previous.X + Calc.Random.Range(-28f, 28f), 18f, ScreenWidth - 18f),
                previous.Y + Calc.Random.Range(20f, 36f)
            );
        }
    }

    private string GetAscendCutsceneId()
    {
        if (!string.IsNullOrWhiteSpace(cutscene))
            return cutscene;

        return progress <= 12 ? $"DZ_CH21_ASCEND_VS_ELS_{progress}" : string.Empty;
    }

    private sealed class LaunchFader : Entity
    {
        public float Fade;
        private readonly FinalTitanSummitBackgroundManager manager;

        public LaunchFader(FinalTitanSummitBackgroundManager manager)
        {
            this.manager = manager;
            Depth = -1000010;
        }

        public override void Render()
        {
            if (Fade <= 0f)
                return;

            Vector2 position = (Scene as Level).Camera.Position;
            Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, (manager.dark ? Color.Black : Color.White) * Fade);
        }
    }

    private static string GetBeginSwapFlag(int stage) => BeginSwapFlag + stage;

    private static string GetBackgroundSwapFlag(int stage) => BgSwapFlag + stage;

    private static string GetActorFlag(int stage) => $"finaltitansummit_actor_{ActorCycle[stage % ActorCycle.Length]}";

    private static StageVisualProfile GetStageProfile(int stage) => StageProfiles[Math.Clamp(stage, 0, StageProfiles.Length - 1)];

    private float GetCloudStrength() => stageProfile.CloudStrength * cloudStrengthMultiplier;

    private float GetDebrisStrength() => stageProfile.DebrisStrength * debrisStrengthMultiplier;

    private float GetCreatureStrength() => stageProfile.CreatureStrength * creatureStrengthMultiplier;

    private float GetGiygasStrength() => stageProfile.GiygasStrength * giygasStrengthMultiplier;

    private float GetThunderStrength() => stageProfile.ThunderStrength * thunderStrengthMultiplier;

    private static List<MTexture> GetAtlasSubtexturesWithFallback(string primaryPath, string fallbackPath)
    {
        List<MTexture> primary = TryGetAtlasSubtextures(primaryPath);
        if (primary.Count > 0)
            return primary;

        return TryGetAtlasSubtextures(fallbackPath);
    }

    private static List<MTexture> GetCombinedAtlasSubtextures(params string[] paths)
    {
        List<MTexture> textures = new List<MTexture>();
        for (int i = 0; i < paths.Length; i++)
        {
            textures.AddRange(TryGetAtlasSubtextures(paths[i]));
        }

        return textures;
    }

    private static List<MTexture> GetCombinedTextures(params string[] paths)
    {
        List<MTexture> textures = new List<MTexture>();
        for (int i = 0; i < paths.Length; i++)
        {
            MTexture texture = TryGetTexture(paths[i]);
            if (texture != null)
                textures.Add(texture);
        }

        return textures;
    }

    private static List<List<MTexture>> GetBirdGonerAnimations()
    {
        List<List<MTexture>> animations = new List<List<MTexture>>();
        for (int i = 0; i < BirdGonerNames.Length; i++)
        {
            List<MTexture> frames = TryGetAtlasSubtextures($"characters/birdgoner/{BirdGonerNames[i]}flyup");
            if (frames.Count > 0)
                animations.Add(frames);
        }

        return animations;
    }

    private List<MTexture> GetBirdAnimationFrames(int animationIndex)
    {
        if (birdGonerAnimations.Count == 0)
            return new List<MTexture>();

        return birdGonerAnimations[animationIndex % birdGonerAnimations.Count];
    }

    private static MTexture TryGetTexture(string path)
    {
        if (!GFX.Game.Has(path))
            return null;

        return GFX.Game[path];
    }

    private static List<MTexture> TryGetAtlasSubtextures(string path)
    {
        if (!GFX.Game.Has(path + "00"))
            return new List<MTexture>();

        return GFX.Game.GetAtlasSubtextures(path);
    }

    private static float Mod(float x, float m) => (x % m + m) % m;
}
