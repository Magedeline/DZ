using FMOD.Studio;

namespace Celeste
{
  [CustomEntity(new string[] { "MaggyHelper/WaveFazeTutorialMachine", "MaggyHelper/WaveFazeMachine" })]
  [HotReloadable]
  public class WaveFazeTutorialMachine : JumpThru
{
    private Entity frontEntity;

    private Image backSprite;

    private Image frontRightSprite;

    private Image frontLeftSprite;

    private Sprite noise;

    private Sprite neon;

    private Solid frontWall;

    private float insideEase;

    private float cameraEase;

    private bool playerInside;

    private bool inCutscene;

    private Coroutine routine;

    private WaveFazePresentation presentation;

    private float interactStartZoom;

    private EventInstance snapshot;

    private EventInstance usingSfx;

    private SoundSource signSfx;

    private TalkComponent talk;
  
    public WaveFazeTutorialMachine(Vector2 position)
        : base(position, 88, safe: true)
    {
        base.Tag = Tags.TransitionUpdate;
        base.Depth = 1000;
        base.Visible = true;
        base.Hitbox.Position = new Vector2(-41f, -59f);
        
        // Try to load back sprite with fallback
        string backPath = "objects/wavedashtutorial/building_back";
        if (GFX.Game.Has(backPath))
        {
            Add(backSprite = new Image(GFX.Game[backPath]));
            backSprite.JustifyOrigin(0.5f, 1f);
            backSprite.Visible = true;
        }
        else
        {
            Logger.Log(LogLevel.Warn, "WaveFazeTutorialMachine", $"Missing texture: {backPath}");
        }
        
        // Try to load noise sprite with fallback
        string noisePath = "objects/wavedashtutorial/noise";
        if (GFX.Game.Has(noisePath + "00"))
        {
            Add(noise = new Sprite(GFX.Game, noisePath));
            noise.AddLoop("static", "", 0.05f);
            noise.Play("static");
            noise.CenterOrigin();
            noise.Position = new Vector2(0f, -30f);
            noise.Color = Color.White * 0.5f;
            noise.Visible = true;
        }
        else
        {
            Logger.Log(LogLevel.Warn, "WaveFazeTutorialMachine", $"Missing texture: {noisePath}00");
        }
        
        // Try to load front left sprite with fallback
        string frontLeftPath = "objects/wavedashtutorial/building_front_left";
        if (GFX.Game.Has(frontLeftPath))
        {
            Add(frontLeftSprite = new Image(GFX.Game[frontLeftPath]));
            frontLeftSprite.JustifyOrigin(0.5f, 1f);
            frontLeftSprite.Visible = true;
        }
        else
        {
            Logger.Log(LogLevel.Warn, "WaveFazeTutorialMachine", $"Missing texture: {frontLeftPath}");
        }
        
        Add(talk = new TalkComponent(new Rectangle(-12, -8, 24, 8), new Vector2(0f, -50f), OnInteract));
        talk.Enabled = false;
        SurfaceSoundIndex = 42;
    }

    public WaveFazeTutorialMachine(EntityData data, Vector2 position)
        : this(data.Position + position)
    {

    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(frontEntity = new Entity(Position));
        frontEntity.Tag = Tags.TransitionUpdate;
        frontEntity.Depth = -10500;
        frontEntity.Visible = true;
        
        // Try to load front right sprite with fallback
        string frontRightPath = "objects/wavedashtutorial/building_front_right";
        if (GFX.Game.Has(frontRightPath))
        {
            frontEntity.Add(frontRightSprite = new Image(GFX.Game[frontRightPath]));
            frontRightSprite.JustifyOrigin(0.5f, 1f);
            frontRightSprite.Visible = true;
        }
        else
        {
            Logger.Log(LogLevel.Warn, "WaveFazeTutorialMachine", $"Missing texture: {frontRightPath}");
        }
        
        // Try to load neon sprite with fallback
        string neonPath = "objects/wavedashtutorial/neon_";
        if (GFX.Game.Has(neonPath + "00"))
        {
            frontEntity.Add(neon = new Sprite(GFX.Game, neonPath));
            neon.AddLoop("loop", "", 0.07f);
            neon.Play("loop");
            neon.JustifyOrigin(0.5f, 1f);
            neon.Visible = true;
        }
        else
        {
            Logger.Log(LogLevel.Warn, "WaveFazeTutorialMachine", $"Missing texture: {neonPath}00");
        }
        
        scene.Add(frontWall = new Solid(Position + new Vector2(-41f, -59f), 88f, 38f, safe: true));
        frontWall.SurfaceSoundIndex = 42;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Add(signSfx = new SoundSource(new Vector2(8f, -16f), "event:/new_content/env/local/cafe_sign"));
    }

