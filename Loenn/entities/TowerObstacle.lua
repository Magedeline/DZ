local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

local towerObstacle = {}

towerObstacle.name = "DZ/TowerObstacle"
towerObstacle.depth = -50
towerObstacle.justification = {0.5, 0.5}
towerObstacle.canResize = {false, false}

towerObstacle.placements = {
    {
        name = "main",
        data = {
            obstacleType = "Spikes",
            movementPattern = "Static",
            moveSpeed = 50.0,
            rotationSpeed = 1.0,
            activationDelay = 0.0,
            damageRadius = 16.0,
            height = 0.0,
            detectionRange = 100.0
        }
    },
    {
        name = "rotating_spinner",
        data = {
            obstacleType = "Spinner",
            movementPattern = "Circular",
            moveSpeed = 50.0,
            rotationSpeed = 2.0,
            activationDelay = 0.0,
            damageRadius = 20.0,
            height = 0.0,
            detectionRange = 100.0
        }
    },
    {
        name = "moving_platform",
        data = {
            obstacleType = "MovingPlatform",
            movementPattern = "Horizontal",
            moveSpeed = 75.0,
            rotationSpeed = 0.0,
            activationDelay = 0.0,
            damageRadius = 0.0,
            height = 0.0,
            detectionRange = 80.0
        }
    },
    {
        name = "falling_block",
        data = {
            obstacleType = "FallingBlock",
            movementPattern = "Static",
            moveSpeed = 100.0,
            rotationSpeed = 0.0,
            activationDelay = 0.5,
            damageRadius = 16.0,
            height = 0.0,
            detectionRange = 60.0
        }
    },
    {
        name = "laser_beam",
        data = {
            obstacleType = "LaserBeam",
            movementPattern = "Static",
            moveSpeed = 0.0,
            rotationSpeed = 0.0,
            activationDelay = 1.0,
            damageRadius = 8.0,
            height = 0.0,
            detectionRange = 150.0
        }
    },
    {
        name = "wind_tunnel",
        data = {
            obstacleType = "WindTunnel",
            movementPattern = "Static",
            moveSpeed = 0.0,
            rotationSpeed = 0.0,
            activationDelay = 0.0,
            damageRadius = 32.0,
            height = 0.0,
            detectionRange = 120.0
        }
    },
    {
        name = "portal",
        data = {
            obstacleType = "Portal",
            movementPattern = "Static",
            moveSpeed = 0.0,
            rotationSpeed = 0.0,
            activationDelay = 0.0,
            damageRadius = 24.0,
            height = 0.0,
            detectionRange = 50.0
        }
    },
    {
        name = "ping_pong_platform",
        data = {
            obstacleType = "MovingPlatform",
            movementPattern = "PingPong",
            moveSpeed = 60.0,
            rotationSpeed = 0.0,
            activationDelay = 0.0,
            damageRadius = 0.0,
            height = 0.0,
            detectionRange = 100.0
        }
    },
    {
        name = "following_spinner",
        data = {
            obstacleType = "Spinner",
            movementPattern = "Follow",
            moveSpeed = 40.0,
            rotationSpeed = 3.0,
            activationDelay = 0.0,
            damageRadius = 18.0,
            height = 0.0,
            detectionRange = 150.0
        }
    },
    {
        name = "vertical_mover",
        data = {
            obstacleType = "MovingPlatform",
            movementPattern = "Vertical",
            moveSpeed = 50.0,
            rotationSpeed = 0.0,
            activationDelay = 0.0,
            damageRadius = 0.0,
            height = 0.0,
            detectionRange = 80.0
        }
    }
}

