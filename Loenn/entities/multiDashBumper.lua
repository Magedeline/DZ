-- Loenn plugin for DZ/MultiDashBumper.
-- One entity, six placements: 1-5 dash bumpers each get their own colour, and the
-- rainbow bumper (10 dashes) draws its pips as a spectrum. Placeholder visual: circle
-- with one pip per dash. Swap the sprite function for a drawableSprite once art exists.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local multiDashBumper = {}

multiDashBumper.name = "DZ/MultiDashBumper"
multiDashBumper.depth = -100
multiDashBumper.nodeLimits = {0, 1}
multiDashBumper.nodeLineRenderType = "line"

-- Every placement shares these; only the dash count differs.
local function bumperData(dashes)
    return {
        dashes = dashes,
        radius = 12.0,
        respawnTime = 0.6,
        launchMultiplier = 1.0,
        setExactly = false,
        refillStamina = true,
        oneUse = false,
        moveTime = 1.81,
        sineWave = true,
        flag = "",
        invertFlag = false
    }
end

multiDashBumper.placements = {
    {
        name = "one_dash",
        data = bumperData(1)
    },
    {
        name = "two_dash",
        data = bumperData(2)
    },
    {
        name = "three_dash",
        data = bumperData(3)
    },
    {
        name = "four_dash",
        data = bumperData(4)
    },
    {
        name = "five_dash",
        data = bumperData(5)
    },
    {
        name = "rainbow_ten_dash",
        data = bumperData(10)
    }
}

multiDashBumper.fieldInformation = {
    dashes = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 20
    },
    radius = {
        minimumValue = 4.0
    },
    respawnTime = {
        minimumValue = 0.05
    },
    launchMultiplier = {
        minimumValue = 0.1
    },
    moveTime = {
        minimumValue = 0.1
    }
}

-- Dash counts at or above this render as the rainbow bumper; matches
-- MultiDashBumper.RainbowThreshold on the C# side.
local rainbowThreshold = 6

-- Kept in step with the DashColors table in MultiDashBumper.cs.
local dashColors = {
    {1.000, 0.200, 0.333, 1.0},
    {1.000, 0.467, 0.867, 1.0},
    {0.490, 0.373, 1.000, 1.0},
    {0.184, 0.847, 1.000, 1.0},
    {1.000, 0.835, 0.239, 1.0}
}

local ringColor = {1.0, 1.0, 1.0, 1.0}

-- Minimal HSV->RGB so the rainbow bumper reads correctly in the editor too.
local function fromHsv(hue, saturation, value)
    hue = hue % 360
    local sector = hue / 60
    local index = math.floor(sector)
    local fraction = sector - index
    local p = value * (1 - saturation)
    local q = value * (1 - saturation * fraction)
    local t = value * (1 - saturation * (1 - fraction))

    if index == 0 then return {value, t, p, 1.0}
    elseif index == 1 then return {q, value, p, 1.0}
    elseif index == 2 then return {p, value, t, 1.0}
    elseif index == 3 then return {p, q, value, 1.0}
    elseif index == 4 then return {t, p, value, 1.0}
    else return {value, p, q, 1.0} end
end

local function dashesOf(entity)
    return math.max(1, math.min(entity.dashes or 1, 20))
end

local function colorOf(entity)
    local dashes = dashesOf(entity)

    if dashes >= rainbowThreshold then
        -- Static mid-spectrum sample; the entity animates through the full cycle in game.
        return fromHsv(300, 0.85, 1.0)
    end

    return dashColors[dashes] or dashColors[1]
end

local function drawBumper(x, y, radius, dashes, fill, rainbow)
    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(fill[1], fill[2], fill[3], 0.3)
        love.graphics.circle("fill", x, y, radius + 3)

        love.graphics.setColor(fill[1], fill[2], fill[3], 0.85)
        love.graphics.circle("fill", x, y, radius)
        love.graphics.setColor(ringColor)
        love.graphics.circle("line", x, y, radius)

        -- One pip per dash, in a ring, matching the in-game render.
        if dashes == 1 then
            love.graphics.rectangle("fill", x - 2, y - 2, 4, 4)
            return
        end

        local pipRadius = radius * 0.5
        local pipSize = dashes > 8 and 2.5 or 3.5

        for i = 0, dashes - 1 do
            local angle = -math.pi / 2 + i * (math.pi * 2 / dashes)
            local px = x + math.cos(angle) * pipRadius
            local py = y + math.sin(angle) * pipRadius

            if rainbow then
                love.graphics.setColor(fromHsv(i / dashes * 360, 0.85, 1.0))
            else
                love.graphics.setColor(ringColor)
            end

            love.graphics.rectangle("fill", px - pipSize / 2, py - pipSize / 2, pipSize, pipSize)
        end
    end)
end

function multiDashBumper.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local radius = entity.radius or 12
    local dashes = dashesOf(entity)
    local rainbow = dashes >= rainbowThreshold
    local fill = colorOf(entity)

    local sprites = {
        drawableFunction.fromFunction(drawBumper, x, y, radius, dashes, fill, rainbow)
    }

    -- A node makes the bumper travel; show a ghost at the far end.
    for _, node in ipairs(entity.nodes or {}) do
        table.insert(sprites, drawableFunction.fromFunction(drawBumper, node.x, node.y, radius, dashes, fill, rainbow))
    end

    return sprites
end

function multiDashBumper.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local radius = entity.radius or 12
    local nodeRects = {}

    for _, node in ipairs(entity.nodes or {}) do
        table.insert(nodeRects, utils.rectangle(node.x - radius, node.y - radius, radius * 2, radius * 2))
    end

    return utils.rectangle(x - radius, y - radius, radius * 2, radius * 2), nodeRects
end

return multiDashBumper
