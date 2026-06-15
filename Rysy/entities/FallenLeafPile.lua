-- RySy plugin for MaggyHelper - FallenLeafPile (Ch10 Ruins)
local Entity = {}

Entity.name = "MaggyHelper/FallenLeafPile"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        hiddenContent = entity.hiddenContent or "Nothing",
        detectionRange = entity.detectionRange or 40,
        enemyType = entity.enemyType or "MaggyHelper/RuinsSentinel",
        collectibleType = entity.collectibleType or ""
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/fallen_leaf_pile/idle",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
