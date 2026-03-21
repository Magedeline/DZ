-- ChapterLobbyEnterTrigger
-- Place at the section of a main A-side chapter (e.g. 11_Snow_A.bin) that
-- should transport the player into the chapter's standalone lobby hub map.
-- Use one placement per chapter (11 Snowdin, 12 Water, 13 Fire, 14 Digital).

local chapterLobbyEnterTrigger = {}

chapterLobbyEnterTrigger.name        = "MaggyHelper/ChapterLobbyEnterTrigger"
chapterLobbyEnterTrigger.color       = { 0.2, 0.7, 1.0 }   -- sky blue

chapterLobbyEnterTrigger.fieldInformation = {
    lobbySid        = { fieldType = "string" },
    lobbyRoom       = { fieldType = "string" },
    requiredFlag    = { fieldType = "string" },
    lockedDialogKey = { fieldType = "string" },
}

chapterLobbyEnterTrigger.fieldOrder = {
    "x", "y", "width", "height",
    "lobbySid", "lobbyRoom", "requiredFlag", "lockedDialogKey",
}

chapterLobbyEnterTrigger.placements = {
    -- ── Chapter 11 – Snowdin City ─────────────────────────────────────────
    {
        name = "CH11 – Enter Snowdin City Lobby",
        data = {
            width           = 16, height          = 64,
            lobbySid        = "Maggy/Lobby/11_Snowdin_Lobby",
            lobbyRoom       = "lvl_lobby_hub",
            requiredFlag    = "ch11_main_completed",
            lockedDialogKey = "SNOWDIN_LOBBY_LOCKED",
        }
    },
    -- ── Chapter 12 – Wateredgefalls ───────────────────────────────────────
    {
        name = "CH12 – Enter Wateredgefalls Lobby",
        data = {
            width           = 16, height          = 64,
            lobbySid        = "Maggy/Lobby/12_Water_Lobby",
            lobbyRoom       = "lvl_lobby_hub",
            requiredFlag    = "ch12_main_completed",
            lockedDialogKey = "WATER_LOBBY_LOCKED",
        }
    },
    -- ── Chapter 13 – Hotcliffland ─────────────────────────────────────────
    {
        name = "CH13 – Enter Hotcliffland Lobby",
        data = {
            width           = 16, height          = 64,
            lobbySid        = "Maggy/Lobby/13_Fire_Lobby",
            lobbyRoom       = "lvl_lobby_hub",
            requiredFlag    = "ch13_main_completed",
            lockedDialogKey = "FIRE_LOBBY_LOCKED",
        }
    },
    -- ── Chapter 14 – Cyber Nexus ──────────────────────────────────────────
    {
        name = "CH14 – Enter Cyber Nexus Lobby",
        data = {
            width           = 16, height          = 64,
            lobbySid        = "Maggy/Lobby/14_Digital_Lobby",
            lobbyRoom       = "lvl_lobby_hub",
            requiredFlag    = "ch14_main_completed",
            lockedDialogKey = "DIGITAL_LOBBY_LOCKED",
        }
    },
}

return chapterLobbyEnterTrigger
