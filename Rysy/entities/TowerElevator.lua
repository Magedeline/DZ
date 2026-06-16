-- RySy plugin for DZ - TowerElevator (Ch12 Titan Tower)
local Entity = {}

Entity.name = "DZ/TowerElevator"
Entity.depth = -100
Entity.minimumSize = {24, 8}
Entity.canResize = {true, true}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 32,
        height = entity.height or 8,
        moveSpeed = entity.moveSpeed or 80,
        waitTime = entity.waitTime or 1.0,
        elevatorId = entity.elevatorId or ""
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/tower_elevator/idle",
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
