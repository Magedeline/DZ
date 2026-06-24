using System.Runtime.CompilerServices;
using Celeste.Cutscenes;
using Celeste.Utils;

namespace Celeste.Entities;

/// <summary>
/// StarJump Cutscene Control Version 2
/// Triggers CS08_StarJumpEnd cutscene when the player hits this entity vertically from below (ascending).
/// Similar to the original StarJumpCutsceneControl but with trigger-based activation.
/// </summary>
[CustomEntity(ids: "MaggyHelper/StarJumpControlCutscenesV2")]
[Tracked(true)]
public class StarJumpCutsceneControlV2 : Entity
{
    public Level Level;
    public Random Random;
    public float CameraOffsetMarker;
    public float CameraOffsetTimer;
    public VirtualRenderTarget BlockFill;
    public const int RAY_COUNT = 100;
    public VertexPositionColor[] Vertices = new VertexPositionColor[600];
    public int VertexCount;
    public Color RayColor = Calc.HexToColor("a3ffff") * 0.25f;
    public StarJumpCutsceneControlV2.Ray[] Rays = new StarJumpCutsceneControlV2.Ray[100];

    private bool cutsceneTriggered;
    private string musicEvent;
    private string cutsceneFlag;
    private float triggerHeight;
    private float triggerWidth;
    private Vector2 triggerOffset;
    private bool useCustomTriggerBox;
    
    // Track player position for vertical entry detection
    private bool playerWasBelow;
    private bool playerInsideTrigger;

    public StarJumpCutsceneControlV2(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        // Configurable properties from Loenn
        musicEvent = data.Attr("musicEvent", "event:/music/pusheen/lvl8/starjump");
        cutsceneFlag = data.Attr("cutsceneFlag", CS08_StarJumpEnd.Flag);
        triggerHeight = data.Float("triggerHeight", 32f);
        triggerWidth = data.Float("triggerWidth", 64f);
        triggerOffset = new Vector2(data.Float("triggerOffsetX", 0f), data.Float("triggerOffsetY", 0f));
        useCustomTriggerBox = data.Bool("useCustomTriggerBox", false);

        // Set up collider for vertical hit detection (player ascending into this)
        if (useCustomTriggerBox)
        {
            base.Collider = new Hitbox(triggerWidth, triggerHeight, triggerOffset.X - triggerWidth / 2f, triggerOffset.Y);
        }
        else
        {
            // Default hitbox at bottom of entity to detect player ascending
            base.Collider = new Hitbox(64f, 32f, -32f, 0f);
        }

        base.Depth = -100;
        this.InitBlockFill();
    }

    public StarJumpCutsceneControlV2() => this.InitBlockFill();

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.Level = this.SceneAs<Level>();
        
        // Set up music
        if (!string.IsNullOrEmpty(musicEvent))
        {
            this.Level.Session.Audio.Music.Event = musicEvent;
            this.Level.Session.Audio.Music.Layer(1, 1f);
            this.Level.Session.Audio.Music.Layer(2, 0.0f);
            this.Level.Session.Audio.Music.Layer(3, 0.0f);
            this.Level.Session.Audio.Music.Layer(4, 0.0f);
            this.Level.Session.Audio.Apply(false);
        }

        this.Random = new Pcg32Random(666u);
        this.Add((Component)new BeforeRenderHook(new Action(this.BeforeRender)));
        
