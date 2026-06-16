local torchEntity = {}

torchEntity.name = "DZ/Torch"
torchEntity.depth = 0

torchEntity.placements = {
    {
        name = "Torch",
        data = {
            isLit = true,
            torchType = "wall",
            lightRadius = 64.0
        }
    },
    {
        name = "floor",
        data = {
            isLit = true,
            torchType = "floor",
            lightRadius = 80.0
        }
    },
    {
        name = "magical",
        data = {
            isLit = true,
            torchType = "magical",
            lightRadius = 120.0
        }
    }
}

torchEntity.fieldInformation = {
    isLit = {
        fieldType = "boolean"
    },
    torchType = {
        options = {"wall", "floor", "magical", "eternal"},
        editable = false
    },
    lightRadius = {
        fieldType = "number",
        minimumValue = 16.0,
        maximumValue = 200.0
    }
}

function torchEntity.sprite(room, entity)
    local color = {1.0, 0.8, 0.3, 1.0}
    local texture = "objects/torch/torch_idle"
    
    if entity.torchType == "magical" then
        color = {0.6, 0.3, 1.0, 1.0}
        texture = "objects/torch/magical_torch"
    elseif entity.torchType == "eternal" then
        color = {0.3, 0.9, 1.0, 1.0}
        texture = "objects/torch/eternal_torch"
    elseif not entity.isLit then
        color = {0.4, 0.4, 0.4, 0.8}
        texture = "objects/torch/torch_unlit"
    end
    
    return {
        {
            texture = texture,
            x = entity.x,
            y = entity.y,
            color = color
        }
    }
end

function torchEntity.selection(room, entity)
    return {entity.x - 8, entity.y - 16, 16, 32}
end

return torchEntity
