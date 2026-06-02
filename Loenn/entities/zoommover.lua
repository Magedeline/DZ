local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local zoomMover = {}

zoomMover.name = "MaggyHelper/ZoomMover"
zoomMover.depth = -9999
zoomMover.nodeLimits = {1, 1}
zoomMover.nodeLineRenderType = "line"
zoomMover.warnBelowSize = {16, 16}

zoomMover.placements = {
    {
        name = "normal",
        data = {
            width = 16,
            height = 16,
            theme = "Normal",
            moveSpeed = 300.0,
            permanent = false,
            waits = false,
            timed = false
        }
    },
    {
        name = "moon",
        data = {
            width = 16,
            height = 16,
            theme = "Moon",
            moveSpeed = 300.0,
            permanent = false,
            waits = false,
            timed = false
        }
    },
    {
        name = "foundlevels",
        data = {
            width = 16,
            height = 16,
            theme = "FoundLevels",
            moveSpeed = 250.0,
            permanent = false,
            waits = true,
            timed = false
        }
    },
    {
        name = "finallevels",
        data = {
            width = 16,
            height = 16,
            theme = "FinalLevels",
            moveSpeed = 350.0,
            permanent = false,
            waits = false,
            timed = true
        }
    }
}

zoomMover.fieldInformation = {
    theme = {
        options = {"Normal", "Moon", "FoundLevels", "FinalLevels"},
        editable = false
    },
    moveSpeed = {
        fieldType = "number",
        minimumValue = 50.0,
        maximumValue = 1000.0
    }
}

local themeColors = {
    Normal = {0.4, 0.2, 0.2, 1.0},
    Moon = {0.2, 0.2, 0.4, 1.0},
    FoundLevels = {0.2, 0.4, 0.2, 1.0},
    FinalLevels = {0.4, 0.1, 0.4, 1.0}
}

local function getThemePath(theme)
    if theme == "Moon" then
        return "moon/"
    elseif theme == "FoundLevels" then
        return "foundlevels/els/"
    elseif theme == "FinalLevels" then
        return "finallevels/els/"
    else
        return ""
    end
end

local function getBlockTexture(theme)
    local themePath = getThemePath(theme)
    local path = "objects/zoommover/" .. themePath .. "block"
    
    -- Return the path, Loenn will handle fallback
    return path
end

local function getCogTexture(theme)
    local themePath = getThemePath(theme)
    local path = "objects/zoommover/" .. themePath .. "cog"
    
    return path
end

function zoomMover.sprite(room, entity)
    local sprites = {}
    
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 16
    local height = entity.height or 16
    local theme = entity.theme or "Normal"
    local nodes = entity.nodes or {{x = x, y = y - 100}}
    
    local targetX = nodes[1].x
    local targetY = nodes[1].y
    
    -- Get theme color for the rope
    local color = themeColors[theme] or themeColors.Normal
    
    -- Draw rope line
    local centerX = x + width / 2
    local centerY = y + height / 2
    local targetCenterX = targetX + width / 2
    local targetCenterY = targetY + height / 2
    
    local line = drawableLine.fromPoints({centerX, centerY, targetCenterX, targetCenterY}, {0.4, 0.2, 0.2, 1.0}, 1)
    table.insert(sprites, line)
    
    -- Draw cog at target position
    local cogTexture = getCogTexture(theme)
    local targetCog = drawableSprite.fromTexture(cogTexture, {x = targetCenterX, y = targetCenterY})
    if targetCog then
        table.insert(sprites, targetCog)
    end
    
    -- Draw the block
    local blockTexture = getBlockTexture(theme)
    local tilesX = math.floor(width / 8)
    local tilesY = math.floor(height / 8)
    
    -- Draw 9-slice block
    for tx = 0, tilesX - 1 do
        for ty = 0, tilesY - 1 do
            local edgeX = 1
            local edgeY = 1
            
            if tx == 0 then edgeX = 0 end
            if tx == tilesX - 1 then edgeX = 2 end
            if ty == 0 then edgeY = 0 end
            if ty == tilesY - 1 then edgeY = 2 end
            
            local sprite = drawableSprite.fromTexture(blockTexture, {
                x = x + tx * 8 + 4,
                y = y + ty * 8 + 4
            })
            
            if sprite then
                sprite:setJustification(0.5, 0.5)
                sprite:useRelativeQuad(edgeX * 8, edgeY * 8, 8, 8)
                table.insert(sprites, sprite)
            end
        end
    end
    
    -- Draw cog at start position
    local startCog = drawableSprite.fromTexture(cogTexture, {x = centerX, y = centerY})
    if startCog then
        table.insert(sprites, startCog)
    end
    
    -- Draw light indicator
    local lightTexture = "objects/zoommover/" .. getThemePath(theme) .. "light00"
    local light = drawableSprite.fromTexture(lightTexture, {
        x = x + width / 2,
        y = y
    })
    if light then
        light:setJustification(0.5, 0)
        table.insert(sprites, light)
    end
    
    return sprites
end

function zoomMover.nodeSprite(room, entity, node)
    local sprites = {}
    local width = entity.width or 16
    local height = entity.height or 16
    local theme = entity.theme or "Normal"
    
    local x = node.x
    local y = node.y
    
    -- Draw a ghost of the block at the target position
    local blockTexture = getBlockTexture(theme)
    local tilesX = math.floor(width / 8)
    local tilesY = math.floor(height / 8)
    
    for tx = 0, tilesX - 1 do
        for ty = 0, tilesY - 1 do
            local edgeX = 1
            local edgeY = 1
            
            if tx == 0 then edgeX = 0 end
            if tx == tilesX - 1 then edgeX = 2 end
            if ty == 0 then edgeY = 0 end
            if ty == tilesY - 1 then edgeY = 2 end
            
            local sprite = drawableSprite.fromTexture(blockTexture, {
                x = x + tx * 8 + 4,
                y = y + ty * 8 + 4
            })
            
            if sprite then
                sprite:setJustification(0.5, 0.5)
                sprite:useRelativeQuad(edgeX * 8, edgeY * 8, 8, 8)
                sprite:setColor({1, 1, 1, 0.4})
                table.insert(sprites, sprite)
            end
        end
    end
    
    return sprites
end

function zoomMover.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 16
    local height = entity.height or 16
    local nodes = entity.nodes or {{x = x, y = y - 100}}
    
    local mainRect = utils.rectangle(x, y, width, height)
    local nodeRects = {}
    
    for i, node in ipairs(nodes) do
        nodeRects[i] = utils.rectangle(node.x, node.y, width, height)
    end
    
    return mainRect, nodeRects
end

return zoomMover
