using System.Runtime.CompilerServices;
using Celeste.Cutscenes;
using Celeste.Utils;

namespace Celeste.Entities;

/// <summary>
/// StarJump Cutscene Control - Similar to BeyondSummitManager
/// A stage-based manager that handles the star jump section progression
/// and triggers the CS08_StarJumpEnd cutscene when complete.
/// </summary>
[CustomEntity(ids: "MaggyHelper/StarJumpControlCutscenes")]
[Tracked(true)]
public class StarJumpCutsceneControl : Entity
{
    // Progression States (similar to BeyondSummitManager)
    public enum Phases
    {
        NotStarted,
        Rising,        // Player is ascending through the star jump section
        Approaching,   // Player is approaching the top
        Cutscene,      // Cutscene is playing
        Completed      // Section complete
    }

    public Level Level;
    public Phases Phase = Phases.NotStarted;
    private global::Celeste.Player player;
    private bool cutsceneStarted;
    
    // Position tracking
    public float MinY;
    public float MaxY;
    public float MinX;
    public float MaxX;
    public float ApproachThreshold;  // Y position to trigger "approaching" phase
    
    // Camera control
    public float CameraOffsetMarker;
    public float CameraOffsetTimer;
    
    // Visual effects (rays)
    public Random Random;
    public VirtualRenderTarget BlockFill;
    public const int RAY_COUNT = 100;
    public VertexPositionColor[] Vertices = new VertexPositionColor[600];
    public int VertexCount;
    public Color RayColor = Calc.HexToColor("a3ffff") * 0.25f;
    public Ray[] Rays = new Ray[100];
    
    // Music state
    private bool musicLayersInitialized;

    public StarJumpCutsceneControl(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        base.Depth = -100;
        this.InitBlockFill();
    }

    public StarJumpCutsceneControl()
    {
        base.Depth = -100;
        this.InitBlockFill();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.Level = this.SceneAs<Level>();
        
        // Set up bounds
        this.MinY = (float)(this.Level.Bounds.Top + 80);
        this.MaxY = (float)(this.Level.Bounds.Top + 1800);
        this.MinX = (float)(this.Level.Bounds.Left + 80);
        this.MaxX = (float)(this.Level.Bounds.Right - 80);
        this.ApproachThreshold = this.MinY + 200f;  // Start "approaching" 200 pixels before top
        
        // Initialize music
        this.Level.Session.Audio.Music.Event = "event:/desolozantas/music/lvl8/starjump";
        this.Level.Session.Audio.Music.Layer(1, 1f);
        this.Level.Session.Audio.Music.Layer(2, 0.0f);
        this.Level.Session.Audio.Music.Layer(3, 0.0f);
        this.Level.Session.Audio.Music.Layer(4, 0.0f);
        this.Level.Session.Audio.Apply(false);
        this.musicLayersInitialized = true;
        
        this.Random = new Pcg32Random(666u);
        this.Add((Component)new BeforeRenderHook(new Action(this.BeforeRender)));
        
        // Check if we already completed this section
        if (this.Level.Session.GetFlag(CS08_StarJumpEnd.Flag))
        {
            this.Phase = Phases.Completed;
        }
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        this.player = scene.Tracker.GetEntity<global::Celeste.Player>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        
        this.player = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
        
        if (this.player != null)
        {
            // Update phase based on player position
            UpdatePhase();
            
            // Update music layers based on player height
            UpdateMusicLayers();
            
            // Update camera offset for star fly state
            UpdateCameraOffset();
        }
        else
        {
            // Reset music when player doesn't exist
            if (this.musicLayersInitialized)
            {
                this.Level.Session.Audio.Music.Layer(1, 1f);
                this.Level.Session.Audio.Music.Layer(2, 0.0f);
                this.Level.Session.Audio.Apply(false);
            }
        }
        
        this.UpdateBlockFill();
    }

