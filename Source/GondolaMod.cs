using Vector2 = Microsoft.Xna.Framework.Vector2;
using MaggyHelper.NPCs;
using MaggyHelper.Cutscenes;
using NPC = MaggyHelper.NPCs.NPC;

namespace MaggyHelper;
[CustomEntity("MaggyHelper/Gondola")]
[Tracked]
[HotReloadable]
public class GondolaMod : Solid
{
    public float Rotation;
    public float RotationSpeed;
    public Entity LeftCliffside;
    private Entity rightCliffside;
    private Entity back;
    private Image backImg;
    private readonly Sprite front;
    public readonly Sprite Lever;
    private readonly Image top;
    private bool brokenLever;
    private readonly bool inCliffside;
    private readonly bool triggerCutscene;
    private bool cutsceneStarted;
    private NPC06_Magolor magolorNPC;

    public Vector2 Start { get; }

    public Vector2 Destination { get; }

    public GondolaMod(EntityData data, Vector2 offset) : base(data.Position + offset, 64f, 8f, true)
    {
        EnableAssistModeChecks = false;
        Add(front = GFX.SpriteBank.Create("MaggyHelper_gondola"));
        front.Play("idle");
        front.Origin = new Vector2(front.Width / 2f, 12f);
        front.Y = -52f;
        Add(top = new Image(GFX.Game["objects/gondola/top"]));
        top.Origin = new Vector2(top.Width / 2f, 12f);
        top.Y = -52f;
        Add(Lever = new Sprite(GFX.Game, "objects/gondola/lever"));
        Lever.Add("idle", "", 0.0f, new int[1]);
        Lever.Add("pulled", "", 0.5f, "idle", 1, 1);
        Lever.Origin = new Vector2(front.Width / 2f, 12f);
        Lever.Y = -52f;
        Lever.Play("idle");
        ((Hitbox)Collider).Position.X = (float)(-(double)Collider.Width / 2.0);
        Start = Position;
        
        // Get destination from nodes or use a default offset
        if (data.Nodes != null && data.Nodes.Length > 0)
        {
            Destination = data.Nodes[0] + offset;
        }
        else
        {
            Destination = Position + new Vector2(320f, 0f); // Default destination
        }
        
        Depth = -10500;
        inCliffside = data.Bool("active", true);
        triggerCutscene = data.Bool("triggerCutscene", false);
        SurfaceSoundIndex = 28;
    }

    /// <summary>
    /// Parameterless constructor for dynamic creation in cutscenes
    /// </summary>
    public GondolaMod() : base(Vector2.Zero, 64f, 8f, true)
    {
        EnableAssistModeChecks = false;
        Add(front = GFX.SpriteBank.Create("MaggyHelper_gondola"));
        front.Play("idle");
        front.Origin = new Vector2(front.Width / 2f, 12f);
        front.Y = -52f;
        Add(top = new Image(GFX.Game["objects/gondola/top"]));
        top.Origin = new Vector2(top.Width / 2f, 12f);
        top.Y = -52f;
        Add(Lever = new Sprite(GFX.Game, "objects/gondola/lever"));
        Lever.Add("idle", "", 0.0f, new int[1]);
        Lever.Add("pulled", "", 0.5f, "idle", 1, 1);
        Lever.Origin = new Vector2(front.Width / 2f, 12f);
        Lever.Y = -52f;
        Lever.Play("idle");
        ((Hitbox)Collider).Position.X = (float)(-(double)Collider.Width / 2.0);
        Start = Position;
        Destination = Position + new Vector2(320f, 0f); // Default destination - reasonable distance
        Depth = -10500;
        inCliffside = true; // Default value - gondola starts in cliffside
        SurfaceSoundIndex = 28;
        
        Logger.Log(LogLevel.Debug, "GondolaMod", "Created gondola with parameterless constructor for cutscene use");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(back = new Entity(Position));
        back.Depth = 9000;
        backImg = new Image(GFX.Game["objects/gondola/back"]);
        backImg.Origin = new Vector2(backImg.Width / 2f, 12f);
        backImg.Y = -52f;
        back.Add(backImg);
        scene.Add(LeftCliffside = new Entity(Position + new Vector2(-124f, 0.0f)));
        Image image1 = new Image(GFX.Game["objects/gondola/cliffsideLeft"]);
        image1.JustifyOrigin(0.0f, 1f);
        LeftCliffside.Add(image1);
        LeftCliffside.Depth = 8998;
        scene.Add(rightCliffside = new Entity(Destination + new Vector2(144f, -104f)));
        Image image2 = new Image(GFX.Game["objects/gondola/cliffsideRight"]);
        image2.JustifyOrigin(0.0f, 0.5f);
        image2.Scale.X = -1f;
        rightCliffside.Add(image2);
        rightCliffside.Depth = 8998;
        scene.Add(new Rope
        {
            Gondola = this
        });
        if (!inCliffside)
        {
            Position = Destination;
            Lever.Visible = false;
            updatePositions();
            JumpThru jumpThru = new JumpThru(Position + new Vector2((float)(-(double)Width / 2.0), -36f), (int)Width, true)
            {
                SurfaceSoundIndex = 28
            };
            Scene.Add(jumpThru);
        }
        top.Rotation = Calc.Angle(Start, Destination);

        // Create NPC06_Magolor for the cutscene if triggerCutscene is enabled
        if (triggerCutscene && inCliffside)
        {
            magolorNPC = new NPC06_Magolor(Position + new Vector2(-100f, 0f));
            scene.Add(magolorNPC);
        }
    }

