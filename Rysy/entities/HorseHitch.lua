-- RySy plugin for DZ - HorseHitch (Ch11 Western)
local Entity = {}

Entity.name = "DZ/HorseHitch"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        hitchId = entity.hitchId or "",
        destinationId = entity.destinationId or "",
        isUnlocked = entity.isUnlocked or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local state = entity.isUnlocked and "active" or "inactive"
    return {
        texture = "objects/horse_hitch/" .. state,
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
