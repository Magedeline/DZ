local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local maggyJumpThru = {}

maggyJumpThru.name = "DZ/MaggyJumpThru"
maggyJumpThru.depth = -60
maggyJumpThru.canResize = {true, false}
maggyJumpThru.minimumSize = {8, 8}

-- All available texture options from the jumpthru folder
local textureOptions = {
    "wood",
    "cliffside",
    "core",
    "dream",
    "error",
    "fatal",
    "heart",
    "moon",
    "reflection",
    "temple",
    "templeB"
}

maggyJumpThru.fieldInformation = {
    texture = {
        fieldType = "string",
        options = textureOptions,
        editable = true
    },
    surfaceIndex = {
        fieldType = "integer",
        minimumValue = -1,
        maximumValue = 40,
        options = {
            ["Default (-1)"] = -1,
            ["Grass (0)"] = 0,
            ["Snow (1)"] = 1,
            ["Dirt (2)"] = 2,
            ["Stone (3)"] = 3,
            ["Wood (4)"] = 4,
            ["Ice (5)"] = 5,
            ["Metal (6)"] = 6,
            ["Dream Block (7)"] = 7,
            ["Cloud (9)"] = 9,
            ["Core Ice (35)"] = 35,
            ["Core Fire (36)"] = 36
        },
        editable = true
    },
    animationSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    sinkAmount = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 16.0
    },
    sinkSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 500.0
    },
    moveSpeed = {
        fieldType = "number",
        minimumValue = -500.0,
        maximumValue = 500.0
    },
    respawnTime = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 30.0
    },
    fallDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    shakeTime = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0
    },
    fallSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1000.0
    },
    tint = {
        fieldType = "color"
    }
}

maggyJumpThru.fieldOrder = {
    "x", "y", "width",
    "texture", "surfaceIndex", "tint",
    "pushKglobal::Player", "attachToSolids", "letSeekersThrough", "oneWay",
    "animated", "animationSpeed",
    "sinks", "sinkAmount", "sinkSpeed",
    "moves", "moveSpeed",
    "falls", "fallDelay", "shakeTime", "fallSpeed", "respawns", "respawnTime"
}

maggyJumpThru.placements = {
    {
        name = "wood",
        data = {
            width = 24,
            texture = "wood",
            surfaceIndex = 4,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = false,
            animationSpeed = 1.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    },
    {
        name = "cliffside",
        data = {
            width = 24,
            texture = "cliffside",
            surfaceIndex = 3,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = false,
            animationSpeed = 1.0,
            sinks = false,
            sinkAmount = 2.0,
            sinkSpeed = 80.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    },
    {
        name = "core",
        data = {
            width = 24,
            texture = "core",
            surfaceIndex = 35,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = true,
            animationSpeed = 2.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    },
    {
        name = "dream",
        data = {
            width = 32,
            texture = "dream",
            surfaceIndex = 7,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = true,
            oneWay = true,
            animated = true,
            animationSpeed = 1.5,
            sinks = true,
            sinkAmount = 4.0,
            sinkSpeed = 60.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    },
    {
        name = "error",
        data = {
            width = 24,
            texture = "error",
            surfaceIndex = -1,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = true,
            animationSpeed = 3.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ff0000"
        }
    },
    {
        name = "fatal",
        data = {
            width = 24,
            texture = "fatal",
            surfaceIndex = -1,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = true,
            animationSpeed = 2.5,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "880000"
        }
    },
    {
        name = "heart",
        data = {
            width = 24,
            texture = "heart",
            surfaceIndex = -1,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = true,
            animationSpeed = 2.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ff88cc"
        }
    },
    {
        name = "moon",
        data = {
            width = 24,
            texture = "moon",
            surfaceIndex = -1,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = true,
            animationSpeed = 1.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "aaccff"
        }
    },
    {
        name = "reflection",
        data = {
            width = 24,
            texture = "reflection",
            surfaceIndex = 6,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = false,
            animationSpeed = 1.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    },
    {
        name = "temple",
        data = {
            width = 24,
            texture = "temple",
            surfaceIndex = 3,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = false,
            animationSpeed = 1.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    },
    {
        name = "templeB",
        data = {
            width = 24,
            texture = "templeB",
            surfaceIndex = 3,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = false,
            animationSpeed = 1.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    },
    {
        name = "falling",
        data = {
            width = 32,
            texture = "wood",
            surfaceIndex = 4,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = false,
            animationSpeed = 1.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = false,
            moveSpeed = 0.0,
            falls = true,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 200.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    },
    {
        name = "sinking",
        data = {
            width = 32,
            texture = "dream",
            surfaceIndex = 7,
            pushKglobal::global::Celeste.Player= false,
            attachToSolids = false,
            letSeekersThrough = false,
            oneWay = true,
            animated = true,
            animationSpeed = 1.5,
            sinks = true,
            sinkAmount = 6.0,
            sinkSpeed = 80.0,
            moves = false,
            moveSpeed = 0.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    },
    {
        name = "moving",
        data = {
            width = 32,
            texture = "reflection",
            surfaceIndex = 6,
            pushKglobal::global::Celeste.Player= true,
            attachToSolids = true,
            letSeekersThrough = false,
            oneWay = true,
            animated = false,
            animationSpeed = 1.0,
            sinks = false,
            sinkAmount = 3.0,
            sinkSpeed = 100.0,
            moves = true,
            moveSpeed = 60.0,
            falls = false,
            fallDelay = 0.3,
            shakeTime = 0.5,
            fallSpeed = 160.0,
            respawns = true,
            respawnTime = 2.0,
            tint = "ffffff"
        }
    }
}

local texturePath = "objects/jumpthru/"

function maggyJumpThru.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 24
    local texture = entity.texture or "wood"
    local tint = entity.tint or "ffffff"
    
    -- Parse the tint color
    local r, g, b = 1, 1, 1
    if tint and #tint >= 6 then
        r = tonumber(tint:sub(1, 2), 16) / 255
        g = tonumber(tint:sub(3, 4), 16) / 255
        b = tonumber(tint:sub(5, 6), 16) / 255
    end
    
    local fullPath = texturePath .. texture
    
    -- Calculate number of 8-pixel segments
    local tileCount = math.floor(width / 8)
    
    for i = 0, tileCount - 1 do
        local quadX = 8  -- Middle piece by default
        
        if i == 0 then
            quadX = 0  -- Left edge
        elseif i == tileCount - 1 then
            quadX = 16  -- Right edge
        end
        
        local sprite = drawableSprite.fromTexture(fullPath, entity)
        sprite:setJustification(0, 0)
        sprite:useRelativeQuad(quadX, 0, 8, 8)
        sprite:setPosition(x + i * 8, y)
        sprite:setColor({r, g, b, 1})
        
        table.insert(sprites, sprite)
    end
    
    return sprites
end

function maggyJumpThru.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 24, 8)
end

return maggyJumpThru
