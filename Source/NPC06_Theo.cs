namespace MaggyHelper.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC06_Theo")]
    public class Npc06Theo : Entity
    {
        private const string baseFlagName = "theo_06_conversation";
        
        private Sprite sprite;
        private TalkComponent talker;
        private Coroutine talkRoutine;
        private bool isInteracting = false;
        private int conversationStage = 1;
        private bool isDisposed = false;

        public Npc06Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            try
            {
                // Get conversation stage from entity data if provided
                conversationStage = data.Int("conversationStage", 1);
                
                setupSprite();
                setupCollision();
                Depth = 100;
                
                IngesteLogger.Debug($"NPC06_Theo created at {Position} with conversation stage {conversationStage}");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error creating NPC06_Theo");
                throw;
            }
        }

        private void setupSprite()
        {
            try
            {
                Add(sprite = GFX.SpriteBank.Create("theo"));
                sprite.Play("idle");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC06_Theo sprite");
                // Create a fallback sprite or handle gracefully
                sprite = null;
            }
        }

        private void setupCollision()
        {
            try
            {
                Add(talker = new TalkComponent(
                    new Rectangle(-20, -8, 40, 16),
                    new Vector2(0f, -24f),
                    ontalk
                ));
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC06_Theo collision");
                talker = null;
            }
        }

        public override void Awake(Scene scene)
        {
            try
            {
                base.Awake(scene);
                IngesteLogger.Debug("NPC06_Theo awakened in scene");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error during NPC06_Theo awake");
            }
        }

        public override void Added(Scene scene)
        {
            try
            {
                base.Added(scene);
                
                if (isDisposed)
                {
                    IngesteLogger.Warn("NPC06_Theo added after disposal, skipping setup");
                    return;
                }
                
                if (Scene is Level level)
                {
                    // Check conversation progress and determine current stage
                    if (level.Session.GetFlag($"{baseFlagName}_3"))
                    {
                        // All conversations done, disable talker
                        IngesteLogger.Debug("NPC06_Theo: All conversations completed, disabling talker");
                        if (talker != null)
                        {
                            talker.Enabled = false;
                        }
                        return;
                    }
                    else if (level.Session.GetFlag($"{baseFlagName}_2"))
                    {
                        conversationStage = 3;
                        IngesteLogger.Debug("NPC06_Theo: Setting conversation stage to 3");
                    }
                    else if (level.Session.GetFlag($"{baseFlagName}_1"))
                    {
                        conversationStage = 2;
                        IngesteLogger.Debug("NPC06_Theo: Setting conversation stage to 2");
                    }
                    else
                    {
                        conversationStage = 1;
                        IngesteLogger.Debug("NPC06_Theo: Setting conversation stage to 1");
                    }
                }
                
                if (talker != null)
                {
                    talker.Enabled = true;
                }
                
                IngesteLogger.Debug($"NPC06_Theo added to scene with conversation stage {conversationStage}");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error adding NPC06_ Theo to scene");
            }
        }

        private void ontalk(global::Celeste.Player player)
        {
            if (isInteracting || isDisposed || player == null) return;
            
            try
            {
                if (Scene is Level level)
                {
                    IngesteLogger.Debug($"NPC06_Theo: Starting conversation stage {conversationStage}");
                    isInteracting = true;
                    level.StartCutscene(ontalkend);
                    Add(talkRoutine = new Coroutine(talkcoroutine(player)));
                }
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error starting NPC06_Theo conversation");
                isInteracting = false;
            }
        }

        private IEnumerator talkcoroutine(global::Celeste.Player player)
        {
            if (player == null || isDisposed)
            {
                yield break;
            }

            player.StateMachine.State = global::Celeste.Player.StDummy;

            // Use the conversation stage to determine dialog
            string dialogKey = conversationStage switch
            {
                1 => "CH6_THEO_1",
                2 => "CH6_THEO_2", 
                3 => "CH6_THEO_3",
                _ => "CH6_THEO_1"
            };

            IngesteLogger.Debug($"NPC06_Theo: Playing dialog {dialogKey}");
            
            // Yield the dialog without try-catch
            yield return Textbox.Say(dialogKey);
            
            // Handle cleanup
            endcutscene();
        }

        private void endcutscene()
        {
            try
            {
                if (Scene is Level level && !isDisposed)
                {
                    level.EndCutscene();
                    ontalkend(level);
                }
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error ending NPC06_Theo cutscene");
                isInteracting = false;
            }
        }

        private void ontalkend(Level level)
        {
            if (isDisposed) return;

            try
            {
                isInteracting = false;
                
                // Set the flag for the current conversation stage
                level.Session.SetFlag($"{baseFlagName}_{conversationStage}", true);
                IngesteLogger.Debug($"NPC06_Theo: Set flag {baseFlagName}_{conversationStage}");
                
                // Move to next conversation stage
                conversationStage++;
                
                // If we've completed all conversations, disable the talker
                if (conversationStage > 3)
                {
                    if (talker != null)
                    {
                        talker.Enabled = false;
                    }
                    IngesteLogger.Debug("NPC06_Theo: All conversations completed, talker disabled");
                }

                cleanupTalkRoutine();

                // Restore player state
                var player = level.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    player.StateMachine.State = global::Celeste.Player.StNormal;
                }
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error in NPC06_Theo talk end handler");
                isInteracting = false;
                cleanupTalkRoutine();
            }
        }

        private void cleanupTalkRoutine()
        {
            if (talkRoutine != null)
            {
                talkRoutine.RemoveSelf();
                talkRoutine = null;
            }
        }

        public override void Update()
        {
            if (isDisposed) return;

            try
            {
                base.Update();
                
                if (sprite != null && !isInteracting)
                {
                    sprite.Play("idle");
                }
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error during NPC06_Theo update");
            }
        }

        public override void Removed(Scene scene)
        {
            try
            {
                IngesteLogger.Debug("NPC06_Theo: Being removed from scene");
                
                isDisposed = true;
                isInteracting = false;
                
                cleanupTalkRoutine();
                
                // Clean up talker
                if (talker != null)
                {
                    talker.Enabled = false;
                }

                base.Removed(scene);
                
                IngesteLogger.Debug("NPC06_Theo: Successfully removed from scene");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error removing NPC06_Theo from scene");
                base.Removed(scene);
            }
        }

        /// <summary>
        /// Static method for registering hooks if needed
        /// </summary>
        public static void Load()
        {
            IngesteLogger.Info("NPC06_Theo: Load called");
            // Register any global hooks here if needed
        }

        /// <summary>
        /// Static method for unregistering hooks
        /// </summary>
        public static void Unload()
        {
            IngesteLogger.Info("NPC06_Theo: Unload called");
            // Unregister any global hooks here if needed
        }
    }
}