    private void UpdatePhase()
    {
        if (this.Phase == Phases.Completed || this.cutsceneStarted)
            return;
            
        float centerY = this.player.CenterY;
        
        switch (this.Phase)
        {
            case Phases.NotStarted:
                // Start rising phase when player enters the star jump section
                if (centerY < this.MaxY)
                {
                    this.Phase = Phases.Rising;
                    OnPhaseChange(Phases.Rising);
                }
                break;
                
            case Phases.Rising:
                // Transition to approaching when player gets close to top
                if (centerY <= this.ApproachThreshold)
                {
                    this.Phase = Phases.Approaching;
                    OnPhaseChange(Phases.Approaching);
                }
                break;
                
            case Phases.Approaching:
                // Trigger cutscene when player reaches the top
                if (centerY <= this.MinY && !this.Level.Session.GetFlag(CS08_StarJumpEnd.Flag))
                {
                    this.Phase = Phases.Cutscene;
                    OnPhaseChange(Phases.Cutscene);
                    StartCutscene();
                }
                break;
                
            case Phases.Cutscene:
                // Wait for cutscene to complete
                if (this.Level.Session.GetFlag(CS08_StarJumpEnd.Flag))
                {
                    this.Phase = Phases.Completed;
                    OnPhaseChange(Phases.Completed);
                }
                break;
        }
    }

    private void OnPhaseChange(Phases newPhase)
    {
        Logger.Log(LogLevel.Info, "StarJumpCutsceneControl", $"Phase changed to: {newPhase}");
        
        switch (newPhase)
        {
            case Phases.Rising:
                // Player has entered the star jump section
                // Could add additional effects here
                break;
                
            case Phases.Approaching:
                // Player is getting close to the top
                // Increase music intensity
                this.Level.Session.Audio.Music.Layer(3, 0.5f);
                this.Level.Session.Audio.Apply(false);
                break;
                
            case Phases.Cutscene:
                // Cutscene is starting
                break;
                
            case Phases.Completed:
                // Section is complete
                break;
        }
    }

    private void StartCutscene()
    {
        if (this.cutsceneStarted)
            return;
            
        this.cutsceneStarted = true;
        
        // Find or create NPC for the cutscene
        NPC npcController = this.Scene.Entities.FindFirst<NPC>();
        if (npcController == null)
        {
            npcController = new NPC(this.player.Position);
            this.Scene.Add(npcController);
        }
        
        Vector2 playerStart = this.player.Position;
        Vector2 cameraStart = this.Level.Camera.Position;
        
        // Add the cutscene
        CS08_StarJumpEnd cutscene = new CS08_StarJumpEnd(npcController, this.player, playerStart, cameraStart);
        this.Scene.Add(cutscene);
        
        Logger.Log(LogLevel.Info, "StarJumpCutsceneControl", 
            $"CS08_StarJumpEnd cutscene triggered! PlayerStart: {playerStart}, CameraStart: {cameraStart}");
    }

    private void UpdateMusicLayers()
    {
        if (!this.musicLayersInitialized)
            return;
            
        float centerY = this.player.CenterY;
        
        // Fade music layers based on height
        this.Level.Session.Audio.Music.Layer(1, Calc.ClampedMap(centerY, this.MaxY, this.MinY, 1f, 0.0f));
        this.Level.Session.Audio.Music.Layer(2, Calc.ClampedMap(centerY, this.MaxY, this.MinY, 0.0f, 1f));
        
        // Add layer 3 intensity when approaching
        if (this.Phase == Phases.Approaching)
        {
            float approachProgress = Calc.ClampedMap(centerY, this.ApproachThreshold, this.MinY, 0.0f, 1.0f);
            this.Level.Session.Audio.Music.Layer(3, 0.5f + approachProgress * 0.5f);
        }
        
        this.Level.Session.Audio.Apply(false);
    }

    private void UpdateCameraOffset()
    {
        // StStarFly = 19
        const int StStarFly = 19;
        const float StarFlyCameraOffset = -38.4f;
        const float NormalCameraOffset = -12.8f;
        
        if ((double)this.Level.CameraOffset.Y == StarFlyCameraOffset)
        {
            if (this.player.StateMachine.State != StStarFly)
            {
                this.CameraOffsetTimer += Engine.DeltaTime;
                if ((double)this.CameraOffsetTimer >= 0.5)
                {
                    this.CameraOffsetTimer = 0.0f;
                    this.Level.CameraOffset.Y = NormalCameraOffset;
                }
            }
            else
            {
                this.CameraOffsetTimer = 0.0f;
            }
        }
        else if (this.player.StateMachine.State == StStarFly)
        {
            this.CameraOffsetTimer += Engine.DeltaTime;
            if ((double)this.CameraOffsetTimer >= 0.1)
            {
                this.CameraOffsetTimer = 0.0f;
                this.Level.CameraOffset.Y = StarFlyCameraOffset;
            }
        }
        else
        {
            this.CameraOffsetTimer = 0.0f;
        }
        
        this.CameraOffsetMarker = this.Level.Camera.Y;
    }

