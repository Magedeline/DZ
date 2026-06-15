-- RySy plugin for MaggyHelper - TowerLantern (Ch12 Titan Tower)
local Entity = {}

Entity.name = "MaggyHelper/TowerLantern"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        lightRadius = entity.lightRadius or 80,
        flickerIntensity = entity.flickerIntensity or 0.1,
        lanternId = entity.lanternId or "",
        startLit = entity.startLit or false,
        lightColor = entity.lightColor or "FFA500"
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local state = entity.startLit and "lit" or "unlit"
    return {
        texture = "objects/tower_lantern/" .. state,
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
