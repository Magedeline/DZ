using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// Chapter 10 - Introduction Stage
/// Toriel explains the stage system with mini hearts, collectibles, and sub-maps to the party
/// </summary>
public class CS10IntroductionStage : CutsceneEntity
{
    private Player player;
    private Npc10Toriel torielNpc;
    
    private int collectedMiniHearts = 0;
    private const int MINI_HEARTS_REQUIRED = 4;
    
    private Coroutine torielWalk;
    private List<Vector2> miniHeartPositions;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS10IntroductionStage(Player player)
        : base(fadeInOnSkip: false)
    {
        this.player = player;
        base.Depth = -1000000;
        miniHeartPositions = new List<Vector2>();
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

        // Set music for exploration/ruins
        Audio.SetMusic("event:/pusheen/music/lvl10/main");

        // Fade in
        FadeWipe fadeWipe = new FadeWipe(level, wipeIn: true);
        fadeWipe.Duration = 1.5f;
        ScreenWipe.WipeColor = Color.White;
        yield return fadeWipe.Duration;
        yield return 0.8f;

        // Toriel walks to center of group
        yield return TorielApproaches();
        yield return 0.5f;

        // Main introduction stage dialog
        yield return Textbox.Say(
            "DZ_CH10_INTRODUCTION_STAGE",
            CheckFirstMiniHeart, OnKirbySelected
        );

        yield return 0.8f;

        // Now the mini-heart collection phase begins
        yield return ManageMiniHeartCollection(level);

        yield return 1f;

        EndCutscene(level);
        yield break;
    }

    /// <summary>
    /// Toriel approaches the group from the side
    /// </summary>
    private IEnumerator TorielApproaches()
    {
        if (torielNpc == null)
        {
            torielNpc = new Npc10Toriel(new EntityData(), player.Position + new Vector2(-120f, 0f));
            torielNpc.IdleAnim = "idle";
            torielNpc.MoveAnim = "walk";
            torielNpc.Maxspeed = 30f;
            torielNpc.Add(torielNpc.Sprite = GFX.SpriteBank.Create("toriel"));
            Scene.Add(torielNpc);
        }

        // Toriel walks toward the player group
        torielWalk = new Coroutine(torielNpc.MoveTo(new Vector2(player.X + 20f, torielNpc.Position.Y)));
        Add(torielWalk);
        yield return 0.6f;
        yield return null;
    }

    /// <summary>
    /// Callback when checking first mini heart
    /// </summary>
    private IEnumerator CheckFirstMiniHeart()
    {
        yield return 0.3f;
        yield return null;
    }

    /// <summary>
    /// Callback when Kirby is selected
    /// </summary>
    private IEnumerator OnKirbySelected()
    {
        yield return 0.5f;
        yield return null;
    }

    /// <summary>
    /// Manage the mini-heart collection sequence with dialog branching
    /// </summary>
    private IEnumerator ManageMiniHeartCollection(Level level)
    {
        // Initialize mini-heart positions across the stage
        InitializeMiniHearts();

        // Wait for player to collect mini hearts through gameplay
        // This is a placeholder - actual mini hearts would be collected via triggers
        collectedMiniHearts = 0;

        // Phase 1: First mini heart collected
        yield return WaitForMiniHeartCollection(1);
        if (collectedMiniHearts >= 1)
        {
            yield return Textbox.Say("DZ_CH10_ASK_TORIEL_AFTER_FIRST_MINIHEART", OnFirstHeartDialog);
            yield return 0.5f;
        }

        // Phase 2: Second mini heart collected
        yield return WaitForMiniHeartCollection(2);
        if (collectedMiniHearts >= 2)
        {
            yield return Textbox.Say("DZ_CH10_ASK_TORIEL_AFTER_SECOND_MINIHEART", OnSecondHeartDialog);
            yield return 0.5f;
        }

        // Phase 3: Third mini heart collected
        yield return WaitForMiniHeartCollection(3);
        if (collectedMiniHearts >= 3)
        {
            yield return Textbox.Say("DZ_CH10_ASK_TORIEL_AFTER_THIRD_MINIHEART", OnThirdHeartDialog);
            yield return 0.5f;
        }

        // Phase 4: Last mini heart collected
        yield return WaitForMiniHeartCollection(4);
        if (collectedMiniHearts >= 4)
        {
            yield return Textbox.Say("DZ_CH10_ASK_TORIEL_AFTER_LAST_MINIHEART", OnLastHeartDialog);
            yield return 0.8f;
        }

        // Optional: Extra mini heart
        yield return CheckForExtraMiniHeart();

        yield return null;
    }

