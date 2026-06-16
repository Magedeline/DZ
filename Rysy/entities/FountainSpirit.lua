-- RySy plugin for DZ - FountainSpirit (Ch12 Titan Tower)
local Entity = {}

Entity.name = "DZ/FountainSpirit"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        spiritType = entity.spiritType or "Healing",
        healAmount = entity.healAmount or 3,
        buffDuration = entity.buffDuration or 10.0
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/fountain_spirit/dormant",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
