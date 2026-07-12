local NightmareSequenceBoss = {}

NightmareSequenceBoss.name = "DZ/NightmareSequenceBoss"
NightmareSequenceBoss.depth = -12500
NightmareSequenceBoss.color = { 0.53, 0.0, 1.0, 1.0 }

NightmareSequenceBoss.placements = {
    {
        name = "normal",
        data = {
            hardMode        = false,
            fromCutscene    = false,
            completionFlag  = "nightmare_sequence_boss_defeated",
        }
    },
    {
        name = "hard",
        data = {
            hardMode        = true,
            fromCutscene    = false,
            completionFlag  = "nightmare_sequence_boss_defeated_hard",
        }
    }
}

NightmareSequenceBoss.attributes = {
    {
        name        = "hardMode",
        type        = "boolean",
        default     = false,
        description = "Multiplies each phase's HP by 1.5×."
    },
    {
        name        = "fromCutscene",
        type        = "boolean",
        default     = false,
        description = "If true the boss starts visible and active immediately (no StartBattle() call needed)."
    },
    {
        name        = "completionFlag",
        type        = "string",
        default     = "nightmare_sequence_boss_defeated",
        description = "Session flag set when all 7 phases are defeated."
    },
}

-- Draw a 40×40 hollow circle to represent the boss orb
function NightmareSequenceBoss.draw(room, entity)
    local x, y = entity.x, entity.y
    local r = 20
    -- Body
    love.graphics.setColor(0.53, 0.0, 1.0, 0.55)
    love.graphics.circle("fill", x, y, r)
    -- Outline
    love.graphics.setColor(0.53, 0.0, 1.0, 1.0)
    love.graphics.setLineWidth(2)
    love.graphics.circle("line", x, y, r)
    love.graphics.setLineWidth(1)
    -- Label
    love.graphics.setColor(1, 1, 1, 1)
    love.graphics.print("NightmareSeq", x - 36, y + r + 2)
end

return NightmareSequenceBoss
