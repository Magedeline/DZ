local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")

local defeatSwitchGate = {}

defeatSwitchGate.name = "DZ/DefeatSwitchGate"
defeatSwitchGate.depth = -9000
defeatSwitchGate.minimumSize = {16, 16}
defeatSwitchGate.canResize = {true, true}

defeatSwitchGate.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 48,
            requiredEnemyDefeats = 5,
            requiredBossDefeats = 0,
            useGlobalCounts = false,
            flag = "",
            persistent = false
        }
    },
    {
        name = "boss_gate",
        data = {
            width = 16,
            height = 48,
            requiredEnemyDefeats = 0,
            requiredBossDefeats = 1,
            useGlobalCounts = false,
            flag = "",
            persistent = false
        }
    },
    {
        name = "combined_gate",
        data = {
            width = 16,
            height = 48,
            requiredEnemyDefeats = 3,
            requiredBossDefeats = 1,
            useGlobalCounts = false,
            flag = "",
            persistent = false
        }
    }
}

defeatSwitchGate.fieldInformation = {
    requiredEnemyDefeats = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 999
    },
    requiredBossDefeats = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 99
    },
    useGlobalCounts = {
        fieldType = "boolean"
    },
    flag = {
        fieldType = "string"
    },
    persistent = {
        fieldType = "boolean"
    }
}

defeatSwitchGate.fieldOrder = {
    "x", "y", "width", "height",
    "requiredEnemyDefeats", "requiredBossDefeats",
    "useGlobalCounts", "flag", "persistent"
}

-- Colour helper based on which requirement is active
local function getGateColor(entity)
    local needsEnemies = (entity.requiredEnemyDefeats or 0) > 0
    local needsBosses  = (entity.requiredBossDefeats or 0) > 0

    if needsBosses and needsEnemies then
        return {1.0, 0.4, 0.0, 0.8}   -- orange
    elseif needsBosses then
        return {0.8, 0.0, 0.0, 0.8}   -- red
    else
        return {0.2, 0.4, 1.0, 0.8}   -- blue
    end
end

function defeatSwitchGate.sprite(room, entity)
    local sprites = {}
    local width  = entity.width  or 16
    local height = entity.height or 48
    local color  = getGateColor(entity)

    -- Filled rectangle for the gate body
    local rect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, width, height, color)
    table.insert(sprites, rect)

    -- Border
    local border = drawableRectangle.fromRectangle("line", entity.x, entity.y, width, height, {1, 1, 1, 0.6})
    table.insert(sprites, border)

    -- Label text (enemies / bosses counts)
    -- Loenn doesn't have a native drawableText, so we skip in-editor text.

    return sprites
end

function defeatSwitchGate.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 48)
end

return defeatSwitchGate
