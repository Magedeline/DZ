namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 9 Area Complete sequence.
    /// Handles the full ending flow: Credits -> Sans Message -> Area Complete screen.
    /// Can be triggered after the fake save point trap sequence.
    /// </summary>
    public class CS09_AreaComplete : CutsceneEntity
    {
        public const string FlagChapter9Complete = "ch9DZ_CHapter_complete";
        
        private global::Celeste.Player player;
        private float fadeAlpha;
#pragma warning disable CS0414
        private bool showingComplete;
#pragma warning restore CS0414
        private readonly bool skipCredits;
        private readonly bool skipMessage;
        private readonly string nextLevelName;
        private readonly TimeRateModifier timeRateModifier;
        
        public CS09_AreaComplete(
            global::Celeste.Player player = null,
            bool skipCredits = false,
            bool skipMessage = false,
            string nextLevelName = null) : base(fadeInOnSkip: false)
        {
            this.player = player;
            this.skipCredits = skipCredits;
            this.skipMessage = skipMessage;
            this.nextLevelName = nextLevelName;
            Add(timeRateModifier = new TimeRateModifier(1f, false));
            Depth = -10000;
        }
        
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(CompleteSequence(level), true));
        }
        
        private IEnumerator CompleteSequence(Level level)
        {
            // Get player if not provided
            if (player == null)
            {
                player = level.Tracker.GetEntity<global::Celeste.Player>();
            }
            
            timeRateModifier.ResetTimeRateMultiplier();
            
            if (player != null)
            {
                player.StateMachine.State = Player.StDummy;
                player.Speed = Vector2.Zero;
            }
            
            // Step 1: Show Credits (unless skipped)
            if (!skipCredits && !level.Session.GetFlag(CS09_Credits.FlagCreditsComplete))
            {
                var creditsCutscene = new CS09_Credits(player);
                level.Add(creditsCutscene);
                
                // Wait for credits to complete
                while (!level.Session.GetFlag(CS09_Credits.FlagCreditsComplete))
                {
                    yield return null;
                }
                
                yield return 0.5f;
            }
            
            // Step 2: Sans Voice Message (unless skipped)
            if (!skipMessage && !level.Session.GetFlag(CS09_MessageEnd.FlagMessageComplete))
            {
                var messageCutscene = new CS09_MessageEnd(player);
                level.Add(messageCutscene);
                
                // Wait for message to complete
                while (!level.Session.GetFlag(CS09_MessageEnd.FlagMessageComplete))
                {
                    yield return null;
                }
                
                yield return 0.5f;
            }
            
            // Step 3: Area Complete sequence
            yield return AreaCompleteSequence(level);
        }
        
        private IEnumerator AreaCompleteSequence(Level level)
        {
            // Fade to white
            ScreenWipe.WipeColor = Color.White;
            FadeWipe fadeIn = new(level, false)
            {
                Duration = 2.0f
            };
            
            for (float t = 0f; t < 2.0f; t += Engine.DeltaTime)
            {
                fadeAlpha = Ease.SineIn(t / 2.0f);
                yield return null;
            }
            fadeAlpha = 1f;
            
            // Hold on white screen
            yield return 0.5f;
            
            // Mark chapter as complete
            level.Session.SetFlag(FlagChapter9Complete, true);
            level.RegisterAreaComplete();
            showingComplete = true;
            
            // Play completion sound
            Audio.Play("event:/game/07_summit/checkpoint_summit");
            
            // Show area complete for a moment
            yield return 2.5f;
            
            // Fade out from white
            FadeWipe fadeOut = new(level, true)
            {
                Duration = 2.0f
            };
            
            for (float t = 0f; t < 2.0f; t += Engine.DeltaTime)
            {
                fadeAlpha = 1f - Ease.SineOut(t / 2.0f);
                yield return null;
            }
            fadeAlpha = 0f;
            
            EndCutscene(level);
        }
        
        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
            }
            
            ScreenWipe.WipeColor = Color.White;
            
            level.OnEndOfFrame += () =>
            {
                // If we have a next level specified, teleport to it
                if (!string.IsNullOrEmpty(nextLevelName) && player != null)
                {
                    level.TeleportTo(player, nextLevelName, global::Celeste.Player.IntroTypes.Transition);
                }
                else
                {
                    // Complete the area - go to chapter select or next chapter
                    int currentAreaId = level.Session.Area.ID;
                    
                    // Check if there's a next chapter to go to
                    int nextAreaId = currentAreaId + 1;
                    
                    if (AreaData.Areas.Count > nextAreaId && AreaData.Areas[nextAreaId] != null)
                    {
                        // Transition to next chapter
                        AreaKey nextAreaKey = new(nextAreaId, AreaMode.Normal);
                        level.Session.InArea = true;
                        Engine.Scene = new LevelLoader(new Session(nextAreaKey), null);
                    }
                    else
                    {
                        // Complete area normally (return to chapter select)
                        level.CompleteArea(spotlightWipe: true, skipScreenWipe: false, skipCompleteScreen: false);
                    }
                }
            };
        }
        
        public override void Render()
        {
            base.Render();
            
            if (fadeAlpha > 0f)
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.White * fadeAlpha);
            }
        }
    }
}
