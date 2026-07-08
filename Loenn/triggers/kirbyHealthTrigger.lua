local kirbyHealthTrigger = {}

kirbyHealthTrigger.name = "DZ/KirbyHealthTrigger"
kirbyHealthTrigger.depth = 0
kirbyHealthTrigger.placements = {
    name = "kirby_health_trigger",
    data = {
        enableHealth = true,
        fullHeal = false,
        setRespawnPoint = false,
        onlyOnce = true,
        maxHealth = 6,
        healAmount = 0
    }
}

return kirbyHealthTrigger