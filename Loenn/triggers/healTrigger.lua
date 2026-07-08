local healTrigger = {}

healTrigger.name = "DZ/HealTrigger"
healTrigger.depth = 0
healTrigger.placements = {
    name = "heal_trigger",
    data = {
        healAmount = 1,
        fullHeal = false,
        removeAfterUse = true,
        onlyOnce = true
    }
}

return healTrigger
