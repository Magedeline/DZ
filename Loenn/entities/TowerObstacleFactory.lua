local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")

local towerObstacleFactory = {}

towerObstacleFactory.name = "DZ/TowerObstacleFactory"
towerObstacleFactory.depth = -200
towerObstacleFactory.justification = {0.5, 0.5}
towerObstacleFactory.canResize = {false, false}

towerObstacleFactory.placements = {
    {
        name = "beginner_set",
        data = {
            obstacleSetType = "Beginner",
            obstaclePattern = "Rings",
            backgroundStyle = "Default",
            createBackground = true,
            createObstacles = true,
            autoPositionAroundTower = true,
            towerRadius = 120.0,
            obstacleCount = 15,
            verticalSpacing = 150.0,
            patternRotation = 0.0,
            activationDelay = 0.0
        }
    },
    {
        name = "intermediate_set",
        data = {
            obstacleSetType = "Intermediate",
            obstaclePattern = "Spiral",
            backgroundStyle = "Mystical",
            createBackground = true,
            createObstacles = true,
            autoPositionAroundTower = true,
            towerRadius = 120.0,
            obstacleCount = 25,
            verticalSpacing = 120.0,
            patternRotation = 0.0,
            activationDelay = 0.0
        }
    },
    {
        name = "advanced_set",
        data = {
            obstacleSetType = "Advanced",
            obstaclePattern = "Zigzag",
            backgroundStyle = "Dark",
            createBackground = true,
            createObstacles = true,
            autoPositionAroundTower = true,
            towerRadius = 120.0,
            obstacleCount = 35,
            verticalSpacing = 100.0,
            patternRotation = 0.0,
            activationDelay = 0.0
        }
    },
    {
        name = "expert_set",
        data = {
            obstacleSetType = "Expert",
            obstaclePattern = "Gauntlet",
            backgroundStyle = "Golden",
            createBackground = true,
            createObstacles = true,
            autoPositionAroundTower = true,
            towerRadius = 120.0,
            obstacleCount = 50,
            verticalSpacing = 80.0,
            patternRotation = 0.0,
            activationDelay = 0.0
        }
    },
    {
        name = "random_set",
        data = {
            obstacleSetType = "Random",
            obstaclePattern = "Spiral",
            backgroundStyle = "Ethereal",
            createBackground = true,
            createObstacles = true,
            autoPositionAroundTower = true,
            towerRadius = 120.0,
            obstacleCount = 30,
            verticalSpacing = 100.0,
            patternRotation = 0.0,
            activationDelay = 0.0
        }
    },
    {
        name = "pattern_only",
        data = {
            obstacleSetType = "Intermediate",
            obstaclePattern = "Rings",
            backgroundStyle = "Default",
            createBackground = false,
            createObstacles = true,
            autoPositionAroundTower = false,
            towerRadius = 120.0,
            obstacleCount = 20,
            verticalSpacing = 120.0,
            patternRotation = 0.0,
            activationDelay = 0.0
        }
    },
    {
        name = "background_only",
        data = {
            obstacleSetType = "Beginner",
            obstaclePattern = "Spiral",
            backgroundStyle = "Mystical",
            createBackground = true,
            createObstacles = false,
            autoPositionAroundTower = true,
            towerRadius = 120.0,
            obstacleCount = 0,
            verticalSpacing = 120.0,
            patternRotation = 0.0,
            activationDelay = 0.0
        }
    },
    {
        name = "complete_setup",
        data = {
            obstacleSetType = "Advanced",
            obstaclePattern = "Gauntlet",
            backgroundStyle = "Dark",
            createBackground = true,
            createObstacles = true,
            autoPositionAroundTower = true,
            towerRadius = 120.0,
            obstacleCount = 40,
            verticalSpacing = 90.0,
            patternRotation = 0.0,
            activationDelay = 1.0
        }
    }
}

towerObstacleFactory.fieldInformation = {
    obstacleSetType = {
        options = {"Beginner", "Intermediate", "Advanced", "Expert", "Random"},
        editable = false
    },
    obstaclePattern = {
        options = {"Spiral", "Rings", "Zigzag", "Gauntlet"},
        editable = false
    },
    backgroundStyle = {
        options = {"Default", "Mystical", "Dark", "Golden", "Ethereal"},
        editable = false
    },
    towerRadius = {
        fieldType = "number",
        minimumValue = 60.0,
        maximumValue = 300.0
    },
    obstacleCount = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 100
    },
    verticalSpacing = {
        fieldType = "number",
        minimumValue = 50.0,
        maximumValue = 300.0
    },
    patternRotation = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2 * math.pi
    },
    activationDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    }
}