towerObstacle.fieldInformation = {
    obstacleType = {
        options = {"Spikes", "Spinner", "MovingPlatform", "FallingBlock", "LaserBeam", "WindTunnel", "Portal"},
        editable = false
    },
    movementPattern = {
        options = {"Static", "Horizontal", "Vertical", "Circular", "PingPong", "Follow"},
        editable = false
    },
    moveSpeed = { fieldType = "number", minimumValue = 0.0, maximumValue = 200.0 },
    rotationSpeed = { fieldType = "number", minimumValue = 0.0, maximumValue = 10.0 },
    activationDelay = { fieldType = "number", minimumValue = 0.0, maximumValue = 5.0 },
    damageRadius = { fieldType = "number", minimumValue = 0.0, maximumValue = 64.0 },
    height = { fieldType = "number", minimumValue = -2000.0, maximumValue = 2000.0 },
    detectionRange = { fieldType = "number", minimumValue = 0.0, maximumValue = 300.0 }
}

local obstacleColors = {
    Spikes = {1.0, 0.3, 0.3, 1.0},
    Spinner = {1.0, 0.8, 0.2, 1.0},
    MovingPlatform = {0.3, 0.8, 0.3, 1.0},
    FallingBlock = {0.8, 0.5, 0.3, 1.0},
    LaserBeam = {1.0, 0.2, 1.0, 1.0},
    WindTunnel = {0.3, 0.8, 1.0, 1.0},
    Portal = {0.8, 0.3, 1.0, 1.0}
}

local obstacleTextures = {
    Spikes = "objects/IngesteHelper/tower_obstacles/spikes",
    Spinner = "objects/IngesteHelper/tower_obstacles/spinner",
    MovingPlatform = "objects/IngesteHelper/tower_obstacles/platform",
    FallingBlock = "objects/IngesteHelper/tower_obstacles/falling_block",
    LaserBeam = "objects/IngesteHelper/tower_obstacles/laser",
    WindTunnel = "objects/IngesteHelper/tower_obstacles/wind",
    Portal = "objects/IngesteHelper/tower_obstacles/portal"
}

function towerObstacle.sprite(room, entity)
    local sprites = {}
    local obstacleType = entity.obstacleType or "Spikes"
    local movementPattern = entity.movementPattern or "Static"

    local texture = obstacleTextures[obstacleType] or obstacleTextures.Spikes
    local mainSprite = drawableSprite.fromTexture(texture, entity)
    local color = obstacleColors[obstacleType] or {1, 1, 1, 1}
    mainSprite:setColor(color)
    table.insert(sprites, mainSprite)

    if movementPattern ~= "Static" then
        local patternSprite = drawableSprite.fromTexture("objects/IngesteHelper/tower_obstacles/movement_indicator", entity)
        patternSprite:setColor({1.0, 1.0, 1.0, 0.6})
        patternSprite.depth = -49
        table.insert(sprites, patternSprite)
    end

    local detectionRange = entity.detectionRange or 0
    if detectionRange > 0 then
        local rangeSprite = drawableSprite.fromTexture("objects/IngesteHelper/tower_obstacles/range_circle", entity)
        rangeSprite:setColor({color[1], color[2], color[3], 0.25})
        -- Assume base circle texture is 64px diameter
        local scale = (detectionRange * 2) / 64
        rangeSprite:setScale(scale, scale)
        rangeSprite.depth = -48
        table.insert(sprites, rangeSprite)
    end

    local label = string.format("%s (%s)", obstacleType, movementPattern)
    local textSprite = drawableText.fromText(label, entity.x, entity.y - 24, {
        font = "small",
        justificationX = 0.5,
        justificationY = 0.5
    })
    textSprite:setColor({1, 1, 1, 0.85})
    table.insert(sprites, textSprite)

    return sprites
end

function towerObstacle.selection(room, entity)
    local detectionRange = entity.detectionRange or 0
    local diameter = math.max(detectionRange * 2, 16)
    return utils.rectangle(entity.x - diameter / 2, entity.y - diameter / 2, diameter, diameter)
end

function towerObstacle.nodeSprite(room, entity, node, nodeIndex)
    local nodeSprite = drawableSprite.fromTexture("objects/IngesteHelper/tower_obstacles/node", { x = node.x, y = node.y })
    nodeSprite:setColor({1.0, 1.0, 0.0, 0.8})
    return nodeSprite
end

function towerObstacle.nodeLineRenderType()
    return "line"
end

return towerObstacle