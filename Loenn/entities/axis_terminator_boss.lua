-- Axis Terminator Boss
local axisTerminatorBoss = {}

axisTerminatorBoss.name = "MaggyHelper/AxisTerminatorBoss"
axisTerminatorBoss.depth = 0
axisTerminatorBoss.texture = "characters/axis/terminator_idle00"
axisTerminatorBoss.justification = {0.5, 1.0}

axisTerminatorBoss.placements = {
    {
        name = "axis_terminator_boss",
        data = {
            health = 500,
            maxHealth = 500,
            armorPieces = 3
        }
    }
}

return axisTerminatorBoss