    public override void Render()
    {
        base.Render();
        // Debug: Draw a placeholder if no sprites loaded
        if (backSprite == null && frontLeftSprite == null && noise == null)
        {
            Draw.Rect(Position.X - 44f, Position.Y - 60f, 88f, 60f, Color.Purple * 0.5f);
            Draw.HollowRect(Position.X - 44f, Position.Y - 60f, 88f, 60f, Color.White);
        }
    }

    public override void Update()
    {
        base.Update();
        if (!inCutscene)
        {
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                frontWall.Collidable = true;
                bool flag = (entity.X > base.X - 37f && entity.X < base.X + 46f && entity.Y > base.Y - 58f) || frontWall.CollideCheck(entity);
                if (flag != playerInside)
                {
                    playerInside = flag;
                    if (playerInside)
                    {
                        signSfx.Stop();
                        snapshot = Audio.CreateSnapshot("snapshot:/game_10_inside_cafe");
                    }
                    else
                    {
                        signSfx.Play("event:/new_content/env/local/cafe_sign");
                        Audio.ReleaseSnapshot(snapshot);
                        snapshot = null;
                    }
                }
            }
            SceneAs<Level>().ZoomSnap(new Vector2(160f, 90f), 1f + Ease.QuadInOut(cameraEase) * 0.75f);
        }
        talk.Enabled = playerInside;
        frontWall.Collidable = !playerInside;
        insideEase = Calc.Approach(insideEase, playerInside ? 1f : 0f, Engine.DeltaTime * 4f);
        cameraEase = Calc.Approach(cameraEase, playerInside ? 1f : 0f, Engine.DeltaTime * 2f);
        
        if (frontRightSprite != null)
        {
            frontRightSprite.Color = Color.White * (1f - insideEase);
            frontRightSprite.Visible = insideEase < 1f;
        }
        if (frontLeftSprite != null)
        {
            frontLeftSprite.Color = Color.White * (1f - insideEase);
            frontLeftSprite.Visible = insideEase < 1f;
        }
        if (neon != null)
        {
            neon.Color = Color.White * (1f - insideEase);
            neon.Visible = insideEase < 1f;
        }
        if (noise != null && base.Scene.OnInterval(0.05f))
        {
            noise.Scale = Calc.Random.Choose(new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(1f, -1f), new Vector2(-1f, -1f));
        }
    }

    private void OnInteract(global::Celeste.Player player)
    {
        if (!inCutscene)
        {
            Level level = base.Scene as Level;
            if (usingSfx != null)
            {
                Audio.SetParameter(usingSfx, "end", 1f);
                Audio.Stop(usingSfx);
            }
            inCutscene = true;
            interactStartZoom = level.ZoomTarget;
            level.StartCutscene(SkipInteraction, fadeInOnSkip: true, endingChapterAfterCutscene: false, resetZoomOnSkip: false);
            Add(routine = new Coroutine(InteractRoutine(player)));
        }
    }

    private IEnumerator InteractRoutine(global::Celeste.Player player)
    {
        Level level = Scene as Level;
        player.StateMachine.State = Player.StDummy;
        player.StateMachine.Locked = true;
        yield return CutsceneEntity.CameraTo(new Vector2(X, Y - 30f) - new Vector2(160f, 90f), 0.25f, Ease.CubeOut);
        yield return level.ZoomTo(new Vector2(160f, 90f), 10f, 1f);
        usingSfx = Audio.Play("event:/state/cafe_computer_active", player.Position);
        Audio.Play("event:/new_content/game/10_farewell/cafe_computer_on", player.Position);
        Audio.Play("event:/new_content/game/10_farewell/cafe_computer_startupsfx", player.Position);
        presentation = new WaveFazePresentation();
        Scene.Add(presentation);
        while (presentation.Viewing)
        {
            yield return null;
        }
        yield return level.ZoomTo(new Vector2(160f, 90f), interactStartZoom, 1f);
        player.StateMachine.Locked = false;
        player.StateMachine.State = Player.StNormal;
        inCutscene = false;
        level.EndCutscene();
        Audio.SetAltMusic(null);
    }

    private void SkipInteraction(Level level)
    {
        Audio.SetAltMusic(null);
        inCutscene = false;
        level.ZoomSnap(new Vector2(160f, 90f), interactStartZoom);
        if (usingSfx != null)
        {
            Audio.SetParameter(usingSfx, "end", 1f);
            usingSfx.release();
        }
        if (presentation != null)
        {
            presentation.RemoveSelf();
        }
        presentation = null;
        if (routine != null)
        {
            routine.RemoveSelf();
        }
        routine = null;
        global::Celeste.Player entity = level.Tracker.GetEntity<global::Celeste.Player>();
        if (entity != null)
        {
            entity.StateMachine.Locked = false;
            entity.StateMachine.State = 0;
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Dispose();
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        Dispose();
    }

    private void Dispose()
    {
        if (usingSfx != null)
        {
            Audio.SetParameter(usingSfx, "quit", 1f);
            usingSfx.release();
            usingSfx = null;
        }
        Audio.ReleaseSnapshot(snapshot);
        snapshot = null;
    }
}
}

