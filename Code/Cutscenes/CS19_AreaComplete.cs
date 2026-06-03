using System.Runtime.CompilerServices;

namespace Celeste.Cutscenes;

/// <summary>
/// Handles the area complete sequence after the final cutscene.
/// Fades to white, shows the area complete screen, fades out, and transitions to the next chapter.
/// When used in chapter 19 (area ID 21) without a specified next level, automatically transitions to chapter 20 (area ID 22).
/// </summary>
public class CS19_AreaComplete : CutsceneEntity
{
    private static readonly string Chapter19Sid = AreaModeExtender.BuildASideSID("19_Space");

    private readonly bool hasGolden;
    private readonly bool hasPinkPlatinum;
    private readonly bool skipCredits;
    private readonly string nextLevel;
    private float fadeAlpha;
    private bool showingComplete;
    private TimeRateModifier timeRateModifier;

    public CS19_AreaComplete(bool showingComplete)
    {
        this.showingComplete = showingComplete;
        Add(timeRateModifier = new TimeRateModifier(1f, false));
    }

    public CS19_AreaComplete(bool hasGoldenStrawberry = false, bool hasPinkPlatinumBerry = false, bool skipCredits = false, string nextLevelName = null, bool showingComplete = false)
        : base(fadeInOnSkip: false)
    {
        hasGolden = hasGoldenStrawberry;
        hasPinkPlatinum = hasPinkPlatinumBerry;
        this.skipCredits = skipCredits;
        nextLevel = nextLevelName;
        Depth = -10000;
        this.showingComplete = showingComplete;
        Add(timeRateModifier = new TimeRateModifier(1f, false));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(CompletionSequence()));
    }

    private IEnumerator CompletionSequence()
    {
        timeRateModifier.ResetTimeRateMultiplier();
        global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();

        {
            player.StateMachine.State = global::Celeste.Player.StDummy;
        }
        
        // Fade to white
        ScreenWipe.WipeColor = Color.White;
        FadeWipe fadeIn = new(Level, false)
        {
            Duration = 2.0f
        };
        
        for (float t = 0f; t < 2.0f; t += Engine.DeltaTime)
        {
                timeRateModifier.SetTimeRateMultiplier(3f);
            yield return null;
        }
        fadeAlpha = 1f;
        
        // Hold on white screen
        yield return 0.5f;
        
        // Mark area as complete
        if (Level != null)
        {
            Level.RegisterAreaComplete();
            showingComplete = true;
        }
        
        // Show area complete for a moment
        yield return 2.0f;
        
        // Fade out from white
        FadeWipe fadeOut = new(Level, true)
        {
            Duration = 2.0f
        };
        
        for (float t = 0f; t < 2.0f; t += Engine.DeltaTime)
        {
            fadeAlpha = 1f - Ease.SineOut(t / 2.0f);
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
                // Check if we're exiting from chapter 19 (area ID 21) and should go to chapter 20 (area ID 22)
                string currentSid = level.Session.Area.SID ?? string.Empty;
                
                // If exiting chapter 19 without a specified next level, queue chapter 20 for next launch
                if (currentSid.Equals(Chapter19Sid, StringComparison.OrdinalIgnoreCase))
                {
                    // Unlock the Void Moon on the overworld mountain
                    if (KIRBY_CELESTEModule.SaveData != null)
                    {
                        KIRBY_CELESTEModule.SaveData.VoidMoonUnlocked = true;
                        KIRBY_CELESTEModule.SaveData.PendingUnlockChapter20OnRestart = true;
                        Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "Void Moon unlocked on overworld mountain (Chapter 19 → 20 transition)");
                    }

                    // Force close so chapter 20 is only available after restart.
                    Engine.Instance.Exit();
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