local setTypeColors = {
    Beginner = {0.2, 1.0, 0.2, 0.8},
    Intermediate = {0.2, 0.8, 1.0, 0.8},
    Advanced = {1.0, 0.8, 0.2, 0.8},
    Expert = {1.0, 0.2, 0.2, 0.8},
    Random = {1.0, 0.2, 1.0, 0.8}
}

local patternIcons = {
    Spiral = "objects/IngesteHelper/tower_factory/spiral_icon",
    Rings = "objects/IngesteHelper/tower_factory/rings_icon",
    Zigzag = "objects/IngesteHelper/tower_factory/zigzag_icon",
    Gauntlet = "objects/IngesteHelper/tower_factory/gauntlet_icon"
}

function towerObstacleFactory.sprite(room, entity)
    local sprites = {}
    local obstacleSetType = entity.obstacleSetType or "Intermediate"
    local obstaclePattern = entity.obstaclePattern or "Rings"
    local createBackground = entity.createBackground ~= false
    local createObstacles = entity.createObstacles ~= false

    local factorySprite = drawableSprite.fromTexture("objects/IngesteHelper/tower_factory/factory", entity)
    local color = setTypeColors[obstacleSetType] or {0.5, 0.5, 0.5, 0.8}
    factorySprite:setColor(color)
    table.insert(sprites, factorySprite)

    local patternTexture = patternIcons[obstaclePattern] or patternIcons["Rings"]
    local patternSprite = drawableSprite.fromTexture(patternTexture, entity)
    patternSprite:setColor({1.0, 1.0, 1.0, 0.9})
    patternSprite.depth = -199
    table.insert(sprites, patternSprite)

    if createBackground then
        local bgSprite = drawableSprite.fromTexture("objects/IngesteHelper/tower_factory/background_indicator",
            { x = entity.x - 16, y = entity.y - 16 })
        bgSprite:setColor({0.8, 0.8, 1.0, 0.7})
        table.insert(sprites, bgSprite)
    end

    if createObstacles then
        local obsSprite = drawableSprite.fromTexture("objects/IngesteHelper/tower_factory/obstacle_indicator",
            { x = entity.x + 16, y = entity.y - 16 })
        obsSprite:setColor({1.0, 0.8, 0.8, 0.7})
        table.insert(sprites, obsSprite)
    end

    local difficultySprite = drawableText.fromText(obstacleSetType, entity.x, entity.y - 32, {
        font = "small",
        justificationX = 0.5,
        justificationY = 0.5
    })
    difficultySprite:setColor(color)
    table.insert(sprites, difficultySprite)

    local patternLabelSprite = drawableText.fromText(obstaclePattern, entity.x, entity.y + 32, {
        font = "small",
        justificationX = 0.5,
        justificationY = 0.5
    })
    patternLabelSprite:setColor({1.0, 1.0, 1.0, 0.9})
    table.insert(sprites, patternLabelSprite)

    local countText = string.format("Count: %d", entity.obstacleCount or 0)
    local countSprite = drawableText.fromText(countText, entity.x, entity.y + 44, {
        font = "small",
        justificationX = 0.5,
        justificationY = 0.5
    })
    countSprite:setColor({0.8, 0.8, 0.8, 0.8})
    table.insert(sprites, countSprite)

    if entity.autoPositionAroundTower then
        local radius = entity.towerRadius or 120
        local radiusSprite = drawableSprite.fromTexture("objects/IngesteHelper/tower_factory/radius_circle", entity)
        radiusSprite:setColor({color[1], color[2], color[3], 0.25})
        local scale = (radius * 2) / 64 -- assume texture is a 64px diameter circle
        radiusSprite:setScale(scale, scale)
        radiusSprite.depth = -198
        table.insert(sprites, radiusSprite)
    end

    return sprites
end

function towerObstacleFactory.selection(room, entity)
    if entity.autoPositionAroundTower then
        local r = entity.towerRadius or 120
        local d = r * 2
        return utils.rectangle(entity.x - r, entity.y - r, d, d)
    end
    return utils.rectangle(entity.x - 16, entity.y - 16, 32, 32)
end

function towerObstacleFactory.nodeSprite(room, entity, node, nodeIndex)
    local nodeSprite = drawableSprite.fromTexture("objects/IngesteHelper/tower_factory/spawn_point", { x = node.x, y = node.y })
    nodeSprite:setColor({1.0, 1.0, 0.0, 0.8})
    return nodeSprite
end

function towerObstacleFactory.nodeLineRenderType()
    return "line"
end

return towerObstacleFactory