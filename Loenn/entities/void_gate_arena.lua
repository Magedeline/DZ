local voidGateArena = {}

voidGateArena.name = "DZ/VoidGateArena"
voidGateArena.depth = -10000
voidGateArena.justification = {0.5, 0.5}
voidGateArena.nodeLimits = {0, -1}
voidGateArena.nodeLineRenderType = "line"

voidGateArena.placements = {
    {
        name = "void_gate_arena_easy",
        data = {
            requiredKills = 8,
            spawnBoss = false,
            enemiesPerWave = 2,
            totalWaves = 4,
            completionFlag = "void_gate_arena_complete"
        }
    },
    {
        name = "void_gate_arena_normal",
        data = {
            requiredKills = 12,
            spawnBoss = true,
            enemiesPerWave = 3,
            totalWaves = 3,
            completionFlag = "void_gate_arena_complete"
        }
    },
    {
        name = "void_gate_arena_hard",
        data = {
            requiredKills = 18,
            spawnBoss = true,
            enemiesPerWave = 4,
            totalWaves = 4,
            completionFlag = "void_gate_arena_complete"
        }
    }
}

voidGateArena.fieldInformation = {
    requiredKills = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 50
    },
    spawnBoss = {
        fieldType = "boolean"
    },
    enemiesPerWave = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    totalWaves = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    completionFlag = {
        fieldType = "string"
    }
}

function voidGateArena.texture(room, entity)
    return "ahorn/entity"
end

function voidGateArena.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    
    local sprites = {}
    
    -- Arena center marker
    table.insert(sprites, {
        texture = "ahorn/entity",
        x = x,
        y = y,
        color = {0.8, 0.0, 0.8, 1.0}
    })
    
    return sprites
end

function voidGateArena.nodeSprite(room, entity, node, nodeIndex, viewport)
    local x, y = node.x, node.y
    local sprites = {}
    
    -- Enemy spawn point marker
    table.insert(sprites, {
        texture = "ahorn/entity_node",
        x = x,
        y = y,
        color = {0.8, 0.0, 0.0, 0.8}
    })
    
    return sprites
end

function voidGateArena.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    
    return {
        x = x - 8,
        y = y - 8,
        width = 16,
        height = 16
    }
end

return voidGateArena
