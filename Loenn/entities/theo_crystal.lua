local drawableSprite = require("structs.drawable_sprite")

local maddycrystal = {}

maddycrystal.name = "DZ/Maddy_crystal"
maddycrystal.depth = 100
maddycrystal.placements = {
    {
        name = "main",
        data = {}
    }
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/maddyCrystal/idle00"

function maddycrystal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y = sprite.y + offsetY

    return sprite
end

return maddycrystal