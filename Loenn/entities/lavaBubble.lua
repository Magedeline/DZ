-- Loenn plugin for MaggyHelper - LavaBubble (Ch13 Inferno)
-- Rising lava bubble hazard that bursts and damages the player
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local lavaBubble = {}

lavaBubble.name = "MaggyHelper/LavaBubble"
lavaBubble.depth = -200

lavaBubble.placements = {
    {
        name = "normal",
        data = {
            riseSpeed = 80.0,
            burstHeight = 100.0,
            damageRadius = 50.0
        }
    },
    {
        name = "fast",
        data = {
            riseSpeed = 140.0,
            burstHeight = 80.0,
            damageRadius = 60.0
        }
    }
}

lavaBubble.fieldInformation = {
    riseSpeed = {
        minimumValue = 10.0
    },
    burstHeight = {
        minimumValue = 10.0
    },
    damageRadius = {
        minimumValue = 5.0
    }
}

lavaBubble.fieldOrder = {
    "x", "y",
    "riseSpeed",
    "burstHeight",
    "damageRadius"
}

function lavaBubble.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/lava_bubble/forming00", entity)
    if sprite then
        sprite:setJustification(0.5, 0.5)
        return {sprite}
    end
    return {}
end

function lavaBubble.selection(room, entity)
    return utils.rectangle(entity.x - 10, entity.y - 10, 20, 20)
end

return lavaBubble
