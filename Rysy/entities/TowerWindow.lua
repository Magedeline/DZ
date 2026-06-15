-- RySy plugin for MaggyHelper - TowerWindow (Ch12 Titan Tower)
local Entity = {}

Entity.name = "MaggyHelper/TowerWindow"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        view = entity.view or "Sky",
        lightIntensity = entity.lightIntensity or 0.6,
        startLit = entity.startLit ~= false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/tower_window_frame/default",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
