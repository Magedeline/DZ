local metaKnightDummy = {}

metaKnightDummy.name = "MaggyHelper/MetaKnightDummy"
metaKnightDummy.depth = 0
metaKnightDummy.texture = "characters/meta_knight/idle00"
metaKnightDummy.justification = {0.5, 1.0}

metaKnightDummy.fieldInformation = {
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
            "sword_ready",
            "cape_flourish",
            "masked",
            "unmasked",
            "flying"
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
    hasWings = {
        fieldType = "boolean"
    },
    hasSword = {
        fieldType = "boolean"
    }
}

metaKnightDummy.placements = {
    {
        name = "normal",
        data = {
            facing = 1,
            animation = "idle",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = false,
            hasWings = true,
            hasSword = true
        }
    },
    {
        name = "sword_ready",
        data = {
            facing = 1,
            animation = "sword_ready",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true,
            hasWings = true,
            hasSword = true
        }
    },
    {
        name = "flying",
        data = {
            facing = 1,
            animation = "flying",
            scale = 1.2,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true,
            hasWings = true,
            hasSword = false
        }
    }
}

function metaKnightDummy.sprite(room, entity)
    local facing = entity.facing or 1
    local scale = entity.scale or 1.0
    local alpha = entity.alpha or 1.0
    
    return {
        texture = "characters/meta_knight/idle00",
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = facing * scale,
        scaleY = scale,
        color = {1.0, 1.0, 1.0, alpha}
    }
end

return metaKnightDummy