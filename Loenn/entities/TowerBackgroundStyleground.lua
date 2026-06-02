local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local towerBackgroundStyleground = {}

towerBackgroundStyleground.name = "MaggyHelper/TowerBackgroundStyleground"
towerBackgroundStyleground.depth = 1000
towerBackgroundStyleground.justification = {0.5, 0.5}
towerBackgroundStyleground.canResize = {true, true}

towerBackgroundStyleground.placements = {
    {
        name = "default_background",
        data = {
            backgroundStyle = "Default",
            tintRed = 128,
            tintGreen = 0,
            tintBlue = 128,
            alpha = 0.8,
            parallaxX = 1.0,
            parallaxY = 1.0,
            scrollX = 0.0,
            scrollY = 0.0,
            tileWidth = 64,
            tileHeight = 64,
            autoFindTower = true,
            width = 320,
            height = 240
        }
    },
    {
        name = "mystical_background",
        data = {
            backgroundStyle = "Mystical",
            tintRed = 147,
            tintGreen = 112,
            tintBlue = 219,
            alpha = 0.9,
            parallaxX = 0.8,
            parallaxY = 0.8,
            scrollX = 0.0,
            scrollY = 0.0,
            tileWidth = 64,
            tileHeight = 64,
            autoFindTower = true,
            width = 320,
            height = 240
        }
    },
    {
        name = "dark_background",
        data = {
            backgroundStyle = "Dark",
            tintRed = 72,
            tintGreen = 61,
            tintBlue = 139,
            alpha = 1.0,
            parallaxX = 0.5,
            parallaxY = 0.5,
            scrollX = 0.0,
            scrollY = 0.0,
            tileWidth = 64,
            tileHeight = 64,
            autoFindTower = true,
            width = 320,
            height = 240
        }
    },
    {
        name = "golden_background",
        data = {
            backgroundStyle = "Golden",
            tintRed = 255,
            tintGreen = 215,
            tintBlue = 0,
            alpha = 0.7,
            parallaxX = 1.2,
            parallaxY = 1.2,
            scrollX = 0.0,
            scrollY = 0.0,
            tileWidth = 64,
            tileHeight = 64,
            autoFindTower = true,
            width = 320,
            height = 240
        }
    },
    {
        name = "ethereal_background",
        data = {
            backgroundStyle = "Ethereal",
            tintRed = 224,
            tintGreen = 255,
            tintBlue = 255,
            alpha = 0.6,
            parallaxX = 1.5,
            parallaxY = 1.5,
            scrollX = 0.0,
            scrollY = 0.0,
            tileWidth = 64,
            tileHeight = 64,
            autoFindTower = true,
            width = 320,
            height = 240
        }
    },
    {
        name = "custom_background",
        data = {
            backgroundStyle = "Default",
            tintRed = 128,
            tintGreen = 128,
            tintBlue = 128,
            alpha = 0.8,
            parallaxX = 1.0,
            parallaxY = 1.0,
            scrollX = 0.0,
            scrollY = 0.0,
            tileWidth = 64,
            tileHeight = 64,
            autoFindTower = false,
            width = 320,
            height = 240
        }
    }
}

towerBackgroundStyleground.fieldInformation = {
    backgroundStyle = {
        options = {"Default", "Mystical", "Dark", "Golden", "Ethereal"},
        editable = false
    },
    tintRed = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255
    },
    tintGreen = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255
    },
    tintBlue = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255
    },
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    parallaxX = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0
    },
    parallaxY = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0
    },
    scrollX = {
        fieldType = "number",
        minimumValue = -100.0,
        maximumValue = 100.0
    },
    scrollY = {
        fieldType = "number",
        minimumValue = -100.0,
        maximumValue = 100.0
    },
    tileWidth = {
        fieldType = "integer",
        minimumValue = 16,
        maximumValue = 256
    },
    tileHeight = {
        fieldType = "integer",
        minimumValue = 16,
        maximumValue = 256
    },
    width = {
        fieldType = "integer",
        minimumValue = 64,
        maximumValue = 2048
    },
    height = {
        fieldType = "integer",
        minimumValue = 64,
        maximumValue = 2048
    }
}

local backgroundTextures = {
    Default = "bgs/IngesteHelper/tower_backgrounds/default_pattern",
    Mystical = "bgs/IngesteHelper/tower_backgrounds/mystical_pattern", 
    Dark = "bgs/IngesteHelper/tower_backgrounds/dark_pattern",
    Golden = "bgs/IngesteHelper/tower_backgrounds/golden_pattern",
    Ethereal = "bgs/IngesteHelper/tower_backgrounds/ethereal_pattern"
}

function towerBackgroundStyleground.sprite(room, entity)
    local sprites = {}
    local backgroundStyle = entity.backgroundStyle or "Default"
    
    -- Calculate tint color from RGB values
    local tintR = (entity.tintRed or 128) / 255.0
    local tintG = (entity.tintGreen or 128) / 255.0
    local tintB = (entity.tintBlue or 128) / 255.0
    local alpha = entity.alpha or 0.8
    
    -- Background pattern
    local texture = backgroundTextures[backgroundStyle] or backgroundTextures["Default"]
    local bgSprite = drawableSprite.fromTexture(texture, entity)
    bgSprite:setColor({tintR, tintG, tintB, alpha})
    
    -- Scale to fit the entity size
    local width = entity.width or 320
    local height = entity.height or 240
    bgSprite:setScale(width / 64, height / 64) -- Assuming base texture is 64x64
    
    table.insert(sprites, bgSprite)
    
    -- Parallax indicator overlay
    local parallaxText = string.format("Parallax: %.1f, %.1f", entity.parallaxX or 1.0, entity.parallaxY or 1.0)
    local textSprite = drawableSprite.fromText(parallaxText, entity.x, entity.y - height/2 + 16, "small")
    textSprite:setColor({1.0, 1.0, 1.0, 0.9})
    table.insert(sprites, textSprite)
    
    -- Style indicator
    local styleText = string.format("Style: %s", backgroundStyle)
    local styleSprite = drawableSprite.fromText(styleText, entity.x, entity.y + height/2 - 16, "small")
    styleSprite:setColor({1.0, 1.0, 1.0, 0.9})
    table.insert(sprites, styleSprite)
    
    -- Tower connection indicator (if auto-find is enabled)
    if entity.autoFindTower then
        local connectionSprite = drawableSprite.fromTexture("objects/IngesteHelper/tower_backgrounds/tower_connection", entity)
        connectionSprite:setColor({1.0, 1.0, 0.0, 0.6}) -- Yellow indicator
        connectionSprite.depth = -1
        table.insert(sprites, connectionSprite)
    end
    
    return sprites
end

function towerBackgroundStyleground.selection(room, entity)
    local width = entity.width or 320
    local height = entity.height or 240
    return utils.rectangle(entity.x - width/2, entity.y - height/2, width, height)
end

function towerBackgroundStyleground.minimumSize(room, entity)
    return 64, 64
end

function towerBackgroundStyleground.maximumSize(room, entity)
    return 2048, 2048
end

function towerBackgroundStyleground.resizeRectangle(room, entity)
    local width = entity.width or 320
    local height = entity.height or 240
    return utils.rectangle(entity.x - width/2, entity.y - height/2, width, height)
end

return towerBackgroundStyleground