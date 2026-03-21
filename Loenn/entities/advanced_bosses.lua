-- Axis Terminator 2.0 Boss
local axisTerminator2Boss = {}

axisTerminator2Boss.name = "MaggyHelper/AxisTerminator2Boss"
axisTerminator2Boss.depth = 0
axisTerminator2Boss.texture = "characters/axis2/axis2_idle00"
axisTerminator2Boss.justification = {0.5, 1.0}

axisTerminator2Boss.placements = {
    {
        name = "axis_terminator_2_boss",
        data = {
            health = 800,
            maxHealth = 800,
            phase2Active = false
        }
    }
}

-- King Titan Boss
local kingTitanBoss = {}

kingTitanBoss.name = "MaggyHelper/KingTitanBoss"
kingTitanBoss.depth = 0
kingTitanBoss.texture = "characters/kingtitan/titan_idle00"
kingTitanBoss.justification = {0.5, 1.0}

kingTitanBoss.placements = {
    {
        name = "king_titan_boss",
        data = {
            health = 2000,
            maxHealth = 2000,
            currentPhase = 1
        }
    }
}

return {
    axisTerminator2Boss,
    kingTitanBoss
}
