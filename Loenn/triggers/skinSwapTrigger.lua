local skinSwapTrigger = {}

skinSwapTrigger.name = "DZ/SkinSwapTrigger"
skinSwapTrigger.depth = 0
skinSwapTrigger.placements = {
    name = "skin_swap_trigger",
    data = {
        skinId = "Default",
        characterId = "",
        revertOnLeave = false,
        playerVariant = true,
        otherselfVariant = true,
        silhouetteVariant = false,
        enableCharacterAbilities = true
    }
}

return skinSwapTrigger