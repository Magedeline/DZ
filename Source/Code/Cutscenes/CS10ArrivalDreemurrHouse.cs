using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// Chapter 10 - Arrival at Dreemurr House Cutscene
/// Implements the arrival sequence with Madeline, Badeline, Theo, and interactions with the Dreemurr family
/// </summary>
public class CS10ArrivalDreemurrHouse : CutsceneEntity
{
    private Player player;
    private Npc10Theo theoNpc;
    private Npc10Toriel torielNpc;
    
    private Coroutine theoWalk;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS10ArrivalDreemurrHouse(Player player)
        : base(fadeInOnSkip: false)
    {
        this.player = player;
        base.Depth = -1000000;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Level obj = scene as Level;
        obj.TimerStopped = true;
        obj.TimerHidden = true;
        obj.SaveQuitDisabled = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        // Setup player state
        player.Dashes = 1;
        player.StateMachine.State = Player.StDummy;
        player.Sprite.Play("idle");

        // Set music
        Audio.SetMusic("event:/pusheen/ch10/music/fallendown", true, true);

        // Fade in
        FadeWipe fadeWipe = new FadeWipe(level, wipeIn: true);
        fadeWipe.Duration = 1.5f;
        ScreenWipe.WipeColor = Color.White;
        yield return fadeWipe.Duration;
        yield return 0.5f;

        // Trigger 0: Madeline step forward twice and zoom in to madeline
        yield return MadelineStepForwardAndZoom();
        yield return 0.8f;

        // Trigger 1: Madeline turn left
        yield return MadelineTurnLeft();
        yield return 0.5f;

        // Trigger 2: Madeline walk right and knock the door
        yield return MadelineWalkRightAndKnock();
        yield return 1f;

        // Dialog section 1
        yield return Textbox.Say(
            "MAGGYHELPER_CH10_ARRIVIAL_DREEMURR_HOUSE",
            null, TheoWalkIn, null, TorielTurnRight
            // MadelineTurnLeftThenRight and MadelineTurnRight don't exist
        );

        yield return 1.5f;
        EndCutscene(level);
        yield break;
    }

    /// <summary>
    /// Trigger 0: Madeline step forward twice and zoom in to madeline
    /// </summary>
    private IEnumerator MadelineStepForwardAndZoom()
    {
        // Zoom camera to focus on Madeline
        Add(new Coroutine(Level.ZoomTo(player.Center + new Vector2(20f, -40f), 1.5f, 2f)));
        
        // Madeline steps forward twice
        yield return player.DummyWalkToExact((int)player.X + 8, false, 0.3f);
        yield return 0.2f;
        yield return player.DummyWalkToExact((int)player.X + 8, false, 0.3f);
        yield return null;
    }

    /// <summary>
    /// Trigger 1: Madeline turn left
    /// </summary>
    private IEnumerator MadelineTurnLeft()
    {
        player.Facing = Facings.Left;
        yield return null;
    }

    /// <summary>
    /// Trigger 2: Madeline walk right and knock the door
    /// </summary>
    private IEnumerator MadelineWalkRightAndKnock()
    {
        player.Facing = Facings.Right;
        yield return player.DummyWalkToExact((int)player.X + 40, false, 0.5f);
        
        // Play knock sound effect
        Audio.Play("event:/ui/main/whoosh");
        
        // Play knock animation on player
        player.Sprite.Play("grab", false);
        yield return 0.5f;
        player.Sprite.Play("idle", false);
        yield return null;
    }

    /// <summary>
    /// Trigger 3: Theo walk in from right (called during dialog)
    /// </summary>
    private IEnumerator TheoWalkIn()
    {
        // Spawn Theo from the right side and walk toward group
        if (theoNpc == null)
        {
            theoNpc = new Npc10Theo(new EntityData(), player.Position + new Vector2(100f, 0f));
            theoNpc.IdleAnim = "idle";
            theoNpc.MoveAnim = "walk";
            theoNpc.Maxspeed = 20f;
            theoNpc.Add(theoNpc.Sprite = GFX.SpriteBank.Create("theo"));
            Scene.Add(theoNpc);
        }

        theoWalk = new Coroutine(theoNpc.MoveTo(new Vector2(player.X - 32f, theoNpc.Position.Y)));
        Add(theoWalk);
        yield return new WaitForMs(300);
        yield return null;
    }

    /// <summary>
    /// Trigger 4: Madeline turn left before turn right
    /// </summary>
    private IEnumerator MadelineTurnLeftThenRight()
    {
        player.Facing = Facings.Left;
        yield return 0.3f;
        player.Facing = Facings.Right;
        yield return null;
    }

    /// <summary>
    /// Trigger 5: Triggered Toriel turn right
    /// </summary>
    private IEnumerator TorielTurnRight()
    {
        // Spawn Toriel if not already present
        if (torielNpc == null)
        {
            torielNpc = new Npc10Toriel(new EntityData(), player.Position + new Vector2(-80f, 0f));
            torielNpc.IdleAnim = "idle";
            torielNpc.MoveAnim = "walk";
            torielNpc.Add(torielNpc.Sprite = GFX.SpriteBank.Create("toriel"));
            Scene.Add(torielNpc);
        }

        torielNpc.Facing = Facings.Right;
        yield return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        level.TimerStopped = false;
        level.TimerHidden = false;
        level.SaveQuitDisabled = false;
    }
}

/// <summary>
/// Helper class to wait for milliseconds
/// </summary>
public class WaitForMs
{
    private float remainingTime;

    public WaitForMs(float milliseconds)
    {
        remainingTime = milliseconds / 1000f;
    }
}