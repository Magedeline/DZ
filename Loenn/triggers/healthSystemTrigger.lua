local healthSystemTrigger = {}

healthSystemTrigger.name = "DZ/HealthSystemTrigger"
healthSystemTrigger.depth = 0
healthSystemTrigger.placements = {
    name = "health_system_trigger",
    data = {
        kirbyMode = false,
        showUI = true,
        persistent = true,
        trackBosses = true,
        healOnEnter = false,
        maxHP = 6,
        displayMode = 0,
        healAmount = 0
    }
}

return healthSystemTrigger