-- DX Void Dash Refill - Special refill with void properties
-- DX-Side exclusive collectible

local drawableSprite = require("structs.drawable_sprite")

local dxVoidDashRefill = {}

dxVoidDashRefill.name = "MaggyHelper/DXVoidDashRefill"
dxVoidDashRefill.depth = -100

dxVoidDashRefill.placements = {
    {
        name = "DXVoidDashRefill",
        data = {
            respawnTime = 2.5,
            oneUse = false
        }
    },
    {
        name = "DXVoidDashRefill_OneUse",
        data = {
            respawnTime = 0.0,
            oneUse = true
        }
    }
}

dxVoidDashRefill.fieldInformation = {
    respawnTime = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 30.0
    },
    oneUse = { fieldType = "boolean" }
}

dxVoidDashRefill.fieldOrder = { "x", "y", "respawnTime", "oneUse" }

function dxVoidDashRefill.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/Maggy/DesoloZantas/refill/idle00", entity)
    sprite:setColor({0.5, 0.0, 0.8, 1.0})
    return sprite
end

return dxVoidDashRefill

