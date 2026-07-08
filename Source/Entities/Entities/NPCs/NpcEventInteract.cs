namespace Celeste.Entities
{
    /// <summary>
    /// A generic custom entity for event and interact logic, usable by any NPC.
    /// Enhanced to support all NPC types from NPCs 00-21 with cs flag support.
    /// </summary>
    [CustomEntity(ids: "DZ/NPCEventInteract")]
    [Tracked(true)]
    public class NpcEventInteract : Entity
    {
        public Sprite Sprite;
        public TalkComponent Talker;
        public VertexLight Light;
        public System.Action<Level, global::Celeste.Player, NpcEventInteract> OnEventStart;
        public System.Action<Level, global::Celeste.Player, NpcEventInteract> OnInteract;
        public System.Action<Level, global::Celeste.Player, NpcEventInteract> OnEventEnd;
        public bool EventTriggered;
        public bool Interacted;

        // Enhanced properties from Lua configuration
        public string NpcName { get; set; }
        public string NpcType { get; set; }
        public string SpriteId { get; set; }
        public string AlternateSprite { get; set; }
        public bool EnableSpriteSwap { get; set; }
        public string CsFlag { get; set; }
        public string DialogKey { get; set; }
        public string TriggerFlag { get; set; }
        public string RequireFlag { get; set; }

        // Internal state tracking
        private bool hasPlayedDialog = false;
        private int conversationCount = 0;
        private bool isCurrentlyInteracting = false;
        private string currentSpriteId = "";

        public NpcEventInteract(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            // Extract all properties from EntityData
            NpcType = data.Attr("npcType", "NPC00_Kirby_Start");
            SpriteId = data.Attr("spriteId", "theo");
            NpcName = data.Attr("npcName", "NPC");
            EnableSpriteSwap = data.Bool("enableSpriteSwap", false);
            AlternateSprite = data.Attr("alternateSprite", "DZ_CHara");
            CsFlag = data.Attr("csFlag", "");
            DialogKey = data.Attr("dialogKey", "");
            TriggerFlag = data.Attr("triggerFlag", "");
            RequireFlag = data.Attr("requireFlag", "");

            SetupSprite();
            SetupCollision();
            SetupInteraction();
            SetupLight(data);

            Depth = 100;
        }

        private void SetupSprite()
        {
            currentSpriteId = DetermineCurrentSprite();

            try
            {
                Add(Sprite = GFX.SpriteBank.Create(currentSpriteId));
                Sprite.Play("idle");
            }
            catch (Exception)
            {
                // Fallback to theo if sprite doesn't exist
                Add(Sprite = GFX.SpriteBank.Create("theo"));
                Sprite.Play("idle");
                currentSpriteId = "theo";
                Logger.Log(LogLevel.Warn, nameof(NpcEventInteract),
                    $"Failed to load sprite '{currentSpriteId}', using fallback 'theo'");
            }
        }

        private string DetermineCurrentSprite()
        {
            // Check if sprite swapping is enabled and conditions are met
            if (EnableSpriteSwap && ShouldUseAlternateSprite())
            {
                return AlternateSprite;
            }
            return SpriteId;
        }

        private bool ShouldUseAlternateSprite()
        {
            if (Scene is Level level)
            {
                // Add your sprite swap logic here based on game state
                // For example, swap based on flags, player position, time, etc.

                // Example: Swap for Chara-related NPCs based on certain flags
                if (NpcType.Contains("Chara") && level.Session.GetFlag("chara_hostile"))
                {
                    return true;
                }

                // Example: Swap for Madeline NPCs based on Badeline integration
                if (NpcType.Contains("Maddy") && level.Session.GetFlag("badeline_merged"))
                {
                    return true;
                }

                // Example: Swap for ending NPCs based on story progression
                if (NpcType.Contains("Ending") && level.Session.GetFlag("final_battle_started"))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetupCollision()
        {
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(Talker = new TalkComponent(new Rectangle(-24, -24, 48, 48), new Vector2(0f, -24f), OnTalk));
            Talker.PlayerMustBeFacing = false;
        }

        private void SetupInteraction()
        {
            // Set up NPC-specific interaction logic based on NpcType
            if (OnInteract == null)
            {
                OnInteract = GetNpcSpecificInteraction();
            }
        }

        private void SetupLight(EntityData data)
        {
            if (data.Bool("hasLight", true))
                Add(Light = new VertexLight(Color.White, 1f, 16, 32));
        }

        private System.Action<Level, global::Celeste.Player, NpcEventInteract> GetNpcSpecificInteraction()
        {
            return NpcType switch
            {
                // Generic NPCs
                "NPC_Theo" => Npc_TheoInteraction,
                "NPCDZ_CHara" => NpcDZ_CHaraInteraction,
                "NPC_DigitalGuide" => Npc_DigitalGuideInteraction,
                "NPC_Els" => Npc_ElsInteraction,
                "NPC_Kirby" => Npc_KirbyInteraction,
                "NPC_MetaKnight" => Npc_MetaKnightInteraction,
                "NPC_Phone" => Npc_PhoneInteraction,
                "NPC_Ralsei" => Npc_RalseiInteraction,
                "NPC_Roxus" => Npc_RoxusInteraction,
                "NPC_Temmie" => Npc_TemmieInteraction,
                "NPC_Axis" => Npc_AxisInteraction,
                "NPC_TitanCouncilMember" => Npc_TitanCouncilMemberInteraction,
                "NPC00_Theo" => Npc00_TheoInteraction,
                "NPC01_DZ" => Npc01_DZInteraction,
                "NPC02_DZ" => Npc02_DZInteraction,
                "NPC03_DZ" => Npc03_DZInteraction,
                "NPC03_Theo" => Npc03_TheoInteraction,
                "NPC05_Magolor_Vents" => Npc05_Magolor_VentsInteraction,
                "NPC05_MagolorEscape" => Npc05_Magolor_EscapeInteraction,
                "NPC05_Oshiro_Breakdown" => Npc05_Oshiro_BreakdownInteraction,
                "NPC05_Oshiro_Clutter" => Npc05_Oshiro_ClutterInteraction,
                "NPC05_Oshiro_Hallway1" => Npc05_Oshiro_Hallway1Interaction,
                "NPC05_Oshiro_Hallway2" => Npc05_Oshiro_Hallway2Interaction,
                "NPC05_Oshiro_Lobby" => Npc05_Oshiro_LobbyInteraction,
                "NPC05_Oshiro_Rooftop" => Npc05_Oshiro_RooftopInteraction,
                "NPC05_Oshiro_Suite" => Npc05_Oshiro_SuiteInteraction,
                "NPC06_Magolor" => Npc06_MagolorInteraction,
                "NPC06_Theo" => Npc06_TheoInteraction,
                "NPC07DZ_CHara" => Npc07DZ_CHaraInteraction,
                "NPC07_Maddy_Mirror" => Npc07_Maddy_MirrorInteraction,
                "NPC08DZ_CHara_Crying" => Npc08DZ_CHara_CryingInteraction,
                "NPC08_Maddy_and_Theo_Ending" => Npc08_Maddy_and_Theo_EndingInteraction,
                "NPC08_Madeline_Plateau" => Npc08_Madeline_PlateauInteraction,
                "NPC08_DZ_Ending" => Npc08_DZ_EndingInteraction,
                "NPC17_Kirby" => Npc17_KirbyInteraction,
                "NPC17_Oshiro" => Npc17_OshiroInteraction,
                "NPC17_Ralsei" => Npc17_RalseiInteraction,
                "NPC17_Theo" => Npc17_TheoInteraction,
                "NPC17_Toriel" => Npc17_TorielInteraction,
                "NPC18_Toriel_Inside" => Npc18_Toriel_InsideInteraction,
                "NPC18_Toriel_Outside" => Npc18_Toriel_OutsideInteraction,
                "NPC19_Gravestone" => Npc19_GravestoneInteraction,
                "NPC19_DZ_Loop" => Npc19_DZ_LoopInteraction,
                "NPC20_Asriel" => Npc20_AsrielInteraction,
                "NPC20_Granny" => Npc20_GrannyInteraction,
                "NPC20_Madeline" => Npc20_MadelineInteraction,
                _ => DefaultInteraction
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (Scene is Level level)
            {
                Logger.Log(LogLevel.Info, nameof(NpcEventInteract),
                    $"NpcEventInteract Added: Name={NpcName}, Type={NpcType}, RequireFlag={RequireFlag}, CsFlag={CsFlag}");

                // Check if this NPC should be visible based on require flag
                if (!string.IsNullOrEmpty(RequireFlag) && !level.Session.GetFlag(RequireFlag))
                {
                    Logger.Log(LogLevel.Info, nameof(NpcEventInteract),
                        $"NPC {NpcName} hidden: RequireFlag '{RequireFlag}' not set");
                    Visible = false;
                    Talker.Enabled = false;
                    return;
                }

                // Check if this NPC has already completed its interaction
                if (!string.IsNullOrEmpty(CsFlag) && level.Session.GetFlag(CsFlag))
                {
                    Logger.Log(LogLevel.Info, nameof(NpcEventInteract),
                        $"NPC {NpcName} already completed: CsFlag '{CsFlag}' is set");
                    // NPC has completed interaction, set to completed state
                    SetCompletedState();
                    return;
                }

                Logger.Log(LogLevel.Info, nameof(NpcEventInteract),
                    $"NPC {NpcName} is ready for interaction. Talker enabled: {Talker?.Enabled}");
            }
        }

        private void SetCompletedState()
        {
            hasPlayedDialog = true;
            Talker.Enabled = false;

            // Optional: Change sprite to indicate completion
            if (EnableSpriteSwap)
            {
                try
                {
                    Remove(Sprite);
                    Add(Sprite = GFX.SpriteBank.Create(AlternateSprite));
                    Sprite.Play("idle");
                    currentSpriteId = AlternateSprite;
                }
                catch (Exception)
                {
                    Logger.Log(LogLevel.Warn, nameof(NpcEventInteract),
                        $"Failed to load completed sprite '{AlternateSprite}'");
                }
            }
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (player == null || Scene == null || isCurrentlyInteracting)
                return;

            Level level = SceneAs<Level>();

            // Check require flag again at interaction time
            if (!string.IsNullOrEmpty(RequireFlag) && !level.Session.GetFlag(RequireFlag))
            {
                Logger.Log(LogLevel.Info, nameof(NpcEventInteract),
                    $"Player tried to interact with {NpcName} but requirement flag '{RequireFlag}' not met");
                return;
            }

            // Check if this NPC has already been interacted with (one-time use)
            if (hasPlayedDialog && !string.IsNullOrEmpty(CsFlag) && level.Session.GetFlag(CsFlag))
            {
                Logger.Log(LogLevel.Info, nameof(NpcEventInteract),
                    $"Player tried to interact with {NpcName} but it's already been used");
                return;
            }

            isCurrentlyInteracting = true;

            Logger.Log(LogLevel.Info, nameof(NpcEventInteract),
                $"Player interacting with {NpcName} (Type: {NpcType})");

            // Fire interaction callback
            OnInteract?.Invoke(level, player, this);
        }

        private void DefaultInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc)
        {
            level.StartCutscene(EndDefaultInteraction);
            Add(new Coroutine(DefaultInteractionRoutine(level, player)));
        }

        private IEnumerator DefaultInteractionRoutine(Level level, global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;

            string dialogKey = !string.IsNullOrEmpty(DialogKey) ? DialogKey : $"DEFAULT_NPC_DIALOG_{NpcName.ToUpper()}";
            yield return Textbox.Say(dialogKey);

            EndDefaultInteraction(level);
        }

        private void EndDefaultInteraction(Level level)
        {
            CompleteInteraction(level);
        }

        private void CompleteInteraction(Level level)
        {
            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }

            level.EndCutscene();

            // Set completion flags
            if (!string.IsNullOrEmpty(CsFlag))
            {
                level.Session.SetFlag(CsFlag, true);
                hasPlayedDialog = true;
                Logger.Log(LogLevel.Info, nameof(NpcEventInteract),
                    $"Set CsFlag '{CsFlag}' for {NpcName}");
            }

            if (!string.IsNullOrEmpty(TriggerFlag))
            {
                level.Session.SetFlag(TriggerFlag, true);
                Logger.Log(LogLevel.Info, nameof(NpcEventInteract),
                    $"Set TriggerFlag '{TriggerFlag}' for {NpcName}");
            }

            conversationCount++;
            isCurrentlyInteracting = false; // Reset to allow future interactions (or block based on flags)
        }

        #region NPC-Specific Interactions

        // Generic NPC interactions
        private void Npc_TheoInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void NpcDZ_CHaraInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_DigitalGuideInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_ElsInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_KirbyInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_MetaKnightInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_PhoneInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_RalseiInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_RoxusInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_TemmieInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_AxisInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc_TitanCouncilMemberInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 0 NPCs
        private void Npc00_TheoInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 1 NPCs
        private void Npc01_DZInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 2 NPCs
        private void Npc02_DZInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 3 NPCs
        private void Npc03_DZInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc03_TheoInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 5 NPCs (Oshiro and Magolor)
        private void Npc05_Magolor_VentsInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc05_Magolor_EscapeInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc05_Oshiro_BreakdownInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc05_Oshiro_ClutterInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc05_Oshiro_Hallway1Interaction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc05_Oshiro_Hallway2Interaction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc05_Oshiro_LobbyInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc05_Oshiro_RooftopInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc05_Oshiro_SuiteInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 6 NPCs
        private void Npc06_MagolorInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc06_TheoInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 7 NPCs
        private void Npc07DZ_CHaraInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc07_Maddy_MirrorInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 8 NPCs
        private void Npc08DZ_CHara_CryingInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc08_Maddy_and_Theo_EndingInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc08_Madeline_PlateauInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc08_DZ_EndingInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 17 NPCs
        private void Npc17_KirbyInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc17_OshiroInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc17_RalseiInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc17_TheoInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc17_TorielInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 18 NPCs
        private void Npc18_Toriel_InsideInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc18_Toriel_OutsideInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 19 NPCs
        private void Npc19_GravestoneInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc19_DZ_LoopInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        // Chapter 20 NPCs
        private void Npc20_AsrielInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc20_GrannyInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);
        private void Npc20_MadelineInteraction(Level level, global::Celeste.Player player, NpcEventInteract npc) => GenericDialogInteraction(level, player, DialogKey);

        private void GenericDialogInteraction(Level level, global::Celeste.Player player, string dialogKey)
        {
            level.StartCutscene(EndGenericDialogInteraction);
            Add(new Coroutine(GenericDialogRoutine(level, player, dialogKey)));
        }

        private IEnumerator GenericDialogRoutine(Level level, global::Celeste.Player player, string dialogKey)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;
            yield return Textbox.Say(dialogKey);
            EndGenericDialogInteraction(level);
        }

        private void EndGenericDialogInteraction(Level level)
        {
            CompleteInteraction(level);
        }

        #endregion

        public override void Update()
        {
            base.Update();

            // Example: trigger event when player is close and not yet triggered
            if (!EventTriggered && Scene is Level level)
            {
                global::Celeste.Player player = level.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null && Vector2.Distance(player.Position, Position) < 32f)
                {
                    EventTriggered = true;
                    OnEventStart?.Invoke(level, player, this);
                }
            }

            // Update sprite based on current conditions
            if (EnableSpriteSwap && Sprite != null)
            {
                string desiredSpriteId = DetermineCurrentSprite();
                if (currentSpriteId != desiredSpriteId)
                {
                    try
                    {
                        Remove(Sprite);
                        Add(Sprite = GFX.SpriteBank.Create(desiredSpriteId));
                        Sprite.Play("idle");
                        currentSpriteId = desiredSpriteId;
                    }
                    catch (Exception)
                    {
                        // Ignore sprite change failures
                        Logger.Log(LogLevel.Warn, nameof(NpcEventInteract),
                            $"Failed to change sprite to '{desiredSpriteId}'");
                    }
                }
            }
        }

        public void EndEvent(Level level, global::Celeste.Player player)
        {
            OnEventEnd?.Invoke(level, player, this);
        }

        public string GetCurrentSpriteId()
        {
            return currentSpriteId;
        }

        public void ForceSetSprite(string spriteId)
        {
            try
            {
                Remove(Sprite);
                Add(Sprite = GFX.SpriteBank.Create(spriteId));
                Sprite.Play("idle");
                currentSpriteId = spriteId;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(NpcEventInteract),
                    $"Failed to force set sprite to '{spriteId}': {ex.Message}");
            }
        }
    }
}



