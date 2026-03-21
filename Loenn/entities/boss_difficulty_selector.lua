local boss_difficulty_selector = {}

boss_difficulty_selector.name = "DesoloZantas/BossDifficultySelector"
boss_difficulty_selector.depth = 0
boss_difficulty_selector.placements = {
    {
        name = "Boss Difficulty Selector",
        data = {
            bossId = "",
            defaultDifficulty = "normal",
            showBeforeBoss = true
        }
    }
}

boss_difficulty_selector.fieldInformation = {
    defaultDifficulty = {
        fieldType = "string",
        options = {"easy", "normal", "hard", "nightmare"}
    }
}

local difficultyColors = {
    easy = {0.2, 0.8, 0.2, 0.8},
    normal = {0.8, 0.8, 0.2, 0.8},
    hard = {0.8, 0.4, 0.2, 0.8},
    nightmare = {0.8, 0.0, 0.0, 0.8}
}

function boss_difficulty_selector.sprite(room, entity)
    local sprites = {}
    local drawableRectangle = require("structs.drawable_rectangle")
    
    local color = difficultyColors[entity.defaultDifficulty] or difficultyColors.normal
    
    -- Background
    local bgRect = drawableRectangle.fromRectangle("fill", entity.x, entity.y, 32, 32, {0.1, 0.1, 0.1, 0.9})
    for _, sprite in ipairs(bgRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    -- Difficulty indicator
    local indicatorRect = drawableRectangle.fromRectangle("bordered", entity.x + 4, entity.y + 4, 24, 24, color, {1.0, 1.0, 1.0, 0.9})
    for _, sprite in ipairs(indicatorRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    return sprites
end

boss_difficulty_selector.offset = {16, 16}

return boss_difficulty_selector
