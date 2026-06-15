-- RySy plugin for MaggyHelper - AncientSwitch
local Entity = {}

Entity.name = "MaggyHelper/AncientSwitch"
Entity.depth = 0

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        isActivated = entity.isActivated or false,
        switchType = entity.switchType or "pressure",
        targetEntity = entity.targetEntity or "",
        persistent = entity.persistent ~= false,
        requiresWeight = entity.requiresWeight or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/temple/switch00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