    public override void Update()
    {
        // Auto-trigger cutscene when player approaches the gondola
        if (triggerCutscene && inCliffside && !cutsceneStarted)
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && player.X > Left - 16f)
            {
                cutsceneStarted = true;
                NPC npc = magolorNPC ?? Scene.Entities.FindFirst<NPC>();
                if (npc != null)
                {
                    CS06_Gondola cutscene = new CS06_Gondola(npc, this, player);
                    Scene.Add(cutscene);
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "GondolaMod", "triggerCutscene: No NPC found in the scene for gondola cutscene");
                }
            }
        }

        if (inCliffside)
        {
            float num = Math.Sign(Rotation) == Math.Sign(RotationSpeed) ? 8f : 6f;
            if (Math.Abs(Rotation) < 0.5)
                num *= 0.5f;
            if (Math.Abs(Rotation) < 0.25)
                num *= 0.5f;
            RotationSpeed += -Math.Sign(Rotation) * num * Engine.DeltaTime;
            Rotation += RotationSpeed * Engine.DeltaTime;
            Rotation = Calc.Clamp(Rotation, -0.4f, 0.4f);
            if (Math.Abs(Rotation) < 0.02 && Math.Abs(RotationSpeed) < 0.2)
                Rotation = RotationSpeed = 0.0f;
        }
        updatePositions();
        base.Update();
    }

    private void updatePositions()
    {
        back.Position = Position;
        backImg.Rotation = Rotation;
        front.Rotation = Rotation;
        if (!brokenLever)
            Lever.Rotation = Rotation;
        top.Rotation = Calc.Angle(Start, Destination);
    }

    public Vector2 GetRotatedFloorPositionAt(float x, float y = 52f)
    {
        Vector2 vector = Calc.AngleToVector(Rotation + MathHelper.PiOver2, 1f);
        Vector2 vector2 = new Vector2(-vector.Y, vector.X);
        return Position + new Vector2(0.0f, -52f) + vector * y - vector2 * x;
    }

    public void BreakLever() => Add(new Coroutine(breakLeverRoutine()));

    private IEnumerator breakLeverRoutine()
    {
        brokenLever = true;
        Vector2 speed = new Vector2(240f, -130f);
        while (true)
        {
            Lever.Position += speed * Engine.DeltaTime;
            Lever.Rotation += 2f * Engine.DeltaTime;
            speed.Y += 400f * Engine.DeltaTime;
            yield return null;
        }
    }

    public void PullSides() => front.Play("pull");

    public void CancelPullSides() => front.Play("idle");

    /// <summary>
    /// Initialize gondola for cutscene use with proper positioning
    /// </summary>
    /// <param name="startPos">Starting position for the gondola</param>
    /// <param name="destPos">Destination position for the gondola</param>
    public void InitializeForCutscene(Vector2 startPos, Vector2 destPos)
    {
        Position = startPos;
        // Update the readonly properties using reflection if needed
        // Or ensure they're set properly during construction
        Logger.Log(LogLevel.Debug, "GondolaMod", $"Initialized gondola for cutscene: Start={Start}, Dest={Destination}, Current={Position}");
        
        // Reset gondola state
        Rotation = 0f;
        RotationSpeed = 0f;
        brokenLever = false;
        
        // Ensure lever is in proper state
        if (Lever != null)
        {
            Lever.Play("idle");
            Lever.Visible = true;
        }
        
        // Ensure front sprite is in proper state
        if (front != null)
        {
            front.Play("idle");
        }
    }

    /// <summary>
    /// Reset gondola to initial state for cutscene restart
    /// </summary>
    public void ResetForCutscene()
    {
        Rotation = 0f;
        RotationSpeed = 0f;
        brokenLever = false;
        Position = Start;
        
        if (Lever != null)
        {
            Lever.Play("idle");
            Lever.Visible = true;
            Lever.Position = Vector2.Zero;
            Lever.Rotation = 0f;
        }
        
        if (front != null)
        {
            front.Play("idle");
        }
        
        updatePositions();
        Logger.Log(LogLevel.Debug, "GondolaMod", "Gondola reset for cutscene");
    }

    private class Rope : Entity
    {
        public GondolaMod Gondola { get; set; }

        public Rope() => Depth = 8999;

        public override void Render()
        {
            Vector2 vector21 = (Gondola.LeftCliffside.Position + new Vector2(40f, -12f)).Floor();
            Vector2 vector22 = (Gondola.rightCliffside.Position + new Vector2(-40f, -4f)).Floor();
            Vector2 vector23 = (vector22 - vector21).SafeNormalize();
            Vector2 vector24 = Gondola.Position + new Vector2(0.0f, -55f) - vector23 * 6f;
            Vector2 vector25 = Gondola.Position + new Vector2(0.0f, -55f) + vector23 * 6f;
            for (int index = 0; index < 2; ++index)
            {
                Vector2 vector26 = Vector2.UnitY * index;
                Draw.Line(vector21 + vector26, vector24 + vector26, Color.Black);
                Draw.Line(vector25 + vector26, vector22 + vector26, Color.Black);
            }
        }
    }
}






