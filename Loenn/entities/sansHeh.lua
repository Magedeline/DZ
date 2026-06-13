local drawableSprite = require("structs.drawable_sprite")

local sansHeh = {}

sansHeh.name = "MaggyHelper/SansHeh"
sansHeh.depth = -10001
sansHeh.texture = "characters/sans/ha00"
sansHeh.justification = {0.5, 1}
sansHeh.nodeLimits = {0, 1}
sansHeh.nodeLineRenderType = "line"

sansHeh.fieldInformation = {
    ifset = {
        fieldType = "string",
        tooltip = "Session flag that must be set for the laugh effect to activate"
    },
    triggerLaughSfx = {
        fieldType = "boolean",
        tooltip = "If true, plays laugh SFX at the first node position (or entity position if no node)"
    },
    laughSfx = {
        fieldType = "string",
        tooltip = "FMOD event path for the laugh sound effect"
    }
}

sansHeh.placements = {
    {
        name = "default",
        data = {
            ifset = "",
            triggerLaughSfx = false,
            laughSfx = "event:/char/sans/laugh_oneha"
        }
    }
}

function sansHeh.nodeTexture(room, entity, node, index)
    return "characters/sans/ha00"
end

function sansHeh.nodeRender(room, entity, node, index)
    local x, y = node.x, node.y
    local sprite = drawableSprite.fromTexture("characters/sans/ha00", {x = x, y = y})
    sprite:setJustification(0.5, 1)
    sprite:setColor({1, 1, 1, 0.6})
    return sprite
end

return sansHeh
