-- RySy plugin for DZ - Chara Boss
local Entity = {}

Entity.name = "DZ/CharaBoss"
Entity.depth = -8500

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        patternIndex = entity.patternIndex or 0,
        cameraPastY = entity.cameraPastY or 120.0,
        dialog = entity.dialog or false,
        startHit = entity.startHit or false,
        cameraLockY = entity.cameraLockY ~= false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/charaBoss/boss00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
