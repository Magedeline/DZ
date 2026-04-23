local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local ruinsPuzzleSwitch = {}

ruinsPuzzleSwitch.name = "MaggyHelper/RuinsPuzzleSwitch"
ruinsPuzzleSwitch.depth = -100
ruinsPuzzleSwitch.fieldInformation = {
    switchType = {
        options = {"Simple", "Hold", "Sequence", "Timed", "Dash"},
        editable = false
    },
    sequenceOrder = {
        minimumValue = 0
    },
    holdTime = {
        minimumValue = 0
    },
    timerDuration = {
        minimumValue = 0
    }
}
ruinsPuzzleSwitch.placements = {
    {
        name = "simple",
        data = {
            switchType = "Simple",
            gateId = "",
            sequenceOrder = 0,
            holdTime = 1.0,
            timerDuration = 3.0
        }
    },
    {
        name = "hold",
        data = {
            switchType = "Hold",
            gateId = "",
            sequenceOrder = 0,
            holdTime = 1.0,
            timerDuration = 3.0
        }
    },
    {
        name = "timed",
        data = {
            switchType = "Timed",
            gateId = "",
            sequenceOrder = 0,
            holdTime = 1.0,
            timerDuration = 3.0
        }
    }
}

function ruinsPuzzleSwitch.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/ruins_puzzle_switch/inactive", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function ruinsPuzzleSwitch.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 8, 24, 8)
end

return ruinsPuzzleSwitch
