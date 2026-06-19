local void_gate = {}

void_gate.name = "DZ/VoidGate"
void_gate.depth = -10000
void_gate.canResize = {true, true}
void_gate.placements = {
    {
        name = "main",
        data = {
            width = 160,
            height = 128,
            gateWidth = 16,
            gateHeight = 128,
            triggerWidth = 96,
            triggerHeight = 128,
            moveSpeed = 100
        }
    }
}

void_gate.fieldInformation = {
    gateWidth = {
        fieldType = "number",
        minimumValue = 8,
        maximumValue = 32
    },
    gateHeight = {
        fieldType = "number",
        minimumValue = 32,
        maximumValue = 256
    },
    triggerWidth = {
        fieldType = "number",
        minimumValue = 32,
        maximumValue = 200
    },
    triggerHeight = {
        fieldType = "number",
        minimumValue = 32,
        maximumValue = 256
    },
    moveSpeed = {
        fieldType = "number",
        minimumValue = 10,
        maximumValue = 300
    }
}

function void_gate.sprite(room, entity)
    local sprites = {}
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 160
    local height = entity.height or 128
    local gateWidth = entity.gateWidth or 16
    local triggerWidth = entity.triggerWidth or 96
    
    -- Draw arena bounds
    table.insert(sprites, {
        texture = "ahorn/entityTrigger",
        x = x,
        y = y,
        scaleX = width / 8,
        scaleY = height / 8,
        color = {0.3, 0.0, 0.5, 0.4}
    })
    
    -- Draw left gate position (open)
    table.insert(sprites, {
        texture = "ahorn/entitySolid",
        x = x - gateWidth,
        y = y,
        scaleX = gateWidth / 8,
        scaleY = height / 8,
        color = {0.5, 0.0, 0.8, 0.8}
    })
    
    -- Draw right gate position (open)
    table.insert(sprites, {
        texture = "ahorn/entitySolid",
        x = x + width,
        y = y,
        scaleX = gateWidth / 8,
        scaleY = height / 8,
        color = {0.5, 0.0, 0.8, 0.8}
    })
    
    -- Draw trigger zone
    local centerX = x + width / 2
    table.insert(sprites, {
        texture = "ahorn/entityTrigger",
        x = centerX - triggerWidth / 2,
        y = y,
        scaleX = triggerWidth / 8,
        scaleY = height / 8,
        color = {1.0, 1.0, 0.0, 0.3}
    })
    
    return sprites
end

function void_gate.rectangle(room, entity)
    return {
        x = entity.x,
        y = entity.y,
        width = entity.width or 160,
        height = entity.height or 128
    }
end

return void_gate
