-- RySy plugin for DZ - Asriel God Boss
local Entity = {}

Entity.name = "DZ/AsrielGodBoss"
Entity.depth = 0

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        patternIndex = entity.patternIndex or 0,
        cameraPastY = entity.cameraPastY or 120.0,
        dialog = entity.dialog ~= false,
        startHit = entity.startHit or false,
        cameraLockY = entity.cameraLockY ~= false,
        attackSequence = entity.attackSequence or ""
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/asrielgodboss/boss00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
