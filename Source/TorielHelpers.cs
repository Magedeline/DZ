namespace Celeste.NPCs;

/// <summary>
/// Base class for Toriel NPCs with shared constants and helper methods
/// </summary>
public static class TorielHelpers
{
    /// <summary>
    /// Maximum movement speed for Toriel NPC
    /// </summary>
    public const float TorielMaxSpeed = 60f;
    
    /// <summary>
    /// Default walk animation ID
    /// </summary>
    public const string TorielWalkAnim = "walk";
    
    /// <summary>
    /// Default idle animation ID
    /// </summary>
    public const string TorielIdleAnim = "idle";
    
    /// <summary>
    /// Sets up sound effects for Toriel's sprite animations
    /// </summary>
    public static void SetupTorielSpriteSounds(this NPC npc)
    {
        if (npc.Sprite == null) return;
        
        // Add footstep sounds to walk animation
        npc.Sprite.OnFrameChange = (string anim) =>
        {
            if (anim == "walk" && (npc.Sprite.CurrentAnimationFrame == 0 || npc.Sprite.CurrentAnimationFrame == 4))
            {
                Audio.Play("event:/desolozantas/char/kirby/footstep", npc.Position);
            }
        };
    }
    
    /// <summary>
    /// Sets up dialogue sounds for Toriel
    /// </summary>
    public static void SetupTorielDialogueSounds(this NPC npc)
    {
        // Configure dialogue voice if needed
    }
}

/// <summary>
/// Base NPC class providing common functionality for Celeste mod NPCs
/// </summary>
public class NPC : global::Celeste.NPC
{
    public new Sprite Sprite { get; protected set; }
    public new TalkComponent Talker { get; protected set; }
    public new Level Level => Scene as Level;
    public new Session Session => Level?.Session;
    public new string MoveAnim { get; set; } = "walk";
    public new string IdleAnim { get; set; } = "idle";
    public new float Maxspeed { get; set; } = 60f;
    
    protected static float TorielMaxSpeed => TorielHelpers.TorielMaxSpeed;

    public NPC(Vector2 position) : base(position)
    {
        Depth = 100;
    }

    public NPC(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = 100;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public override void Update()
    {
        base.Update();
    }

    protected void SetupTorielSpriteSounds()
    {
        TorielHelpers.SetupTorielSpriteSounds(this);
    }

    /// <summary>
    /// Moves the NPC to a target X position using the walk animation
    /// </summary>
    public IEnumerator MoveTo(float targetX, bool turnAtEnd = true)
    {
        if (Sprite == null) yield break;
        
        float dir = Math.Sign(targetX - Position.X);
        if (dir == 0) yield break;
        
        Sprite.Scale.X = dir;
        Sprite.Play(MoveAnim);
        
        while (Math.Abs(targetX - Position.X) > 2f)
        {
            Position.X += dir * Maxspeed * Engine.DeltaTime;
            yield return null;
        }
        
        Position.X = targetX;
        Sprite.Play(IdleAnim);
        
        if (turnAtEnd)
        {
            Sprite.Scale.X = -dir;
        }
    }

    /// <summary>
    /// Makes the player approach the NPC
    /// </summary>
    protected IEnumerator PlayerApproach(global::Celeste.Player player, bool turnToFace, float distance, int dir = 1)
    {
        if (player == null) yield break;
        
        player.StateMachine.State = global::Celeste.Player.StDummy;
        
        float targetX = Position.X + (distance * dir);
        while (Math.Abs(player.Position.X - targetX) > 2f)
        {
            float moveDir = Math.Sign(targetX - player.Position.X);
            player.Position.X += moveDir * 64f * Engine.DeltaTime;
            yield return null;
        }
        
        player.Position.X = targetX;
        
        if (turnToFace && Sprite != null)
        {
            Sprite.Scale.X = Math.Sign(player.Position.X - Position.X);
        }
        
        yield return 0.1f;
    }

    /// <summary>
    /// Overload for PlayerApproach with fewer parameters
    /// </summary>
    protected IEnumerator PlayerApproach(global::Celeste.Player player, bool turnToFace, float distance)
    {
        yield return PlayerApproach(player, turnToFace, distance, 1);
    }

    /// <summary>
    /// Makes the player leave the NPC area
    /// </summary>
    protected IEnumerator PlayerLeave(global::Celeste.Player player, float distance = 32f)
    {
        if (player == null) yield break;
        
        player.StateMachine.Locked = false;
        player.StateMachine.State = 0;
        
        yield return 0.1f;
    }

    /// <summary>
    /// Sets up Theo sprite sounds
    /// </summary>
    protected void SetupTheoSpriteSounds()
    {
        // Stub - override in derived classes if needed
    }

    /// <summary>
    /// Move to a position and remove the NPC when done
    /// </summary>
    public void MoveToAndRemove(Vector2 targetPosition)
    {
        Add(new Coroutine(MoveToAndRemoveRoutine(targetPosition)));
    }

    private IEnumerator MoveToAndRemoveRoutine(Vector2 targetPosition)
    {
        if (Sprite == null)
        {
            RemoveSelf();
            yield break;
        }
        
        float dir = Math.Sign(targetPosition.X - Position.X);
        if (dir != 0)
        {
            Sprite.Scale.X = dir;
            Sprite.Play(MoveAnim);
            
            while (Math.Abs(targetPosition.X - Position.X) > 2f)
            {
                Position.X += dir * Maxspeed * Engine.DeltaTime;
                yield return null;
            }
        }
        
        RemoveSelf();
    }

    /// <summary>
    /// Moves the NPC to a target Vector2 position using the walk animation
    /// </summary>
    public IEnumerator MoveTo(Vector2 targetPosition, bool turnAtEnd = true)    
    {
        yield return MoveTo(targetPosition.X, turnAtEnd);
        Position.Y = targetPosition.Y; // Set Y directly
    }

    /// <summary>
    /// Overload matching global::Celeste.NPC.MoveTo signature for compatibility
    /// </summary>
    public IEnumerator MoveTo(Vector2 targetPosition, bool walkBack, SoundSource loopSfx = null, bool turnToFace = true)
    {
        yield return MoveTo(targetPosition.X, turnToFace);
        Position.Y = targetPosition.Y;
    }
}
