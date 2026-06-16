-- RySy plugin for DZ - WaterfallClimb (Ch12 Titan Tower)
local Entity = {}

Entity.name = "DZ/WaterfallClimb"
Entity.depth = -50
Entity.minimumSize = {16, 16}
Entity.canResize = {true, true}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 32,
        height = entity.height or 64,
        flowStrength = entity.flowStrength or 80,
        rushInterval = entity.rushInterval or 5.0,
        rushDuration = entity.rushDuration or 2.0
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/waterfall/flowing",
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
