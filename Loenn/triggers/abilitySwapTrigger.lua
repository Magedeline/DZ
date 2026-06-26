local abilitySwapTrigger = {}

abilitySwapTrigger.name = "DZ/AbilitySwapTrigger"
abilitySwapTrigger.depth = 0
abilitySwapTrigger.placements = {
    name = "ability_swap_trigger",
    data = {
        ability = "Fire",
        action = "give",
        onlyOnce = true
    }
}

return abilitySwapTrigger