        // Initialize tracking
        playerWasBelow = true;
        playerInsideTrigger = false;
    }

    /// <summary>
    /// Check if player entered the trigger area vertically (from below)
    /// </summary>
    private void CheckVerticalEntry(global::Celeste.Player player)
    {
        if (player == null || cutsceneTriggered || this.Level.Session.GetFlag(cutsceneFlag))
            return;

        // Get the trigger bounds
        float triggerTop = this.Y + (useCustomTriggerBox ? triggerOffset.Y : 0f);
        float triggerBottom = triggerTop + (useCustomTriggerBox ? triggerHeight : 32f);
        float triggerLeft = this.X + (useCustomTriggerBox ? triggerOffset.X - triggerWidth / 2f : -32f);
        float triggerRight = triggerLeft + (useCustomTriggerBox ? triggerWidth : 64f);

        // Check if player is within horizontal bounds
        bool withinHorizontal = player.CenterX >= triggerLeft && player.CenterX <= triggerRight;
        
        // Check if player is within vertical bounds
        bool withinVertical = player.Top <= triggerBottom && player.Bottom >= triggerTop;
        
        // Player is currently inside the trigger area
        bool isInside = withinHorizontal && withinVertical;
        
        // Check if player was below the trigger (their top was below trigger bottom)
        bool isCurrentlyBelow = player.Top > triggerBottom;
        
        // Trigger cutscene if:
        // 1. Player just entered the trigger area
        // 2. Player was previously below the trigger
        // 3. Player is moving upward (ascending)
        if (isInside && !playerInsideTrigger && playerWasBelow && player.Speed.Y < 0)
        {
            TriggerCutscene(player);
        }
        
        // Update tracking state
        playerInsideTrigger = isInside;
        if (!isInside)
        {
            playerWasBelow = isCurrentlyBelow;
        }
    }

    private void TriggerCutscene(global::Celeste.Player player)
    {
        cutsceneTriggered = true;

        // Store starting positions before modifying player state
        Vector2 playerStartPos = player.Position;
        Vector2 cameraStartPos = this.Level.Camera.Position;

        // IMMEDIATELY disable falling and gravity when entering the box
        player.Speed = Vector2.Zero;  // Stop all movement first
        player.StateMachine.State = global::Celeste.Player.StDummy;  // Put in dummy state to prevent normal physics
        player.DummyGravity = false;  // Disable gravity completely
        player.DummyFriction = false;  // Disable friction
        player.DummyAutoAnimate = false;
        player.ForceCameraUpdate = false;
        
        // Now set up for the star fly cutscene
        player.Speed = new Vector2(0f, -80f);  // Upward momentum matching cutscene
        player.StateMachine.State = Player.StStarFly;  // StStarFly state
        player.Sprite.Play("fallSlow", false, false);  // Match cutscene animation
        player.Dashes = 1;
        player.Facing = Facings.Right;
        
        // Adjust camera for star fly
        this.Level.CameraOffset.Y = -30f;  // Match CS08_StarJumpEnd camera offset
        
        // Rumble feedback
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        
        // Create visual effect
        this.Level.Displacement.AddBurst(player.Center, 0.5f, 8f, 32f, 0.5f, null, null);
        Audio.Play("event:/game/06_reflection/feather_state_start", player.Position);

        // Find or create NPC for the cutscene (StarJumpController)
        // CS08_StarJumpEnd uses this as the starJumpController parameter
        NPC npcController = this.Scene.Entities.FindFirst<NPC>();
        if (npcController == null)
        {
            // Create a dummy NPC at player position if none exists
            npcController = new NPC(player.Position);
            this.Scene.Add(npcController);
        }
        
        // Add the CS08_StarJumpEnd cutscene with proper parameters
        // The cutscene will handle the rest: camera movement, character spawning, dialogue, etc.
        CS08_StarJumpEnd cutscene = new CS08_StarJumpEnd(npcController, player, playerStartPos, cameraStartPos);
        this.Scene.Add(cutscene);
        
        // Set the cutscene flag so it doesn't trigger again
        this.Level.Session.SetFlag(cutsceneFlag, true);
        
        Logger.Log(LogLevel.Info, "StarJumpCutsceneControlV2", 
            $"CS08_StarJumpEnd cutscene triggered! PlayerStart: {playerStartPos}, CameraStart: {cameraStartPos}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        global::Celeste.Player entity = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
        
        if (entity != null)
        {
            // Check for vertical entry to trigger cutscene
            CheckVerticalEntry(entity);
            
            float centerY = entity.CenterY;
            float minY = (float)(this.Level.Bounds.Top + 80);
            float maxY = (float)(this.Level.Bounds.Top + 1800);
            
            // Update music layers based on player height
            this.Level.Session.Audio.Music.Layer(1, Calc.ClampedMap(centerY, maxY, minY, 1f, 0.0f));
            this.Level.Session.Audio.Music.Layer(2, Calc.ClampedMap(centerY, maxY, minY));
            this.Level.Session.Audio.Apply(false);

            // Camera offset adjustments for star jump state (state 19)
            // Use -30f to match CS08_StarJumpEnd cutscene camera offset
            if ((double)this.Level.CameraOffset.Y == -30.0)
            {
                if (entity.StateMachine.State != 19)
                {
                    this.CameraOffsetTimer += Engine.DeltaTime;
                    if ((double)this.CameraOffsetTimer >= 0.5)
                    {
                        this.CameraOffsetTimer = 0.0f;
                        this.Level.CameraOffset.Y = -12.8f;
                    }
                }
                else
                    this.CameraOffsetTimer = 0.0f;
            }
            else if (entity.StateMachine.State == Player.StStarFly)
            {
                this.CameraOffsetTimer += Engine.DeltaTime;
                if ((double)this.CameraOffsetTimer >= 0.10000000149011612)
                {
                    this.CameraOffsetTimer = 0.0f;
                    this.Level.CameraOffset.Y = -30f;
                }
            }
            else
                this.CameraOffsetTimer = 0.0f;
            
            this.CameraOffsetMarker = this.Level.Camera.Y;
        }
        else
        {
            this.Level.Session.Audio.Music.Layer(1, 1f);
            this.Level.Session.Audio.Music.Layer(2, 0.0f);
            this.Level.Session.Audio.Apply(false);
        }
        
        this.UpdateBlockFill();
    }

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
            
            VertexPositionColor vertexPositionColor1 = new VertexPositionColor(new Vector3(vector22 + vector21 * width + vector * length, 0.0f), color);
            VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(vector22 - vector21 * width, 0.0f), color);
            VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(vector22 + vector21 * width, 0.0f), color);
            VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(vector22 - vector21 * width - vector * length, 0.0f), color);
            
            this.Vertices[num1++] = vertexPositionColor1;
            this.Vertices[num1++] = vertexPositionColor2;
            this.Vertices[num1++] = vertexPositionColor3;
            this.Vertices[num1++] = vertexPositionColor2;
            this.Vertices[num1++] = vertexPositionColor3;
            this.Vertices[num1++] = vertexPositionColor4;
        }
        this.VertexCount = num1;
    }

    public void BeforeRender()
    {
        if (this.BlockFill == null)
            this.BlockFill = VirtualContent.CreateRenderTarget("block-fill-v2", 320, 180);
        if (this.VertexCount <= 0)
            return;
        Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)this.BlockFill);
        Engine.Graphics.GraphicsDevice.Clear(Color.Lerp(Color.Black, Color.LightSkyBlue, 0.3f));
        GFX.DrawVertices<VertexPositionColor>(Matrix.Identity, this.Vertices, this.VertexCount);
    }

    public override void Render()
    {
        base.Render();
        
        // Draw debug hitbox in editor/debug mode
        if (this.Collider != null)
        {
            Draw.HollowRect(this.Collider, Color.Cyan * 0.5f);
        }
    }

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

    public float Mod(float x, float m) => (x % m + m) % m;

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
}

