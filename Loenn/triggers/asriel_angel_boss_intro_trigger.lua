-- Asriel Angel of Death Boss Intro Trigger
-- Triggers the Chapter 20 Angel of Death boss intro cutscene when the player enters.
-- This is the final form of Asriel after the HyperGoner sequence.

local asrielAngelBossIntroTrigger = {}

asrielAngelBossIntroTrigger.name = "DZ/AsrielAngelBossIntroTrigger"

asrielAngelBossIntroTrigger.nodeLimits = {0, 0}
asrielAngelBossIntroTrigger.canResize = {true, true}

asrielAngelBossIntroTrigger.placements = {
    {
        name = "main",
        data = {
            width = 32,
            height = 16,
            triggerOnce = true,
            requireFlag = "",
            requireNotFlag = "asriel_angel_boss_intro",
            dialogKey = "ch20_asriel_angel_boss_intro",
            shakeIntensity = 1.0,
            zoomDuration = 0.6
        }
    },
    {
        name = "room_specific",
        data = {
            width = 32,
            height = 16,
            triggerOnce = true,
            requireFlag = "",
            requireNotFlag = "asriel_angel_boss_intro_azzydc",
            dialogKey = "ch20_asriel_angel_boss_intro",
            shakeIntensity = 1.0,
            zoomDuration = 0.6
        }
    },
    {
        name = "no_zoom",
        data = {
            width = 24,
            height = 16,
            triggerOnce = true,
            requireFlag = "",
            requireNotFlag = "asriel_angel_boss_intro",
            dialogKey = "ch20_asriel_angel_boss_intro",
            shakeIntensity = 0.5,
            zoomDuration = 0.0
        }
    }
}

asrielAngelBossIntroTrigger.fieldInformation = {
    triggerOnce = {
        fieldType = "boolean",
        description = "Only trigger this cutscene once per save file"
    },
    requireFlag = {
        fieldType = "string",
        description = "Optional: Only trigger if this flag is SET (empty = no requirement)"
    },
    requireNotFlag = {
        fieldType = "string",
        description = "Optional: Only trigger if this flag is NOT SET (default: asriel_angel_boss_intro)"
    },
    dialogKey = {
        fieldType = "string",
        description = "Dialog key from English.txt for this intro (default: ch20_asriel_angel_boss_intro)"
    },
    shakeIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 3.0,
        description = "Screen shake intensity during transformation (0-3)"
    },
    zoomDuration = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 3.0,
        description = "Camera zoom duration in seconds (0 = no zoom)"
    }
}

asrielAngelBossIntroTrigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "triggerOnce",
    "requireFlag",
    "requireNotFlag",
    "dialogKey",
    "shakeIntensity",
    "zoomDuration"
}

-- Golden/angelic color scheme for this trigger
function asrielAngelBossIntroTrigger.color(room, entity)
    return {1.0, 0.85, 0.3, 0.7}  -- Golden color
end

function asrielAngelBossIntroTrigger.sprite(room, entity)
    local width = entity.width or 32
    local height = entity.height or 16

    return {
        {
            texture = "ahorn/entityTrigger",
            x = entity.x,
            y = entity.y,
            scaleX = width / 8,
            scaleY = height / 8,
            color = asrielAngelBossIntroTrigger.color(room, entity)
        }
    }
end

function asrielAngelBossIntroTrigger.selection(room, entity)
    local width = entity.width or 32
    local height = entity.height or 16

    return {
        entity.x,
        entity.y,
        width,
        height
    }
end

return asrielAngelBossIntroTrigger
