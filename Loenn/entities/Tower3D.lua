-- 3D Tower Entity for L�nn Map Editor
-- Provides a climbable 3D tower with customizable obstacles and rotation

local tower3D = {}

tower3D.name = "DZ/Tower3D"
tower3D.depth = -8500
tower3D.justification = {0.5, 1.0}
tower3D.placements = {
    {
        name = "basic_tower",
        data = {
            towerHeight = 1000,
            radius = 120,
            rotationSpeed = 1.0,
            climbingSpeed = 100,
            climbingEnabled = true,
            obstacleSetType = "intermediate",
            backgroundStyle = "default",
            autoCreateObstacles = true,
            segments = 8,
            renderIn3D = true,
            overworldVisible = true,
            overworldScale = 1.0,
            overworldPosition = "center"
        }
    },
    {
        name = "beginner_tower",
        data = {
            towerHeight = 800,
            radius = 100,
            rotationSpeed = 0.5,
            climbingSpeed = 80,
            climbingEnabled = true,
            obstacleSetType = "beginner",
            backgroundStyle = "tutorial",
            autoCreateObstacles = true,
            segments = 6,
            renderIn3D = true,
            overworldVisible = true,
            overworldScale = 0.8,
            overworldPosition = "left"
        }
    },
    {
        name = "expert_tower",
        data = {
            towerHeight = 1500,
            radius = 150,
            rotationSpeed = 2.0,
            climbingSpeed = 120,
            climbingEnabled = true,
            obstacleSetType = "expert",
            backgroundStyle = "dramatic",
            autoCreateObstacles = true,
            segments = 12,
            renderIn3D = true,
            overworldVisible = true,
            overworldScale = 1.5,
            overworldPosition = "right"
        }
    },
    {
        name = "overworld_model",
        data = {
            towerHeight = 500,
            radius = 80,
            rotationSpeed = 0.2,
            climbingSpeed = 0,
            climbingEnabled = false,
            obstacleSetType = "none",
            backgroundStyle = "overworld",
            autoCreateObstacles = false,
            segments = 4,
            renderIn3D = true,
            overworldVisible = true,
            overworldScale = 2.0,
            overworldPosition = "background"
        }
    }
}

tower3D.fieldInformation = {
    towerHeight = {
        fieldType = "integer",
        minimumValue = 200,
        maximumValue = 3000
    },
    radius = {
        fieldType = "integer", 
        minimumValue = 50,
        maximumValue = 300
    },
    rotationSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    climbingSpeed = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 200
    },
    obstacleSetType = {
        options = {"none", "beginner", "intermediate", "expert", "custom"},
        editable = false
    },
    backgroundStyle = {
        options = {"default", "tutorial", "dramatic", "overworld", "space", "underground", "crystal"},
        editable = false
    },
    segments = {
        fieldType = "integer",
        minimumValue = 3,
        maximumValue = 16
    },
    overworldScale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },
    overworldPosition = {
        options = {"center", "left", "right", "background", "foreground"},
        editable = false
    }
}

-- Sprite and rendering
tower3D.texture = "objects/IngesteHelper/tower3d/base"
tower3D.nodeLineRenderType = "line"

-- Visual helpers for L�nn editor
function tower3D.sprite(room, entity)
    local radius = entity.radius or 120
    local height = entity.towerHeight or 1000
    local segments = entity.segments or 8
    
    -- Main tower base sprite
    local sprites = {}
    
    -- Base circle
    local baseSprite = {
        texture = "objects/IngesteHelper/tower3d/base",
        x = entity.x,
        y = entity.y,
        scaleX = radius / 64,
        scaleY = radius / 64,
        justificationX = 0.5,
        justificationY = 1.0
    }
    table.insert(sprites, baseSprite)
    
    -- Tower segments visualization
    for i = 1, math.min(segments, 8) do
        local segmentY = entity.y - (height * i / segments)
        local segmentScale = (radius - i * 5) / 64
        
        local segmentSprite = {
            texture = "objects/IngesteHelper/tower3d/segment",
            x = entity.x,
            y = segmentY,
            scaleX = segmentScale,
            scaleY = 0.3,
            justificationX = 0.5,
            justificationY = 0.5,
            alpha = 0.7 - (i * 0.1)
        }
        table.insert(sprites, segmentSprite)
    end
    
    -- 3D indicator
    if entity.renderIn3D then
        local indicator = {
            texture = "objects/IngesteHelper/icons/3d_icon",
            x = entity.x + radius + 16,
            y = entity.y - 16,
            scaleX = 0.5,
            scaleY = 0.5
        }
        table.insert(sprites, indicator)
    end
    
    -- Overworld indicator
    if entity.overworldVisible then
        local overworldIcon = {
            texture = "objects/IngesteHelper/icons/overworld_icon",
            x = entity.x - radius - 16,
            y = entity.y - 16,
            scaleX = entity.overworldScale or 1.0,
            scaleY = entity.overworldScale or 1.0
        }
        table.insert(sprites, overworldIcon)
    end
    
    return sprites
end

-- Selection rectangle for easier selection
function tower3D.selection(room, entity)
    local radius = entity.radius or 120
    local height = entity.towerHeight or 1000
    
    return utils.rectangle(
        entity.x - radius, 
        entity.y - height, 
        radius * 2, 
        height
    )
end

-- Node handling for tower waypoints/checkpoints
tower3D.nodeLimits = {0, -1} -- Unlimited nodes
tower3D.nodeLineRenderType = "fan"

function tower3D.nodeSprite(room, entity, node, nodeIndex)
    return {
        texture = "objects/IngesteHelper/tower3d/checkpoint",
        x = node.x,
        y = node.y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return tower3D