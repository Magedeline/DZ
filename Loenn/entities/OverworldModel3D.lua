-- 3D Overworld Model Entity for Lönn Map Editor
-- Creates 3D models that are visible both in the overworld and in the map editor

local overworldModel3D = {}

overworldModel3D.name = "MaggyHelper/OverworldModel3D"
overworldModel3D.depth = -9000
overworldModel3D.justification = {0.5, 1.0}

overworldModel3D.placements = {
    {
        name = "tower_model",
        data = {
            modelType = "tower",
            modelScale = 1.0,
            rotationX = 0.0,
            rotationY = 0.0,
            rotationZ = 0.0,
            positionX = 0.0,
            positionY = 0.0,
            positionZ = 0.0,
            materialType = "stone",
            lightingEnabled = true,
            shadowsEnabled = true,
            overworldVisible = true,
            mapEditorVisible = true,
            animationType = "idle",
            animationSpeed = 1.0,
            collisionEnabled = true,
            interactable = false,
            renderPriority = 0
        }
    },
    {
        name = "building_model",
        data = {
            modelType = "building",
            modelScale = 1.5,
            rotationX = 0.0,
            rotationY = 0.0,
            rotationZ = 0.0,
            positionX = 0.0,
            positionY = 0.0,
            positionZ = -100.0,
            materialType = "concrete",
            lightingEnabled = true,
            shadowsEnabled = true,
            overworldVisible = true,
            mapEditorVisible = true,
            animationType = "none",
            animationSpeed = 0.0,
            collisionEnabled = true,
            interactable = true,
            renderPriority = -1
        }
    },
    {
        name = "mountain_model",
        data = {
            modelType = "mountain",
            modelScale = 3.0,
            rotationX = 0.0,
            rotationY = 0.0,
            rotationZ = 0.0,
            positionX = 0.0,
            positionY = 0.0,
            positionZ = -500.0,
            materialType = "rock",
            lightingEnabled = true,
            shadowsEnabled = true,
            overworldVisible = true,
            mapEditorVisible = false,
            animationType = "none",
            animationSpeed = 0.0,
            collisionEnabled = false,
            interactable = false,
            renderPriority = -2
        }
    },
    {
        name = "crystal_model",
        data = {
            modelType = "crystal",
            modelScale = 0.8,
            rotationX = 0.0,
            rotationY = 45.0,
            rotationZ = 0.0,
            positionX = 0.0,
            positionY = -50.0,
            positionZ = 0.0,
            materialType = "crystal",
            lightingEnabled = true,
            shadowsEnabled = false,
            overworldVisible = true,
            mapEditorVisible = true,
            animationType = "rotate",
            animationSpeed = 0.5,
            collisionEnabled = true,
            interactable = true,
            renderPriority = 1
        }
    },
    {
        name = "floating_platform",
        data = {
            modelType = "platform",
            modelScale = 1.2,
            rotationX = 0.0,
            rotationY = 0.0,
            rotationZ = 0.0,
            positionX = 0.0,
            positionY = -100.0,
            positionZ = 0.0,
            materialType = "metal",
            lightingEnabled = true,
            shadowsEnabled = true,
            overworldVisible = true,
            mapEditorVisible = true,
            animationType = "float",
            animationSpeed = 1.5,
            collisionEnabled = true,
            interactable = false,
            renderPriority = 0
        }
    }
}

