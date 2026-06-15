-- Loenn plugin for MaggyHelper - LavaBubbleSpawner (Ch13 Inferno)
-- Periodically spawns LavaBubble hazards
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local lavaBubbleSpawner = {}

lavaBubbleSpawner.name = "MaggyHelper/LavaBubbleSpawner"
lavaBubbleSpawner.depth = -200

lavaBubbleSpawner.placements = {
    {
        name = "normal",
        data = {
            spawnInterval = 2.0,
            maxBubbles = 3
        }
    },
    {
        name = "fast",
        data = {
            spawnInterval = 1.0,
            maxBubbles = 5
        }
    }
}

lavaBubbleSpawner.fieldInformation = {
    spawnInterval = {
        minimumValue = 0.2
    },
    maxBubbles = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    }
}

lavaBubbleSpawner.fieldOrder = {
    "x", "y",
    "spawnInterval",
    "maxBubbles"
}

function lavaBubbleSpawner.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/lava_bubble/forming00", entity)
    if sprite then
        sprite:setJustification(0.5, 0.5)
        return {sprite}
    end
    return {}
end

function lavaBubbleSpawner.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return lavaBubbleSpawner
