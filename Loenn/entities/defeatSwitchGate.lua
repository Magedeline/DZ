local defeatSwitchGate = {}

defeatSwitchGate.name = "DZ/DefeatSwitchGate"
defeatSwitchGate.depth = 0
defeatSwitchGate.texture = "objects/DZ/defeat_switch_gate"

defeatSwitchGate.placements = {
    name = "defeat_switch_gate",
    data = {
        flag = "",
        useGlobalCounts = false,
        persistent = false,
        requiredEnemyDefeats = 0,
        requiredBossDefeats = 0
    }
}

return defeatSwitchGate