    /// <summary>
    /// Initialize mini-heart spawn positions
    /// </summary>
    private void InitializeMiniHearts()
    {
        // Position mini hearts across different areas of the stage
        miniHeartPositions.Add(player.Position + new Vector2(100f, -50f));    // First area
        miniHeartPositions.Add(player.Position + new Vector2(200f, 30f));     // Second area
        miniHeartPositions.Add(player.Position + new Vector2(150f, 80f));     // Third area
        miniHeartPositions.Add(player.Position + new Vector2(300f, -20f));    // Fourth area
    }

    /// <summary>
    /// Wait for player to collect next mini heart
    /// </summary>
    private IEnumerator WaitForMiniHeartCollection(int targetCount)
    {
        // In actual implementation, this would check for trigger/collision events
        // For now, simulate the collection through dialog triggers
        float timeout = 30f; // 30 second timeout
        float elapsed = 0f;

        while (collectedMiniHearts < targetCount && elapsed < timeout)
        {
            yield return null;
            elapsed += Engine.DeltaTime;
        }

        // Simulate collection if not done
        if (collectedMiniHearts < targetCount)
        {
            collectedMiniHearts = targetCount;
        }

        yield return null;
    }

    /// <summary>
    /// Dialog trigger for first mini heart collected
    /// </summary>
    private IEnumerator OnFirstHeartDialog()
    {
        // Camera pans to show the stage entrance
        Add(new Coroutine(Level.ZoomTo(player.Center + new Vector2(80f, -40f), 1.2f, 1f)));
        yield return 0.8f;
        yield return null;
    }

    /// <summary>
    /// Dialog trigger for second mini heart collected
    /// </summary>
    private IEnumerator OnSecondHeartDialog()
    {
        // Brief pause, focus on stage interior
        Add(new Coroutine(Level.ZoomTo(player.Center + new Vector2(60f, 20f), 1.2f, 1f)));
        yield return 0.8f;
        yield return null;
    }

    /// <summary>
    /// Dialog trigger for third mini heart collected
    /// </summary>
    private IEnumerator OnThirdHeartDialog()
    {
        // Show satisfaction/progress
        Audio.Play("event:/ui/game/completed");
        yield return 0.5f;
        yield return null;
    }

    /// <summary>
    /// Dialog trigger for last mini heart collected
    /// </summary>
    private IEnumerator OnLastHeartDialog()
    {
        // Final preparations before moving to next stage
        Add(new Coroutine(Level.ZoomBack(1.5f)));
        yield return 0.8f;
        yield return null;
    }

    /// <summary>
    /// Check if player collected extra/bonus mini heart
    /// </summary>
    private IEnumerator CheckForExtraMiniHeart()
    {
        // This would trigger the extra difficult stage intro if collected
        if (collectedMiniHearts > MINI_HEARTS_REQUIRED)
        {
            yield return Textbox.Say("DZ_CH10_ASK_TORIEL_AFTER_EXTRA_MINIHEART", OnExtraHeartDialog);
            yield return 0.8f;
        }
        yield return null;
    }

    /// <summary>
    /// Dialog trigger for extra mini heart
    /// </summary>
    private IEnumerator OnExtraHeartDialog()
    {
        // Show warning or excitement about extra content
        if (torielNpc != null)
        {
            torielNpc.Sprite.Play("worried", false);
            yield return 0.5f;
            torielNpc.Sprite.Play("idle", false);
        }
        yield return null;
    }

    /// <summary>
    /// Public method to register mini heart collection (called from triggers)
    /// </summary>
    public void OnMiniHeartCollected()
    {
        collectedMiniHearts++;
        Audio.Play("event:/ui/game/reflect_check");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        level.TimerStopped = false;
        level.TimerHidden = false;
        level.SaveQuitDisabled = false;
    }
}