overworldModel3D.fieldInformation = {
    modelType = {
        options = {"tower", "building", "mountain", "crystal", "platform", "bridge", "statue", "tree", "rock", "custom"},
        editable = false
    },
    modelScale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    rotationX = {
        fieldType = "number",
        minimumValue = -360.0,
        maximumValue = 360.0
    },
    rotationY = {
        fieldType = "number",
        minimumValue = -360.0,
        maximumValue = 360.0
    },
    rotationZ = {
        fieldType = "number",
        minimumValue = -360.0,
        maximumValue = 360.0
    },
    positionX = {
        fieldType = "number",
        minimumValue = -2000.0,
        maximumValue = 2000.0
    },
    positionY = {
        fieldType = "number",
        minimumValue = -2000.0,
        maximumValue = 2000.0
    },
    positionZ = {
        fieldType = "number",
        minimumValue = -2000.0,
        maximumValue = 2000.0
    },
    materialType = {
        options = {"stone", "concrete", "rock", "crystal", "metal", "wood", "glass", "ice", "lava", "energy"},
        editable = false
    },
    animationType = {
        options = {"none", "idle", "rotate", "float", "pulse", "wave", "spin", "bounce", "custom"},
        editable = false
    },
    animationSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0
    },
    renderPriority = {
        fieldType = "integer",
        minimumValue = -10,
        maximumValue = 10
    }
}

-- Sprite and rendering
overworldModel3D.texture = "objects/IngesteHelper/overworld3d/base"

function overworldModel3D.sprite(room, entity)
    local modelType = entity.modelType or "tower"
    local scale = entity.modelScale or 1.0
    local sprites = {}
    
    -- Main model representation
    local mainSprite = {
        texture = "objects/IngesteHelper/overworld3d/" .. modelType,
        x = entity.x + (entity.positionX or 0),
        y = entity.y + (entity.positionY or 0),
        scaleX = scale,
        scaleY = scale,
        rotation = math.rad(entity.rotationZ or 0),
        justificationX = 0.5,
        justificationY = 1.0
    }
    table.insert(sprites, mainSprite)
    
    -- 3D depth indicator
    if entity.positionZ and entity.positionZ ~= 0 then
        local depthOffset = (entity.positionZ or 0) / 10
        local shadowSprite = {
            texture = "objects/IngesteHelper/overworld3d/" .. modelType .. "_shadow",
            x = entity.x + depthOffset,
            y = entity.y + depthOffset,
            scaleX = scale * 0.8,
            scaleY = scale * 0.8,
            alpha = 0.3,
            color = {0.2, 0.2, 0.2},
            justificationX = 0.5,
            justificationY = 1.0
        }
        table.insert(sprites, shadowSprite)
    end
    
    -- Lighting indicator
    if entity.lightingEnabled then
        local lightSprite = {
            texture = "objects/IngesteHelper/icons/light_icon",
            x = entity.x + 20,
            y = entity.y - 20,
            scaleX = 0.5,
            scaleY = 0.5
        }
        table.insert(sprites, lightSprite)
    end
    
    -- Animation indicator
    if entity.animationType and entity.animationType ~= "none" then
        local animSprite = {
            texture = "objects/IngesteHelper/icons/anim_icon",
            x = entity.x - 20,
            y = entity.y - 20,
            scaleX = 0.5,
            scaleY = 0.5
        }
        table.insert(sprites, animSprite)
    end
    
    -- Overworld visibility indicator
    if entity.overworldVisible then
        local overworldSprite = {
            texture = "objects/IngesteHelper/icons/overworld_icon",
            x = entity.x,
            y = entity.y - 40,
            scaleX = 0.6,
            scaleY = 0.6
        }
        table.insert(sprites, overworldSprite)
    end
    
    return sprites
end

function overworldModel3D.selection(room, entity)
    local scale = entity.modelScale or 1.0
    local baseSize = 32
    local size = baseSize * scale
    
    return utils.rectangle(
        entity.x - size/2,
        entity.y - size,
        size,
        size
    )
end

-- Node support for model waypoints
overworldModel3D.nodeLimits = {0, -1}
overworldModel3D.nodeLineRenderType = "line"

function overworldModel3D.nodeSprite(room, entity, node, nodeIndex)
    return {
        texture = "objects/IngesteHelper/overworld3d/waypoint",
        x = node.x,
        y = node.y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return overworldModel3D