    #region Visual Effects (Rays)

    public void InitBlockFill()
    {
        for (int index = 0; index < this.Rays.Length; ++index)
        {
            this.Rays[index].Reset();
            this.Rays[index].Percent = Calc.Random.NextFloat();
        }
    }

    public void UpdateBlockFill()
    {
        Level scene = this.Scene as Level;
        if (scene == null) return;
        
        Vector2 vector = Calc.AngleToVector(-1.67079639f, 1f);
        Vector2 vector21 = new Vector2(-vector.Y, vector.X);
        int num1 = 0;
        
        for (int index1 = 0; index1 < this.Rays.Length; ++index1)
        {
            if ((double)this.Rays[index1].Percent >= 1.0)
                this.Rays[index1].Reset();
            this.Rays[index1].Percent += Engine.DeltaTime / this.Rays[index1].Duration;
            this.Rays[index1].Y += 8f * Engine.DeltaTime;
            
            float percent = this.Rays[index1].Percent;
            float x = this.Mod(this.Rays[index1].X - scene.Camera.X * 0.9f, 320f);
            float y = this.Mod(this.Rays[index1].Y - scene.Camera.Y * 0.7f, 580f) - 200f;
            float width = this.Rays[index1].Width;
            float length = this.Rays[index1].Length;
            Vector2 vector22 = new Vector2((float)(int)x, (float)(int)y);
            Color color = this.RayColor * Ease.CubeInOut(Calc.YoYo(percent));
            
            VertexPositionColor v1 = new VertexPositionColor(new Vector3(vector22 + vector21 * width + vector * length, 0.0f), color);
            VertexPositionColor v2 = new VertexPositionColor(new Vector3(vector22 - vector21 * width, 0.0f), color);
            VertexPositionColor v3 = new VertexPositionColor(new Vector3(vector22 + vector21 * width, 0.0f), color);
            VertexPositionColor v4 = new VertexPositionColor(new Vector3(vector22 - vector21 * width - vector * length, 0.0f), color);
            
            this.Vertices[num1++] = v1;
            this.Vertices[num1++] = v2;
            this.Vertices[num1++] = v3;
            this.Vertices[num1++] = v2;
            this.Vertices[num1++] = v3;
            this.Vertices[num1++] = v4;
        }
        this.VertexCount = num1;
    }

    public void BeforeRender()
    {
        if (this.BlockFill == null)
            this.BlockFill = VirtualContent.CreateRenderTarget("block-fill", 320, 180);
        if (this.VertexCount <= 0)
            return;
        Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)this.BlockFill);
        Engine.Graphics.GraphicsDevice.Clear(Color.Lerp(Color.Black, Color.LightSkyBlue, 0.3f));
        GFX.DrawVertices<VertexPositionColor>(Matrix.Identity, this.Vertices, this.VertexCount);
    }

    #endregion

    #region Cleanup

    public override void Removed(Scene scene)
    {
        this.Dispose();
        base.Removed(scene);
    }

    public override void SceneEnd(Scene scene)
    {
        this.Dispose();
        base.SceneEnd(scene);
    }

    public void Dispose()
    {
        if (this.BlockFill != null)
            this.BlockFill.Dispose();
        this.BlockFill = (VirtualRenderTarget)null;
    }

    #endregion

    #region Utilities

    public float Mod(float x, float m) => (x % m + m) % m;

    #endregion

    #region Ray Struct

    public struct Ray
    {
        public float X;
        public float Y;
        public float Percent;
        public float Duration;
        public float Width;
        public float Length;

        public void Reset()
        {
            this.Percent = 0.0f;
            this.X = Calc.Random.NextFloat(320f);
            this.Y = Calc.Random.NextFloat(580f);
            this.Duration = (float)(4.0 + (double)Calc.Random.NextFloat() * 8.0);
            this.Width = (float)Calc.Random.Next(8, 80);
            this.Length = (float)Calc.Random.Next(20, 200);
        }
    }

    #endregion
}




