namespace Celeste.Entities
{
    /// <summary>
    /// Static helpers for cross-map SID navigation (used by the Ruins lobby system
    /// and small-maps).  Keeps Celeste's LevelLoader plumbing in one place.
    /// </summary>
    public static class SidMapNavigation
    {
        /// <summary>
        /// Transitions to a different .bin map identified by its Everest SID.
        /// </summary>
        public static void NavigateToSid(Level level, string targetSid, string targetRoom = null)
        {
            AreaData area = AreaData.Get(targetSid);
            if (area == null)
            {
                Logger.Log(LogLevel.Error, "SidMapNavigation",
                    $"Cannot find map SID '{targetSid}'. " +
                    $"Make sure the .bin exists and has the matching package name.");
                return;
            }

            level.OnEndOfFrame += () =>
            {
                var session = new Session(area.ToKey());
                if (!string.IsNullOrEmpty(targetRoom))
                    session.Level = targetRoom;
                Engine.Scene = new LevelLoader(session);
            };
        }
    }

    /// <summary>
    /// In-memory store for "where to return to" when a small-map / EX / boss map
    /// is completed.  Stored as statics so it survives the LevelLoader transition
    /// within the same game session, but resets on game restart (acceptable).
    /// </summary>
    public static class SidMapReturnContext
    {
        private static string _lobbySid;
        private static string _lobbyRoom;
        private static Vector2 _spawnPos;

        public static void Set(string lobbySid, string lobbyRoom, Vector2 spawnPos)
        {
            _lobbySid  = lobbySid;
            _lobbyRoom = lobbyRoom;
            _spawnPos  = spawnPos;
        }

        public static void ReturnToLobby(Level level)
        {
            if (string.IsNullOrEmpty(_lobbySid))
            {
                // Fallback: return to the chapter-10 lobby
                _lobbySid  = "Maggy/Lobby/10_Ruins_Lobby";
                _lobbyRoom = "lvl_lobby_hub";
            }

            AreaData area = AreaData.Get(_lobbySid);
            if (area == null)
            {
                Logger.Log(LogLevel.Error, "SidMapReturnContext",
                    $"Cannot find lobby SID '{_lobbySid}' for return.");
                return;
            }

            string lobbyRoom  = _lobbyRoom;
            Vector2 spawnPos  = _spawnPos;

            level.OnEndOfFrame += () =>
            {
                var session = new Session(area.ToKey())
                {
                    Level        = lobbyRoom,
                    RespawnPoint = spawnPos
                };
                Engine.Scene = new LevelLoader(session);
            };
        }

        public static void Clear()
        {
            _lobbySid  = null;
            _lobbyRoom = null;
            _spawnPos  = Vector2.Zero;
        }
    }
}
