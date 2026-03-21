local kingDDDDummy = {}

kingDDDDummy.name = "MaggyHelper/KingDDDDummy"
kingDDDDummy.depth = 0
kingDDDDummy.texture = "characters/king_ddd/idle00"
kingDDDDummy.justification = {0.5, 1.0}

kingDDDDummy.fieldInformation = {
    facing = {
        fieldType = "integer",
        options = {-1, 1},
        editable = false
    },
    animation = {
        fieldType = "string",
        options = {
            "idle",
            "walk",
            "royal_pose",
            "hammer_ready",
            "laughing",
            "angry",
            "sleeping"
        },
        editable = true
    },
    scale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 3.0
    },
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    isVisible = {
        fieldType = "boolean"
    },
    playAnimationOnSpawn = {
        fieldType = "boolean"
    },
    hasHammer = {
        fieldType = "boolean"
    },
    hasCrown = {
        fieldType = "boolean"
    }
}

kingDDDDummy.placements = {
    {
        name = "normal",
        data = {
            facing = 1,
            animation = "idle",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = false,
            hasHammer = true,
            hasCrown = true
        }
    },
    {
        name = "royal_pose",
        data = {
            facing = 1,
            animation = "royal_pose",
            scale = 1.2,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true,
            hasHammer = false,
            hasCrown = true
        }
    },
    {
        name = "battle_ready",
        data = {
            facing = 1,
            animation = "hammer_ready",
            scale = 1.1,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true,
            hasHammer = true,
            hasCrown = true
        }
    }
}

function kingDDDDummy.sprite(room, entity)
    local facing = entity.facing or 1
    local scale = entity.scale or 1.0
    local alpha = entity.alpha or 1.0
    
    return {
        texture = "characters/king_ddd/idle00",
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = facing * scale,
        scaleY = scale,
        color = {1.0, 1.0, 1.0, alpha}
    }
end

return kingDDDDummy