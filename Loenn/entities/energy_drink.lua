local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local energyDrink = {}

energyDrink.name = "DZ/EnergyDrink"
energyDrink.depth = -50

energyDrink.placements = {
    {
        name = "normal",
        data = {
            x = 0,
            y = 0
        }
    }
}

function energyDrink.sprite(room, entity)
    return drawableSprite.fromTexture("items/food/energydrink/idle00", entity)
end

function energyDrink.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    return utils.rectangle(x - 6, y - 12, 12, 12)
end

return energyDrink
