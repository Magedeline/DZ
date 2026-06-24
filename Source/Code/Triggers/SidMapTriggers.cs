using Celeste.Entities;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger placed in the Ruins lobby hub (10_Ruins_Lobby.bin) that navigates
    /// the player to a specific map by its SID (separate .bin file).
    /// Use this for the 8 portals: sm1–sm6, ex, boss.
    /// </summary>
    [Tracked(false)]
    [HotReloadable]
    public class SidMapEnterTrigger : Trigger
    {
        private readonly string targetSid;
        private readonly string targetRoom;
        private readonly string requiredFlag;
        private readonly string lockedDialogKey;
        private bool triggered;

        public SidMapEnterTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            targetSid     = data.Attr(nameof(targetSid), "");
            targetRoom    = data.Attr(nameof(targetRoom), "");
            requiredFlag  = data.Attr(nameof(requiredFlag), "");
            lockedDialogKey = data.Attr(nameof(lockedDialogKey), "SUBMAP_LOCKED_DEFAULT");
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggered) return;

            Level level = SceneAs<Level>();
            if (level == null) return;

            if (!string.IsNullOrEmpty(requiredFlag) && !level.Session.GetFlag(requiredFlag))
            {
                level.Add(new MiniTextbox(lockedDialogKey));
                return;
            }

            if (string.IsNullOrEmpty(targetSid))
            {
                Logger.Log(LogLevel.Warn, "SidMapEnterTrigger", "targetSid is empty — no navigation.");
                return;
            }

            triggered = true;

            // Store lobby return context so the sub-map can come back here.
            SidMapReturnContext.Set(
                lobbySid:   level.Session.Area.SID,
                lobbyRoom:  level.Session.Level,
                spawnPos:   player.Position
            );

            SidMapNavigation.NavigateToSid(level, targetSid, targetRoom);
        }
    }

    /// <summary>
    /// Trigger placed at the exit of each small-map / EX / boss room.
    /// Returns the player to whichever lobby SID was stored when they entered.
    /// </summary>
    [Tracked(false)]
    [HotReloadable]
    public class SidMapReturnTrigger : Trigger
    {
        private bool triggered;

        public SidMapReturnTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggered) return;
            triggered = true;

            Level level = SceneAs<Level>();
            if (level == null) return;

            SidMapReturnContext.ReturnToLobby(level);
        }
    }

    /// <summary>
    /// Generic trigger for entering any chapter's lobby hub from its main A-side map.
    /// Place at the section of the main chapter that unlocks lobby access.
    /// Parameterised via Lönn: lobbySid, lobbyRoom, requiredFlag, lockedDialogKey.
    /// Used by chapters 11 (Snowdin), 12 (Water), 13 (Fire), 14 (Digital).
    /// </summary>
    [Tracked(false)]
    [HotReloadable]
    public class ChapterLobbyEnterTrigger : Trigger
    {
        private readonly string lobbySid;
        private readonly string lobbyRoom;
        private readonly string requiredFlag;
        private readonly string lockedDialogKey;
        private bool triggered;

        public ChapterLobbyEnterTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            lobbySid        = data.Attr(nameof(lobbySid),        "");
            lobbyRoom       = data.Attr(nameof(lobbyRoom),       "lvl_lobby_hub");
            requiredFlag    = data.Attr(nameof(requiredFlag),    "");
            lockedDialogKey = data.Attr(nameof(lockedDialogKey), "SUBMAP_LOCKED_DEFAULT");
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggered) return;

            Level level = SceneAs<Level>();
            if (level == null) return;

            if (!string.IsNullOrEmpty(requiredFlag) && !level.Session.GetFlag(requiredFlag))
            {
                level.Add(new MiniTextbox(lockedDialogKey));
                return;
            }

            if (string.IsNullOrEmpty(lobbySid))
            {
                Logger.Log(LogLevel.Warn, "ChapterLobbyEnterTrigger", "lobbySid is empty — no navigation.");
                return;
            }

            triggered = true;
            SidMapNavigation.NavigateToSid(level, lobbySid, lobbyRoom);
        }
    }

    /// <summary>
    /// Trigger placed in 10_Ruins_A.bin that sends the player to the
    /// 10_Ruins_Lobby.bin (separate SID), after completing the main chapter.
    /// Replaces the old room-based SubMapLobbyTrigger for chapter 10.
    /// </summary>
    [Tracked(false)]
    [HotReloadable]
    public class RuinsLobbyEnterTrigger : Trigger
    {
        private static readonly string LobbySid  = "Maggy/Lobby/10_Ruins_Lobby";
        private static readonly string LobbyRoom = "lvl_lobby_hub";
        private static readonly string RequiredFlag = "ch10_main_completed";

        private bool triggered;

        public RuinsLobbyEnterTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggered) return;

            Level level = SceneAs<Level>();
            if (level == null) return;

            if (!level.Session.GetFlag(RequiredFlag))
            {
                level.Add(new MiniTextbox("RUINS_LOBBY_LOCKED"));
                return;
            }

            triggered = true;
            SidMapNavigation.NavigateToSid(level, LobbySid, LobbyRoom);
        }
    }
}
