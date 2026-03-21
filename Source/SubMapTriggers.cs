using MaggyHelper.Entities;

namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Trigger that transports player to submap lobby for chapters 10-14
    /// </summary>
    [Tracked(false)]
    [HotReloadable]
    public class SubMapLobbyTrigger : Trigger
    {
        private readonly int chapterNumber;
        private readonly Vector2 lobbySpawnPosition;
        private readonly string requiredFlag;
        private bool triggered;

        public SubMapLobbyTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            chapterNumber = data.Int(nameof(chapterNumber), 10);
            lobbySpawnPosition = data.NodesOffset(offset)[0];
            requiredFlag = data.Attr(nameof(requiredFlag), $"ch{chapterNumber}_main_completed");
        }

        public override void OnEnter(global::Celeste.Player player) // Fixed namespace reference
        {
            base.OnEnter(player);
            
            if (triggered) return;
            
            Level level = SceneAs<Level>();
            if (level == null) return;
            
            // Check if player has completed the main chapter
            if (!string.IsNullOrEmpty(requiredFlag) && !level.Session.GetFlag(requiredFlag))
            {
                // Show message about needing to complete main chapter first
                level.Add(new MiniTextbox($"SUBMAP_LOCKED_MAIN_CH{chapterNumber}"));
                return;
            }
            
            triggered = true;
            
            // Transport to submap lobby
            level.OnEndOfFrame += () => {
                var session = new Session(level.Session.Area)
                {
                    Level = $"lobby_ch{chapterNumber}",
                    RespawnPoint = lobbySpawnPosition
                };
                Engine.Scene = new LevelLoader(session);
            };
        }
    }

    /// <summary>
    /// Trigger for returning from submap to main chapter
    /// </summary>
    [Tracked(false)]
    [HotReloadable]
    public class SubMapReturnTrigger : Trigger
    {
        private readonly string returnLevel;
        private readonly Vector2 returnPosition;
        private bool triggered;

        public SubMapReturnTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            returnLevel = data.Attr(nameof(returnLevel), "");
            returnPosition = data.NodesOffset(offset).Length > 0 ? data.NodesOffset(offset)[0] : Vector2.Zero;
        }

        public override void OnEnter(global::Celeste.Player player) // Fixed namespace reference
        {
            base.OnEnter(player);
            
            if (triggered) return;
            triggered = true;
            
            Level level = SceneAs<Level>();
            if (level == null) return;
            
            // Return to specified level or use SubMapManager
            if (!string.IsNullOrEmpty(returnLevel))
            {
                level.OnEndOfFrame += () => {
                    var session = new Session(level.Session.Area)
                    {
                        Level = returnLevel,
                        RespawnPoint = returnPosition
                    };
                    Engine.Scene = new LevelLoader(session);
                };
            }
            else
            {
                // Use SubMapManager to handle return
                SubMapManager.Instance?.ReturnToLobby(level, player);
            }
        }
    }

    /// <summary>
    /// Trigger for entering a specific submap directly
    /// </summary>
    [Tracked(false)]
    [HotReloadable]
    public class SubMapEnterTrigger : Trigger
    {
        private readonly int chapterNumber;
        private readonly int submapNumber;
        private readonly bool checkUnlocked;
        private bool triggered;

        public SubMapEnterTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            chapterNumber = data.Int(nameof(chapterNumber), 10);
            submapNumber = data.Int(nameof(submapNumber), 1);
            checkUnlocked = data.Bool(nameof(checkUnlocked), true);
        }

        public override void OnEnter(global::Celeste.Player player) // Fixed namespace reference
        {
            base.OnEnter(player);
            
            if (triggered) return;
            
            Level level = SceneAs<Level>();
            if (level == null) return;
            
            // Check if submap is unlocked
            if (checkUnlocked && SubMapManager.Instance?.CanEnterSubmap(chapterNumber, submapNumber) != true)
            {
                level.Add(new MiniTextbox($"SUBMAP_LOCKED_CH{chapterNumber}_{submapNumber}"));
                return;
            }
            
            triggered = true;
            SubMapManager.Instance?.EnterSubmap(level, player, chapterNumber, submapNumber);
        }
    }
}



