local drawableSprite = require("structs.drawable_sprite")

local infernoMaddyGate = {}

infernoMaddyGate.name = "MaggyHelper/InfernoMaddyGate"
infernoMaddyGate.depth = -9000
infernoMaddyGate.minimumSize = {8, 8}
infernoMaddyGate.canResize = {false, true}

infernoMaddyGate.placements = {
    {
        name = "close_behind_player",
        data = {
            height = 48,
            sprite = "default",
            type = "CloseBehindPlayer"
        }
    },
    {
        name = "close_behind_player_always",
        data = {
            height = 48,
            sprite = "default",
            type = "CloseBehindPlayerAlways"
        }
    },
    {
        name = "nearest_switch",
        data = {
            height = 48,
            sprite = "default",
            type = "NearestSwitch"
        }
    },
    {
        name = "holding_theo",
        data = {
            height = 48,
            sprite = "default",
            type = "HoldingTheo"
        }
    },
    {
        name = "touch_switches",
        data = {
            height = 48,
            sprite = "default",
            type = "TouchSwitches"
        }
    }
}

infernoMaddyGate.fieldInformation = {
    sprite = {
        options = { "default", "mirror", "theo" },
        editable = true
    },
    type = {
        options = {
            "NearestSwitch",
            "CloseBehindPlayer",
            "CloseBehindPlayerAlways",
            "HoldingTheo",
            "TouchSwitches"
        },
        editable = false
    }
}

infernoMaddyGate.fieldOrder = {
    "x", "y", "height", "sprite", "type"
}

function infernoMaddyGate.sprite(room, entity)
    local sprites = {}
    local height = entity.height or 48
    local texture = "objects/door/templeDoor00"
    local spriteType = entity.sprite or "default"

    if spriteType == "mirror" then
        texture = "objects/door/templeDoorB00"
    elseif spriteType == "theo" then
        texture = "objects/door/templeDoorC00"
    end

    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setJustification(0.5, 0.0)
    table.insert(sprites, sprite)

    return sprites
end

function infernoMaddyGate.selection(room, entity)
    local utils = require("utils")
    local height = entity.height or 48
    return utils.rectangle(entity.x - 4, entity.y, 8, height)
end

return infernoMaddyGate
