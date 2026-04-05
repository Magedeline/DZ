local eventTrigger = {}
local eventOptions = require("libraries.cutscene_event_ids")

eventTrigger.name = "MaggyHelper/EventTrigger"
eventTrigger.placements = {
    {
        name = "default",
        data = {
            x = 0,
            y = 0,
            width = 16,
            height = 16,
            event = ""
        }
    },
    {
        name = "chapter_intro",
        data = {
            x = 0,
            y = 0,
            width = 24,
            height = 16,
            event = "ch10_intro"
        }
    },
    {
        name = "boss_intro",
        data = {
            x = 0,
            y = 0,
            width = 32,
            height = 16,
            event = "ch11_boss_intro"
        }
    },
    {
        name = "boss_mid",
        data = {
            x = 0,
            y = 0,
            width = 16,
            height = 16,
            event = "ch20_boss_mid"
        }
    },
    {
        name = "boss_end",
        data = {
            x = 0,
            y = 0,
            width = 24,
            height = 16,
            event = "ch20_boss_end"
        }
    },
    {
        name = "ending",
        data = {
            x = 0,
            y = 0,
            width = 32,
            height = 16,
            event = "ch20_ending"
        }
    },
    {
        name = "credits",
        data = {
            x = 0,
            y = 0,
            width = 32,
            height = 16,
            event = "ch21_epilogue_credits"
        }
    }
}

eventTrigger.fieldInformation = {
    event = {
        fieldType = "string",
        editable = true,
        options = eventOptions
    }
}

eventTrigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "event"
}

function eventTrigger.color(room, entity)
    if entity.event == nil or entity.event == "" then
        return {0.85, 0.35, 0.35, 0.8}
    end

    return {0.35, 0.8, 0.95, 0.8}
end

function eventTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16

    return {
        {
            texture = "ahorn/entityTrigger",
            x = entity.x,
            y = entity.y,
            scaleX = width / 8,
            scaleY = height / 8,
            color = eventTrigger.color(room, entity)
        }
    }
end

function eventTrigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16

    return {
        entity.x,
        entity.y,
        width,
        height
    }
end

return eventTrigger
