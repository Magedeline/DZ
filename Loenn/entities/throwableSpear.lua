-- Loenn plugin for DZ/ThrowableSpear (chapter 12 - Wateredgefall).
-- Placeholder visual: long thin rectangle shaft with a square tip. Swap the sprite function
-- for a drawableSprite once real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local throwableSpear = {}

throwableSpear.name = "DZ/ThrowableSpear"
throwableSpear.depth = 100

throwableSpear.placements = {
    name = "throwable_spear",
    data = {
        throwSpeed = 300.0,
        stickDuration = 4.0,
        respawnDelay = 2.5,
        damage = 2,
        killsEnemies = true,
        sticksToSolids = true,
        platformWhenStuck = false,
        spearLength = 20.0,
        spearThickness = 4.0
    }
}

throwableSpear.fieldInformation = {
    throwSpeed = {
        minimumValue = 40.0
    },
    stickDuration = {
        minimumValue = 0.1
    },
    respawnDelay = {
        minimumValue = 0.0
    },
    damage = {
        fieldType = "integer",
        minimumValue = 1
    },
    spearLength = {
        minimumValue = 8.0
    },
    spearThickness = {
        minimumValue = 2.0
    }
}

-- drawableRectangle returns a single sprite in "fill" mode and a list of sprites in
-- "line" mode, so flatten whatever comes back before handing it to Loenn.
local function push(sprites, rectangle)
    local result = rectangle:getDrawableSprite()

    if result[1] ~= nil then
        for _, sprite in ipairs(result) do
            table.insert(sprites, sprite)
        end
    else
        table.insert(sprites, result)
    end
end

local function pushRect(sprites, x, y, width, height, fill, line)
    push(sprites, drawableRectangle.fromRectangle("fill", x, y, width, height, fill))
    push(sprites, drawableRectangle.fromRectangle("line", x, y, width, height, line))
end

local fillColor = {0.18, 0.56, 0.84, 0.95}
local lineColor = {0.85, 0.95, 1.0, 1.0}
local tipColor = {0.92, 0.96, 1.0, 1.0}

-- The C# hitbox is 8x8 with its origin at the spear's feet; draw the shaft over that.
local function spearRect(entity)
    local length = entity.spearLength or 20
    local thickness = entity.spearThickness or 4
    local x, y = entity.x or 0, entity.y or 0

    return x - length / 2, y - 4 - thickness / 2, length, thickness
end

function throwableSpear.sprite(room, entity)
    local x, y, length, thickness = spearRect(entity)
    local sprites = {}

    pushRect(sprites, x, y, length, thickness, fillColor, lineColor)
    pushRect(sprites, x + length - thickness, y - thickness / 2, thickness * 1.5, thickness * 2,
        tipColor, lineColor)

    return sprites
end

function throwableSpear.selection(room, entity)
    local x, y, length, thickness = spearRect(entity)

    return utils.rectangle(x, y - thickness, length + thickness * 1.5, thickness * 3)
end

return throwableSpear
