local drawableSprite = require("structs.drawable_sprite")

local maddyCrystal = {}

maddyCrystal.name = "DZ/MaddyCrystal"
maddyCrystal.depth = 100
maddyCrystal.placements = {
    name = "maddy_crystal",
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/maddyCrystal/idle00"

function maddyCrystal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

return maddyCrystal