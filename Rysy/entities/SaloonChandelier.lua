-- RySy plugin for DZ - SaloonChandelier (Ch11 Western)
local Entity = {}

Entity.name = "DZ/SaloonChandelier"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        swingPeriod = entity.swingPeriod or 3.0,
        swingAngle = entity.swingAngle or 0.4,
        chainLength = entity.chainLength or 80,
        canFall = entity.canFall ~= false,
        isHazard = entity.isHazard ~= false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/saloon_chandelier/swinging",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.0
    }
end

return Entity
