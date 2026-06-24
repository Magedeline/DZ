using System.Runtime.CompilerServices;

namespace Celeste.Cutscenes;

/// <summary>
/// Handles the Chapter 20 area complete sequence after the final boss.
/// Fades to white, shows the area complete screen, fades out, and transitions to the next chapter.
/// Similar to CS19_AreaComplete but specifically for the final boss chapter.
/// </summary>
public class CS20_AreaComplete : CutsceneEntity
{
    private readonly bool hasGolden;
    private readonly bool hasPinkPlatinum;
    private readonly bool skipCredits;
    private readonly string nextLevel;
    private float fadeAlpha;
    private bool showingComplete;
    private TimeRateModifier timeRateModifier;

    public CS20_AreaComplete(bool hasGoldenStrawberry = false, bool hasPinkPlatinumBerry = false, bool skipCredits = false, string nextLevelName = null)
        : base(fadeInOnSkip: false)
    {
        hasGolden = hasGoldenStrawberry;
        hasPinkPlatinum = hasPinkPlatinumBerry;
        this.skipCredits = skipCredits;
        this.nextLevel = nextLevelName;
        Depth = -10000;
        Add(timeRateModifier = new TimeRateModifier(1f, false));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        // Ensure music transitions appropriately
        Audio.SetMusic(null);
        Audio.SetAmbience(null);
        
        Add(new Coroutine(CompletionSequence()));
    }

    private IEnumerator CompletionSequence()
    {
        timeRateModifier.ResetTimeRateMultiplier();
        global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();

        {
            player.StateMachine.State = global::Celeste.Player.StDummy;
        }
        
        // Set the chapter 20 completion flag
        Level.Session.SetFlag("chapter_20_complete");
        
        // Play completion sound effect
        Audio.Play("event:/ui/game/chapter_complete");
        
        // Fade to white
        ScreenWipe.WipeColor = Color.White;
        FadeWipe fadeIn = new FadeWipe(Level, false)
        {
            Duration = 2.5f
        };
        
        for (float t = 0f; t < 2.5f; t += Engine.DeltaTime)
        {
                timeRateModifier.SetTimeRateMultiplier(3f);
            yield return null;
        }
        fadeAlpha = 1f;
        
        // Hold on white screen
        yield return 0.75f;
        
        // Mark area as complete
        if (Level != null)
        {
            Level.RegisterAreaComplete();
            showingComplete = true;
        }
        
        // Play area complete jingle
        Audio.Play("event:/ui/game/complete_area");
        
        // Show area complete for a moment
        yield return 2.5f;
        
        // Check if we should play credits
        if (!skipCredits)
        {
            // Optionally trigger credits here
            // This could be done via a flag or separate trigger
            Level.Session.SetFlag("credits_ready");
        }
        
        // Fade out from white
        FadeWipe fadeOut = new FadeWipe(Level, true)
        {
            Duration = 2.5f
        };
        
        for (float t = 0f; t < 2.5f; t += Engine.DeltaTime)
        {
            fadeAlpha = 1f - Ease.SineOut(t / 2.5f);
            yield return null;
        }
        fadeAlpha = 0f;
        
        EndCutscene(Level);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        global::Celeste.Player player = level.Tracker.GetEntity<global::Celeste.Player>();
        
        if (player != null)
        {
            player.StateMachine.State = global::Celeste.Player.StNormal;
        }
        
        // Set up wipe color
        ScreenWipe.WipeColor = Color.White;
        
        // Complete the area and transition to next level
        level.OnEndOfFrame += () =>
        {
            // If we have a next level specified, teleport to it
            if (!string.IsNullOrEmpty(nextLevel) && player != null)
            {
                level.TeleportTo(player, nextLevel, global::Celeste.Player.IntroTypes.Transition);
            }
            else
            {
                // Otherwise, complete the area normally
                if (hasGolden || hasPinkPlatinum)
                {
                    level.CompleteArea(spotlightWipe: true, skipScreenWipe: false, skipCompleteScreen: false);
                }
                else
                {
                    level.CompleteArea(spotlightWipe: false, skipScreenWipe: false, skipCompleteScreen: false);
                }
            }
        };
        
        // Clean up
        timeRateModifier.ResetTimeRateMultiplier();
        fadeAlpha = 0f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        base.Render();
        
        if (fadeAlpha > 0f)
        {
            Camera camera = Level?.Camera;
            if (camera != null)
            {
                // Full screen white fade
                Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.White * fadeAlpha);
            }
        }
    }

    public override void Update()
    {
        base.Update();
        
        // Allow skipping with certain inputs (speeds up the sequence)
        if (!skipCredits && (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed || Input.Pause.Pressed))
        {
            timeRateModifier.SetTimeRateMultiplier(3f);
        }
    }
}




