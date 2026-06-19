local drawableSprite = require("structs.drawable_sprite")

local papyrusSprite = {}

papyrusSprite.name = "DZ/PapyrusSprite"
papyrusSprite.depth = -8500
papyrusSprite.texture = "characters/papyrus/idle00"
papyrusSprite.justification = {0.5, 1.0}

papyrusSprite.fieldInformation = {
    animation = {
        fieldType = "string",
        options = {
            "idle",
            "walk",
            "notfair",
            "idontgetit",
            "cry",
            "angy",
            "depress"
        },
        tooltip = "Starting animation for the sprite"
    }
}

papyrusSprite.placements = {
    {
        name = "main",
        data = {
            animation = "idle"
        }
    }
}

function papyrusSprite.sprite(room, entity)
    local anim = entity.animation or "idle"
    local texture = "characters/papyrus/" .. anim .. "00"
    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

return papyrusSprite
