local drawableSprite = require("structs.drawable_sprite")

local sansSprite = {}

sansSprite.name = "DZ/SansSprite"
sansSprite.depth = -8500
sansSprite.texture = "characters/sans/idle00"
sansSprite.justification = {0.5, 1.0}

sansSprite.fieldInformation = {
    animation = {
        fieldType = "string",
        options = {
            "idle",
            "walk",
            "heh",
            "noeyes",
            "patonmadshoulder",
            "sheild",
            "wtf",
            "oderup",
            "eepy"
        },
        tooltip = "Starting animation for the sprite"
    }
}

sansSprite.placements = {
    {
        name = "default",
        data = {
            animation = "idle"
        }
    }
}

function sansSprite.sprite(room, entity)
    local anim = entity.animation or "idle"
    local texture = "characters/sans/" .. anim .. "00"
    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

return sansSprite
