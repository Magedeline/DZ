-- Axis Terminator 2.0 Boss
local axisTerminator2Boss = {}

axisTerminator2Boss.name = "DZ/AxisTerminator2Boss"
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

return {
    axisTerminator2Boss
}
