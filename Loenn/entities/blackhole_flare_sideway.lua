local drawableSpriteStruct = require("structs.drawable_sprite")

local blackholeFlareSideway = {}

blackholeFlareSideway.name = "DZ/BlackholeFlareSideway"
blackholeFlareSideway.depth = -50
blackholeFlareSideway.placements = {
    {
        name = "Blackhole Flare Sideway (Right)",
        data = {
            width = 32,
            height = 32,
            direction = "Right",
            speed = 100.0,
            glitchy = true
        }
    },
    {
        name = "Blackhole Flare Sideway (Left)",
        data = {
            width = 32,
            height = 32,
            direction = "Left",
            speed = 100.0,
            glitchy = true
        }
    }
}

blackholeFlareSideway.fieldInformation = {
    direction = {
        options = { "Left", "Right" },
        editable = false
    },
    speed = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 500.0
    },
    glitchy = {
        fieldType = "boolean"
    }
}

function blackholeFlareSideway.sprite(room, entity)
    local sprite = drawableSpriteStruct.fromTexture("danger/lava", entity)

    -- Scale to match entity size
    local scaleX = entity.width / 8
    local scaleY = entity.height / 8

    sprite:setScale(scaleX, scaleY)
    sprite.color = { 0.5, 0.0, 0.5, 0.8 } -- Purple-blackhole tint

    return sprite
end

function blackholeFlareSideway.rectangle(room, entity)
    return entity.x, entity.y, entity.width or 32, entity.height or 32
end

return blackholeFlareSideway
