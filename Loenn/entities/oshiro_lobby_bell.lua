local oshiroLobbyBell = {}

oshiroLobbyBell.name = "MaggyHelper/OshiroLobbyBell"
oshiroLobbyBell.depth = 0
oshiroLobbyBell.texture = "objects/IngesteHelper/oshiro_lobby_bell"
oshiroLobbyBell.justification = {0.5, 1.0}

oshiroLobbyBell.fieldInformation = {
    isActive = {
        fieldType = "boolean"
    },
    bellType = {
        options = {"normal", "golden", "silver", "antique"},
        editable = false
    },
    soundEffect = {
        fieldType = "string",
        options = {
            "event:/game/03_resort/oshiro_bell",
            "event:/game/general/thing_booped",
            "event:/char/oshiro/chat_voice"
        },
        editable = true
    },
    interactDistance = {
        fieldType = "number",
        minimumValue = 16.0,
        maximumValue = 64.0
    },
    onceOnly = {
        fieldType = "boolean"
    },
    flagName = {
        fieldType = "string"
    },
    eventId = {
        fieldType = "string"
    }
}

oshiroLobbyBell.placements = {
    {
        name = "normal",
        data = {
            isActive = true,
            bellType = "normal",
            soundEffect = "event:/game/03_resort/oshiro_bell",
            interactDistance = 32.0,
            onceOnly = false,
            flagName = "",
            eventId = ""
        }
    },
    {
        name = "golden",
        data = {
            isActive = true,
            bellType = "golden",
            soundEffect = "event:/game/03_resort/oshiro_bell",
            interactDistance = 40.0,
            onceOnly = true,
            flagName = "oshiro_bell_rung",
            eventId = "oshiro_summon"
        }
    },
    {
        name = "antique",
        data = {
            isActive = true,
            bellType = "antique",
            soundEffect = "event:/char/oshiro/chat_voice",
            interactDistance = 24.0,
            onceOnly = false,
            flagName = "",
            eventId = ""
        }
    }
}

function oshiroLobbyBell.sprite(room, entity)
    local bellType = entity.bellType or "normal"
    
    return {
        texture = "objects/IngesteHelper/oshiro_lobby_bell_" .. bellType,
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return oshiroLobbyBell