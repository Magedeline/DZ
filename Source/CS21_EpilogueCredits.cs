using Celeste.Mod.MaggyHelper;

namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Chapter 21 - Post Epilogue Credits
    /// Shows memories and alliances with dev credits
    /// </summary>
    [Tracked]
    public class CS21_EpilogueCredits : CutsceneEntity
    {
        private global::Celeste.Player player;
        private List<CreditEntry> credits = new List<CreditEntry>();
        private int currentCreditIndex = -1;
        private float creditYPosition = 1080f;
        private int previousPlayerState;
        private bool previousTimerStopped;
        private bool previousTimerHidden;
        private bool previousSaveQuitDisabled;
        private bool previousPauseLock;
        
        private struct CreditEntry
        {
            public string Title;
            public string[] Names;
            public string MemoryText;
            public bool IsMemory; // true for memories, false for credits
            
            public CreditEntry(string title, string[] names, string memory = "", bool isMemory = false)
            {
                Title = title;
                Names = names;
                MemoryText = memory;
                IsMemory = isMemory;
            }
        }
        
        private float memoryAlpha = 0f;
        
        public CS21_EpilogueCredits(global::Celeste.Player player) : base(false, true)
        {
            this.player = player;
            setupCredits();
        }
        
        private void setupCredits()
        {
            // Memories and alliances
            credits.Add(new CreditEntry(
                "The Mountain Summit",
                null,
                "Where it all began... Madeline's journey to the top.",
                true
            ));
            
            credits.Add(new CreditEntry(
                "Badeline",
                null,
                "The other side of myself. Together, we are whole.",
                true
            ));
            
            credits.Add(new CreditEntry(
                "Kirby",
                null,
                "A pink hero from another world. Small but mighty.",
                true
            ));
            
            credits.Add(new CreditEntry(
                "Asriel",
                null,
                "Lost but found. A soul seeking redemption.",
                true
            ));
            
            credits.Add(new CreditEntry(
                "The Celestial Hypergod",
                null,
                "Light and darkness united. An unstoppable force.",
                true
            ));
            
            credits.Add(new CreditEntry(
                "The Final Battle",
                null,
                "Against Els, the true enemy. We fought together.",
                true
            ));
            
            credits.Add(new CreditEntry(
                "Struggles Overcome",
                null,
                "Every challenge made us stronger. Every failure a lesson.",
                true
            ));
            
            credits.Add(new CreditEntry(
                "Bonds Forged",
                null,
                "Friendships that transcend worlds and dimensions.",
                true
            ));
            
            // Developer credits
            credits.Add(new CreditEntry(
                "Created By",
                new string[] { "DesoloZatnas Team" },
                "",
                false
            ));
            
            credits.Add(new CreditEntry(
                "Lead Developer",
                new string[] { "Magedeline" },
                "",
                false
            ));
            
            credits.Add(new CreditEntry(
                "Additional Programming",
                new string[] {
                    "Community Contributors",
                    "Everest Team"
                },
                "",
                false
            ));
            
            credits.Add(new CreditEntry(
                "Story & Design",
                new string[] {
                    "DesoloZatnas Team",
                    "Community Feedback"
                },
                "",
                false
            ));
            
            credits.Add(new CreditEntry(
                "Art & Graphics",
                new string[] {
                    "Asset Contributors",
                    "Community Artists"
                },
                "",
                false
            ));
            
            credits.Add(new CreditEntry(
                "Music & Audio",
                new string[] {
                    "FMOD Audio Team",
                    "Sound Designers"
                },
                "",
                false
            ));
            
            credits.Add(new CreditEntry(
                "Special Thanks",
                new string[] {
                    "Celeste Community",
                    "Modding Community",
                    "Playtester s",
                    "All Supporters"
                },
                "",
                false
            ));
            
            credits.Add(new CreditEntry(
                "Final DLC",
                new string[] {
                    "Thank you for playing!",
                    "Your journey is complete."
                },
                "",
                false
            ));
            
            credits.Add(new CreditEntry(
                "But the adventure continues...",
                new string[] {
                    "Two worlds become one."
                },
                "",
                false
            ));
        }
        
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            
            if (player == null)
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
        }
        
        public override void OnBegin(Level level)
        {
            if (player == null)
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();

            previousTimerStopped = level.TimerStopped;
            previousTimerHidden = level.TimerHidden;
            previousSaveQuitDisabled = level.SaveQuitDisabled;
            previousPauseLock = level.PauseLock;

            level.TimerStopped = true;
            level.TimerHidden = true;
            level.SaveQuitDisabled = true;
            level.PauseLock = true;

            if (player?.StateMachine != null)
            {
                previousPlayerState = player.StateMachine.State;
                player.StateMachine.State = 11;
            }

            if (MaggyHelperModule.Session != null)
            {
                MaggyHelperModule.Session.InCredits = true;
                MaggyHelperModule.Session.CreditsPhase = 2;
                MaggyHelperModule.Session.CreditsCompleted = false;
            }

            Audio.SetAmbience(null);
            Audio.SetMusic("event:/desolozantas/music/menu/cast");
            
            Add(new Coroutine(creditsSequence(level)));
        }
        
        private IEnumerator creditsSequence(Level level)
        {
            // Fade in
            yield return 2f;
            
            // Show credits one by one
            for (int i = 0; i < credits.Count; i++)
            {
                currentCreditIndex = i;
                CreditEntry credit = credits[i];

                if (credit.IsMemory)
                {
                    // Show memory
                    yield return showMemory(credit, level);
                }
                else
                {
                    // Show developer credit
                    yield return showCredit(credit, level);
                }
                
                yield return 2f;
            }

            currentCreditIndex = -1;
            
            yield return 3f;
            
            // Fade out
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                float alpha = t / 3f;
                yield return null;
            }
            
            // Set completion flag
            level.Session.SetFlag("epilogue_credits_complete");
            
            EndCutscene(level);
        }
        
        private IEnumerator showMemory(CreditEntry memory, Level level)
        {
            memoryAlpha = 0f;

            // Fade in memory text
            for (float t = 0f; t < 1f; t += Engine.DeltaTime)
            {
                memoryAlpha = Ease.CubeOut(t);
                yield return null;
            }
            
            Audio.Play("event:/ui/postgame/memory_display");
            
            // Display memory
            yield return 3f;
            
            // Fade out
            for (float t = 0f; t < 1f; t += Engine.DeltaTime)
            {
                memoryAlpha = 1f - Ease.CubeIn(t);
                yield return null;
            }
            
            memoryAlpha = 0f;
        }
        
        private IEnumerator showCredit(CreditEntry credit, Level level)
        {
            // Scroll credit onto screen
            float targetY = 400f;
            float startY = 1080f;
            creditYPosition = startY;
            
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                creditYPosition = MathHelper.Lerp(startY, targetY, Ease.CubeOut(t / 2f));
                yield return null;
            }
            
            yield return 2f;
            
            // Scroll off screen
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                creditYPosition = MathHelper.Lerp(targetY, -200f, Ease.CubeIn(t / 2f));
                yield return null;
            }
        }
        
        public override void OnEnd(Level level)
        {
            if (player?.StateMachine != null)
                player.StateMachine.State = previousPlayerState;

            level.TimerStopped = previousTimerStopped;
            level.TimerHidden = previousTimerHidden;
            level.SaveQuitDisabled = previousSaveQuitDisabled;
            level.PauseLock = previousPauseLock;

            level.Session.SetFlag("epilogue_credits_complete");
            level.Session.SetFlag("ready_for_final_cutscene");

            if (level.Session.Area.SID == MaggyHelperModule.Chapter17EpilogueSid)
                MaggyHelperModule.MarkChapter17EpilogueCompleted();
            else if (MaggyHelperModule.Session != null)
            {
                MaggyHelperModule.Session.InCredits = false;
                MaggyHelperModule.Session.CreditsPhase = 0;
                MaggyHelperModule.Session.CreditsCompleted = true;
            }
        }
        
        public override void Render()
        {
            base.Render();
            
            // Draw black background
            Draw.Rect(0f, 0f, 1920f, 1080f, Color.Black);
            
            // Draw current credit or memory
            if (currentCreditIndex >= 0 && currentCreditIndex < credits.Count)
            {
                var current = credits[currentCreditIndex];
                
                if (current.IsMemory)
                {
                    // Render memory
                    renderMemory(current);
                }
                else
                {
                    // Render credit
                    renderCredit(current);
                }
            }
        }
        
        private void renderMemory(CreditEntry memory)
        {
            Vector2 center = new Vector2(960f, 540f);
            
            // Title
            ActiveFont.DrawOutline(
                memory.Title,
                center + new Vector2(0f, -100f),
                new Vector2(0.5f, 0.5f),
                Vector2.One * 1.5f,
                Color.Gold * memoryAlpha,
                2f,
                Color.Black * memoryAlpha
            );
            
            // Memory text
            ActiveFont.DrawOutline(
                memory.MemoryText,
                center + new Vector2(0f, 50f),
                new Vector2(0.5f, 0.5f),
                Vector2.One,
                Color.White * memoryAlpha,
                2f,
                Color.Black * memoryAlpha
            );
        }
        
        private void renderCredit(CreditEntry credit)
        {
            Vector2 position = new Vector2(960f, creditYPosition);
            
            // Title
            ActiveFont.DrawOutline(
                credit.Title,
                position,
                new Vector2(0.5f, 0.5f),
                Vector2.One * 1.2f,
                Color.Cyan,
                2f,
                Color.Black
            );
            
            // Names
            if (credit.Names != null)
            {
                float yOffset = 60f;
                foreach (var name in credit.Names)
                {
                    ActiveFont.DrawOutline(
                        name,
                        position + new Vector2(0f, yOffset),
                        new Vector2(0.5f, 0.5f),
                        Vector2.One,
                        Color.White,
                        2f,
                        Color.Black
                    );
                    yOffset += 40f;
                }
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            // Update credit index based on scroll position
            // This is handled by the coroutine
        }
    }
}




