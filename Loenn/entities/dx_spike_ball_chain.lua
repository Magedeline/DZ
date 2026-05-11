-- DX Spike Ball Chain - Swinging hazard
-- DX-Side exclusive platforming hazard

local drawableSprite = require("structs.drawable_sprite")

local dxSpikeBallChain = {}

dxSpikeBallChain.name = "MaggyHelper/DXSpikeBallChain"
dxSpikeBallChain.depth = -9000

dxSpikeBallChain.placements = {
    {
        name = "DXSpikeBallChain",
        data = {
            chainLength = 80.0,
            swingSpeed = 2.0,
            ballRadius = 12.0,
            swingAngle = 1.5708
        }
    },
    {
        name = "DXSpikeBallChain_Long",
        data = {
            chainLength = 140.0,
            swingSpeed = 1.5,
            ballRadius = 16.0,
            swingAngle = 1.5708
        }
    },
    {
        name = "DXSpikeBallChain_Fast",
        data = {
            chainLength = 60.0,
            swingSpeed = 4.0,
            ballRadius = 10.0,
            swingAngle = 2.0944
        }
    }
}

dxSpikeBallChain.fieldInformation = {
    chainLength = {
        fieldType = "number",
        minimumValue = 20.0,
        maximumValue = 300.0
    },
    swingSpeed = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 10.0
    },
    ballRadius = {
        fieldType = "number",
        minimumValue = 4.0,
        maximumValue = 40.0
    },
    swingAngle = {
        fieldType = "number",
        minimumValue = 0.2,
        maximumValue = 3.14159
    }
}

dxSpikeBallChain.fieldOrder = { "x", "y", "chainLength", "swingSpeed", "ballRadius", "swingAngle" }

function dxSpikeBallChain.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("Maggy/DesoloZantas/danger/spikeball00", entity)
    sprite:setColor({0.5, 0.0, 0.5, 1.0})
    return sprite
end

return dxSpikeBallChain

