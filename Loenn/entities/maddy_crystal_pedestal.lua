local drawableSprite = require("structs.drawable_sprite")

local maddyCrystalPedestal = {}

maddyCrystalPedestal.name = "DZ/MaddyCrystalPedestal"
maddyCrystalPedestal.depth = 8998
maddyCrystalPedestal.placements = {
    {
        name = "main",
        data = {}
    }
}

local texture = "characters/MaddyCrystal/pedestal"

function maddyCrystalPedestal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite:setJustification(0.5, 1.0)

    return sprite
end

return maddyCrystalPedestal
