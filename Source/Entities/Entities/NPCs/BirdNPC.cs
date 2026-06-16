using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities;

    [CustomEntity(ids: "DZ/BirdNPC")]
    [Monocle.Tracked]
    [HotReloadable]

public class BirdNPC : Actor
{
    public enum Modes
    {
        None,
        Sleeping,
        HoverNGrab,
        ClimbingTutorial,
        DashingTutorial,
        DreamJumpTutorial,
        SuperWallJumpTutorial,
        HyperJumpTutorial,
        FlyAway,
        MoveToNodes,
        WaitForLightningOff
    }

    public static ParticleType P_Feather;

    private static string FlownFlag = "bird_fly_away_";

    static BirdNPC()
    {
        P_Feather = new ParticleType
        {
            Source = GFX.Game["particles/feather"],
            Color = Color.White,
            Color2 = Color.Gray,
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.8f,
            LifeMax = 1.2f,
            Size = 1f,
            SizeRange = 0.5f,
            SpeedMin = 10f,
            SpeedMax = 20f,
            Direction = (float)Math.PI / 2f,
            DirectionRange = (float)Math.PI / 4f,
            Acceleration = new Vector2(0f, 10f),
            RotationMode = ParticleType.RotationModes.SameAsDirection
        };
    }

    public Facings Facing = Facings.Left;

    public Sprite Sprite;

    public Vector2 StartPosition;

    public VertexLight Light;

    public bool AutoFly;

    public EntityID EntityID;

    public bool FlyAwayUp = true;

    public float WaitForLightningPostDelay;

    public bool DisableFlapSfx;

    private Coroutine tutorialRoutine;

    private Modes mode;

    private BirdTutorialGui gui;

    private Level level;

    private Vector2[] nodes;

    private StaticMover staticMover;

    private bool onlyOnce;

