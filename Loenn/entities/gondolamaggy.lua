local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local gondolamaggy = {}

gondolamaggy.name = "DZs/GondolaMaggy"
gondolamaggy.depth = -10500
gondolamaggy.nodeVisibility = "always"
gondolamaggy.nodeLimits = {1, 1}
gondolamaggy.placements = {
    {
        name = "main",
        placementType = "line",
        data = {
            active = true
        }
    }
}

local frontTexture = "objects/gondola/front"
local backTexture = "objects/gondola/back"
local topTexture = "objects/gondola/top"
local leverTexture = "objects/gondola/lever01"
local leftTexture = "objects/gondola/cliffsideLeft"
local rightTexture = "objects/gondola/cliffsideRight"

local renderOffsetY = -64

local wireColor = {0, 0, 0, 1}
local wireThickness = 1

local function getGondolaMaggyPosition(room, entity)
    local active = entity.active
    local x, y = entity.x or 0, entity.y or 0

    if active == false then
        -- Gondola starts at end position if not active
        local nodes = entity.nodes or {{x = 0, y = 0}}
        local nodeX, nodeY = nodes[1].x, nodes[1].y

        return {x = nodeX, y = nodeY}

    else
        return {x = x, y = y}
    end
end

local function addGondolaMaggyMainSprites(room, entity, sprites)
    local active = entity.active
    local gondolamaggyPosition = getGondolaMaggyPosition(room, entity)

    local frontSprite = drawableSprite.fromTexture(frontTexture, gondolamaggyPosition)
    frontSprite:addPosition(0, renderOffsetY)
    frontSprite:setJustification(0.5, 0.0)

    -- Don't rotate, looks weird
    local topSprite = drawableSprite.fromTexture(topTexture, gondolamaggyPosition)
    topSprite:addPosition(0, renderOffsetY)
    topSprite:setJustification(0.5, 0.0)

    local leverSprite = drawableSprite.fromTexture(leverTexture, gondolamaggyPosition)
    leverSprite:addPosition(0, renderOffsetY)
    leverSprite:setJustification(0.5, 0.0)

    local backSprite = drawableSprite.fromTexture(backTexture, gondolamaggyPosition)
    backSprite:addPosition(0, renderOffsetY)
    backSprite:setJustification(0.5, 0.0)
    backSprite.depth = 9000

    table.insert(sprites, frontSprite)
    table.insert(sprites, topSprite)

    if active ~= false then
        table.insert(sprites, leverSprite)
    end

    table.insert(sprites, backSprite)
end

local function getLeftSprite(room, entity)
    local leftSprite = drawableSprite.fromTexture(leftTexture, entity)
    leftSprite:addPosition(-124, 0)
    leftSprite:setJustification(0.0, 1.0)
    leftSprite.depth = 8998

    return leftSprite
end

local function getRightSprite(room, entity)
    local nodes = entity.nodes or {{x = 0, y = 0}}
    local node = nodes[1]
    local rightSprite = drawableSprite.fromTexture(rightTexture, node)
    rightSprite:addPosition(144, -104)
    rightSprite:setJustification(0.0, 0.5)
    rightSprite:setScale(-1, 1)
    rightSprite.depth = 8998

    return rightSprite
end

function gondolamaggy.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y
    local active = entity.active
    local gondolamaggyPosition = getGondolaMaggyPosition(room, entity)

    local leftSprite = getLeftSprite(room, entity)

    -- Only used to calculate wire position
    local rightSprite = getRightSprite(room, entity)

    -- Only used for wire position
    local topSprite = drawableSprite.fromTexture(topTexture, gondolamaggyPosition)
    topSprite:addPosition(0, renderOffsetY)
    topSprite:setJustification(0.5, 0.0)

    local wireLeftX = leftSprite.x + 40
    local wireLeftY = leftSprite.y - 12
    local wireRightX = rightSprite.x - 40
    local wireRightY = rightSprite.y - 4

    local topX = gondolamaggyPosition.x
    local topY = gondolamaggyPosition.y + renderOffsetY + topSprite.meta.height

    local leftWire = drawableLine.fromPoints({wireLeftX, wireLeftY, topX, topY}, wireColor, wireThickness)
    local rightWire = drawableLine.fromPoints({wireRightX, wireRightY, topX, topY}, wireColor, wireThickness)

    leftWire.depth = 8999
    rightWire.depth = 8999

    table.insert(sprites, leftWire:getDrawableSprite()[1])
    table.insert(sprites, rightWire:getDrawableSprite()[1])

    if active ~= false then
        addGondolaMaggyMainSprites(room, entity, sprites)
    end

    table.insert(sprites, leftSprite)

    return sprites
end

-- Define custom main entity rectangle otherwise the cable etc. is automatically considered part of it
function gondolamaggy.selection(room, entity)
    local active = entity.active
    local gondolamaggyPosition = getGondolaMaggyPosition(room, entity)

    local frontSprite = drawableSprite.fromTexture(frontTexture, gondolamaggyPosition)
    frontSprite:addPosition(0, renderOffsetY)
    frontSprite:setJustification(0.5, 0.0)

    local topSprite = drawableSprite.fromTexture(topTexture, gondolamaggyPosition)
    topSprite:addPosition(0, renderOffsetY)
    topSprite:setJustification(0.5, 0.0)

    local gondolamaggySpriteRectangles = {frontSprite:getRectangle(), topSprite:getRectangle()}
    local gondolamaggyRectangle = utils.rectangle(utils.coverRectangles(gondolamaggySpriteRectangles))

    if active ~= false then
        local rightSprite = getRightSprite(room, entity)

        return gondolamaggyRectangle, {rightSprite:getRectangle()}

    else
        local leftSprite = getLeftSprite(room, entity)

        return leftSprite:getRectangle(), {gondolamaggyRectangle}
    end
end

function gondolamaggy.nodeSprite(room, entity, node)
    local rightSprite = getRightSprite(room, entity)
    local active = entity.active

    if active == false then
        local sprites = {rightSprite}

        addGondolaMaggyMainSprites(room, entity, sprites)

        return sprites
    end

    return rightSprite
end

return gondolamaggy
