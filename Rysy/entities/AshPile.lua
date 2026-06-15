-- RySy plugin for MaggyHelper - AshPile (Ch13 Inferno)
local Entity = {}

Entity.name = "MaggyHelper/AshPile"
Entity.depth = -100
Entity.minimumSize = {8, 8}
Entity.canResize = {true, true}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 32,
        height = entity.height or 16,
        climbSlowFactor = entity.climbSlowFactor or 0.5,
        shiftInterval = entity.shiftInterval or 3.0,
        isClimbable = entity.isClimbable ~= false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/ash_pile/stable00",
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