    private bool onlyIfPlayerLeft;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public BirdNPC(Vector2 position, Modes mode)
        : base(position)
    {
        Add(Sprite = GFX.SpriteBank.Create("bird"));
        Sprite.Scale.X = (float)Facing;
        Sprite.UseRawDeltaTime = true;
        Sprite.OnFrameChange = [MethodImpl(MethodImplOptions.NoInlining)] (string spr) =>
        {
            if (level != null && base.X > level.Camera.Left + 64f && base.X < level.Camera.Right - 64f && (spr.Equals("peck") || spr.Equals("peckRare")) && Sprite.CurrentAnimationFrame == 6)
            {
                Audio.Play("event:/game/general/bird_peck", Position);
            }
            if (level != null && level.Session.Area.ID == 10 && !DisableFlapSfx)
            {
                FlapSfxCheck(Sprite);
            }
        };
        Add(Light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 8, 32));
        StartPosition = Position;
        SetMode(mode);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public BirdNPC(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Enum("mode", Modes.None))
    {
        EntityID = new EntityID(data.Level.Name, data.ID);
        nodes = data.NodesOffset(offset);
        onlyOnce = data.Bool("onlyOnce");
        onlyIfPlayerLeft = data.Bool("onlyIfPlayerLeft");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void SetMode(Modes mode)
    {
        this.mode = mode;
        if (tutorialRoutine != null)
        {
            tutorialRoutine.RemoveSelf();
        }
        switch (mode)
        {
            case Modes.ClimbingTutorial:
                Add(tutorialRoutine = new Coroutine(ClimbingTutorial()));
                break;
            case Modes.DashingTutorial:
                Add(tutorialRoutine = new Coroutine(DashingTutorial()));
                level.Session.SetFlag("birdfirstdash");
                break;
            case Modes.DreamJumpTutorial:
                Add(tutorialRoutine = new Coroutine(DreamJumpTutorial()));
                break;
            case Modes.SuperWallJumpTutorial:
                Add(tutorialRoutine = new Coroutine(SuperWallJumpTutorial()));
                break;
            case Modes.HyperJumpTutorial:
                Add(tutorialRoutine = new Coroutine(HyperJumpTutorial()));
                break;
            case Modes.FlyAway:
                Add(tutorialRoutine = new Coroutine(WaitRoutine()));
                break;
            case Modes.Sleeping:
                Sprite.Play("sleep");
                Facing = Facings.Right;
                break;
            case Modes.HoverNGrab:
                Sprite.Play("hover");
                break;
            case Modes.MoveToNodes:
                Add(tutorialRoutine = new Coroutine(MoveToNodesRoutine()));
                break;
            case Modes.WaitForLightningOff:
                Add(tutorialRoutine = new Coroutine(WaitForLightningOffRoutine()));
                break;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = scene as Level;
        if (mode == Modes.ClimbingTutorial && level.Session.GetLevelFlag("2"))
        {
            RemoveSelf();
        }
        else if (mode == Modes.FlyAway && level.Session.GetFlag(FlownFlag + level.Session.Level))
        {
            RemoveSelf();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (mode == Modes.SuperWallJumpTutorial)
        {
            Player entity = scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.Y < base.Y + 32f)
            {
                RemoveSelf();
            }
        }
        if (onlyIfPlayerLeft)
        {
            Player entity2 = level.Tracker.GetEntity<Player>();
            if (entity2 != null && entity2.X > base.X)
            {
                RemoveSelf();
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override bool IsRiding(Solid solid)
    {
        return base.Scene.CollideCheck(new Rectangle((int)base.X - 4, (int)base.Y, 8, 2), solid);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        Sprite.Scale.X = (float)Facing;
        base.Update();
    }

    public IEnumerator Caw()
    {
        Sprite.Play("croak");
        while (Sprite.CurrentAnimationFrame < 9)
        {
            yield return null;
        }
        Audio.Play("event:/game/general/bird_squawk", Position);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IEnumerator ShowTutorial(BirdTutorialGui gui, bool caw = false)
    {
        if (caw)
        {
            yield return Caw();
        }
        this.gui = gui;
        gui.Open = true;
        Scene.Add(gui);
        while (gui.Scale < 1f)
        {
            yield return null;
        }
    }

    public IEnumerator HideTutorial()
    {
        if (gui != null)
        {
            gui.Open = false;
            while (gui.Scale > 0f)
            {
                yield return null;
            }
            Scene.Remove(gui);
            gui = null;
        }
    }

    public IEnumerator StartleAndFlyAway()
    {
        Depth = -1000000;
        level.Session.SetFlag(FlownFlag + level.Session.Level);
        yield return Startle("event:/game/general/bird_startle");
        yield return FlyAway();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IEnumerator FlyAway(float upwardsMultiplier = 1f)
    {
        if (staticMover != null)
        {
            staticMover.RemoveSelf();
            staticMover = null;
        }
        Sprite.Play("fly");
        Facing = (Facings)(0 - Facing);
        Vector2 speed = new Vector2((int)Facing * 20, -40f * upwardsMultiplier);
        while (Y > (float)level.Bounds.Top)
        {
            speed += new Vector2((int)Facing * 140, -120f * upwardsMultiplier) * Engine.DeltaTime;
            Position += speed * Engine.DeltaTime;
            yield return null;
        }
        RemoveSelf();
    }

    private IEnumerator ClimbingTutorial()
    {
        yield return 0.25f;
        Player p = Scene.Tracker.GetEntity<Player>();
        while (Math.Abs(p.X - X) > 120f)
        {
            yield return null;
        }
        BirdTutorialGui tut1 = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("tutorial_climb"), Dialog.Clean("tutorial_hold"), BirdTutorialGui.ButtonPrompt.Grab);
        BirdTutorialGui tut2 = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("tutorial_climb"), BirdTutorialGui.ButtonPrompt.Grab, "+", new Vector2(0f, -1f));
        bool first = true;
        bool willEnd;
        do
        {
            yield return ShowTutorial(tut1, first);
            first = false;
            while (p.StateMachine.State != 1 && p.Y > Y)
            {
                yield return null;
            }
            if (p.Y > Y)
            {
                Audio.Play("event:/ui/game/tutorial_note_flip_back");
                yield return HideTutorial();
                yield return ShowTutorial(tut2);
            }
            while (p.Scene != null && (!p.OnGround() || p.StateMachine.State == Player.StClimb))
            {
                yield return null;
            }
            willEnd = p.Y <= Y + 4f;
            if (!willEnd)
            {
                Audio.Play("event:/ui/game/tutorial_note_flip_front");
            }
            yield return HideTutorial();
        }
        while (!willEnd);
        yield return StartleAndFlyAway();
    }

    private IEnumerator DashingTutorial()
    {
        Y = level.Bounds.Top;
        X += 32f;
        yield return 1f;
        global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
        Bridge bridge = Scene.Entities.FindFirst<Bridge>();
        while ((player == null || !(player.X > StartPosition.X - 36f) || !(player.Y > StartPosition.Y - 24f) || !(player.Y < StartPosition.Y + 8f)) && (!SaveData.Instance.Assists.Invincible || player == null || !(player.X > StartPosition.X - 24f) || !(player.Y > StartPosition.Y - 4f) || !(player.Y < StartPosition.Y + 48f)))
        {
            yield return null;
        }
        Scene.Add(new CS00_EndingMod(player, this, bridge));
    }

    private IEnumerator DreamJumpTutorial()
    {
        yield return ShowTutorial(new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("tutorial_dreamjump"), new Vector2(1f, 0f), "+", BirdTutorialGui.ButtonPrompt.Jump), caw: true);
        while (true)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && (entity.X > X || (Position - entity.Position).Length() < 32f))
            {
                break;
            }
            yield return null;
        }
        yield return HideTutorial();
        while (true)
        {
            Player entity2 = Scene.Tracker.GetEntity<Player>();
            if (entity2 != null && (Position - entity2.Position).Length() < 24f)
            {
                break;
            }
            yield return null;
        }
        yield return StartleAndFlyAway();
    }

    private IEnumerator SuperWallJumpTutorial()
    {
        Facing = Facings.Right;
        yield return 0.25f;
        bool caw = true;
        BirdTutorialGui tut1 = new BirdTutorialGui(this, new Vector2(0f, -16f), GFX.Gui["hyperjump/tutorial00"], Dialog.Clean("TUTORIAL_DASH"), new Vector2(0f, -1f));
        BirdTutorialGui tut2 = new BirdTutorialGui(this, new Vector2(0f, -16f), GFX.Gui["hyperjump/tutorial01"], Dialog.Clean("TUTORIAL_DREAMJUMP"));
        Player entity;
        do
        {
            yield return ShowTutorial(tut1, caw);
            Sprite.Play("idleRarePeck");
            yield return 2f;
            gui = tut2;
            gui.Open = true;
            gui.Scale = 1f;
            Scene.Add(gui);
            yield return null;
            tut1.Open = false;
            tut1.Scale = 0f;
            Scene.Remove(tut1);
            yield return 2f;
            yield return HideTutorial();
            yield return 2f;
            caw = false;
            entity = Scene.Tracker.GetEntity<Player>();
        }
        while (entity == null || !(entity.Y <= Y) || !(entity.X > X + 144f));
        yield return StartleAndFlyAway();
    }

    private IEnumerator HyperJumpTutorial()
    {
        Facing = Facings.Left;
        BirdTutorialGui tut = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("TUTORIAL_DREAMJUMP"), new Vector2(1f, 1f), "+", BirdTutorialGui.ButtonPrompt.Dash, GFX.Gui["tinyarrow"], BirdTutorialGui.ButtonPrompt.Jump);
        yield return 0.3f;
        yield return ShowTutorial(tut, caw: true);
    }

    private IEnumerator WaitRoutine()
    {
        while (!AutoFly)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && Math.Abs(entity.X - X) < 120f)
            {
                break;
            }
            yield return null;
        }
        yield return Caw();
        while (!AutoFly)
        {
            Player entity2 = Scene.Tracker.GetEntity<Player>();
            if (entity2 != null && (entity2.Center - Position).Length() < 32f)
            {
                break;
            }
            yield return null;
        }
        yield return StartleAndFlyAway();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IEnumerator Startle(string startleSound, float duration = 0.8f, Vector2? multiplier = null)
    {
        if (!multiplier.HasValue)
        {
            multiplier = new Vector2(1f, 1f);
        }
        if (!string.IsNullOrWhiteSpace(startleSound))
        {
            Audio.Play(startleSound, Position);
        }
        Dust.Burst(Position, -MathF.PI / 2f, 8, null);
        Sprite.Play("jump");
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, duration, start: true);
        tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
        {
            if (t.Eased < 0.5f && Scene.OnInterval(0.05f) && P_Feather != null)
            {
                level.Particles.Emit(P_Feather, 2, Position + Vector2.UnitY * -6f, Vector2.One * 4f);
            }
            Vector2 vector = Vector2.Lerp(new Vector2(100f, -100f) * multiplier.Value, new Vector2(20f, -20f) * multiplier.Value, t.Eased);
            vector.X *= 0 - Facing;
            Position += vector * Engine.DeltaTime;
        };
        Add(tween);
        while (tween.Active)
        {
            yield return null;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IEnumerator FlyTo(Vector2 target, float durationMult = 1f, bool relocateSfx = true)
    {
        Sprite.Play("fly");
        if (relocateSfx)
        {
            Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_relocate"));
        }
        int num = Math.Sign(target.X - X);
        if (num != 0)
        {
            Facing = (Facings)num;
        }
        Vector2 position = Position;
        Vector2 vector = target;
        SimpleCurve curve = new SimpleCurve(position, vector, position + (vector - position) * 0.75f - Vector2.UnitY * 30f);
        float duration = (vector - position).Length() / 100f * durationMult;
        for (float p = 0f; p < 0.95f; p += Engine.DeltaTime / duration)
        {
            Position = curve.GetPoint(Ease.SineInOut(p)).Floor();
            Sprite.Rate = 1f - p * 0.5f;
            yield return null;
        }
        Dust.Burst(Position, -MathF.PI / 2f, 8, null);
        Position = target;
        Facing = Facings.Left;
        Sprite.Rate = 1f;
        Sprite.Play("idle");
    }

    private IEnumerator MoveToNodesRoutine()
    {
        int index = 0;
        while (true)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || !((entity.Center - Position).Length() < 80f))
            {
                yield return null;
                continue;
            }
            Depth = -1000000;
            yield return Startle("event:/new_content/game/10_farewell/bird_startle", 0.2f);
            if (index < nodes.Length)
            {
                yield return FlyTo(nodes[index], 0.6f);
                index++;
                continue;
            }
            Tag = Tags.Persistent;
            Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_relocate"));
            if (onlyOnce)
            {
                level.Session.DoNotLoad.Add(EntityID);
            }
            Sprite.Play("fly");
            Facing = Facings.Right;
            Vector2 speed = new Vector2((int)Facing * 20, -40f);
            while (Y > (float)(level.Bounds.Top - 200))
            {
                speed += new Vector2((int)Facing * 140, -60f) * Engine.DeltaTime;
                Position += speed * Engine.DeltaTime;
                yield return null;
            }
            RemoveSelf();
        }
    }

    private IEnumerator WaitForLightningOffRoutine()
    {
        Sprite.Play("hoverStressed");
        while (Scene.Entities.FindFirst<Lightning>() != null)
        {
            yield return null;
        }
        if (WaitForLightningPostDelay > 0f)
        {
            yield return WaitForLightningPostDelay;
        }
        if (!FlyAwayUp)
        {
            Sprite.Play("fly");
            Vector2 speed = new Vector2((int)Facing * 20, -10f);
            while (Y > (float)level.Bounds.Top)
            {
                speed += new Vector2((int)Facing * 140, -10f) * Engine.DeltaTime;
                Position += speed * Engine.DeltaTime;
                yield return null;
            }
        }
        else
        {
            Sprite.Play("flyup");
            Vector2 speed = new Vector2(0f, -32f);
            while (Y > (float)level.Bounds.Top)
            {
                speed += new Vector2(0f, -100f) * Engine.DeltaTime;
                Position += speed * Engine.DeltaTime;
                yield return null;
            }
        }
        RemoveSelf();
    }

    public override void SceneEnd(Scene scene)
    {
        Engine.TimeRate = 1f;
        base.SceneEnd(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        if (mode == Modes.DashingTutorial)
        {
            float x = StartPosition.X - 36f;
            float x2 = level.Bounds.Right;
            float y = StartPosition.Y - 24f;
            float y2 = StartPosition.Y + 8f;
            Draw.Line(new Vector2(x, y), new Vector2(x, y2), Color.Aqua);
            Draw.Line(new Vector2(x, y), new Vector2(x2, y), Color.Aqua);
            Draw.Line(new Vector2(x2, y), new Vector2(x2, y2), Color.Aqua);
            Draw.Line(new Vector2(x, y2), new Vector2(x2, y2), Color.Aqua);
            float x3 = StartPosition.X - 24f;
            float x4 = level.Bounds.Right;
            float y3 = StartPosition.Y - 4f;
            float y4 = StartPosition.Y + 48f;
            Draw.Line(new Vector2(x3, y3), new Vector2(x3, y4), Color.Aqua);
            Draw.Line(new Vector2(x3, y3), new Vector2(x4, y3), Color.Aqua);
            Draw.Line(new Vector2(x4, y3), new Vector2(x4, y4), Color.Aqua);
            Draw.Line(new Vector2(x3, y4), new Vector2(x4, y4), Color.Aqua);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void FlapSfxCheck(Sprite sprite)
    {
        if (sprite.Entity != null && sprite.Entity.Scene != null)
        {
            Camera camera = (sprite.Entity.Scene as Level).Camera;
            Vector2 renderPosition = sprite.RenderPosition;
            if (renderPosition.X < camera.X - 32f || renderPosition.Y < camera.Y - 32f || renderPosition.X > camera.X + 320f + 32f || renderPosition.Y > camera.Y + 180f + 32f)
            {
                return;
            }
        }
        string currentAnimationID = sprite.CurrentAnimationID;
        int currentAnimationFrame = sprite.CurrentAnimationFrame;
        if ((currentAnimationID == "hover" && currentAnimationFrame == 0) || (currentAnimationID == "hoverStressed" && currentAnimationFrame == 0) || (currentAnimationID == "fly" && currentAnimationFrame == 0))
        {
            Audio.Play("event:/new_content/game/10_farewell/bird_wingflap", sprite.RenderPosition);
        }
    }
}

