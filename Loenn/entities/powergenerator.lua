local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local PowerGenerator = {}

PowerGenerator.name = "DZ/PowerGenerator"
PowerGenerator.depth = -10550
PowerGenerator.texture = "objects/powergen/Idle00"
PowerGenerator.nodeLimits = {0, 1}
PowerGenerator.fieldInformation = {
    music_progress = {
        fieldType = "integer",
    }
}
PowerGenerator.placements = {
    {
        name = "main",
        data = {
        flipX = false,
        health = 5,
        music_progress = -1,
        music_session = false,
        music = "",
        flag = false,
        canTeleport = false -- Added missing comma and fixed field
        }
    }
}

function PowerGenerator.scale(room, entity)
    local scaleX = entity.flipX and -1 or 1

    return scaleX, 1
end

function PowerGenerator.justification(room, entity)
    local flipX = entity.flipX

    return flipX and 0.75 or 0.25, 0.25
end

return PowerGenerator
