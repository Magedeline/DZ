local areaCompleteTrigger = {}

areaCompleteTrigger.name = "DZ/AreaCompleteTrigger"
areaCompleteTrigger.depth = 0
areaCompleteTrigger.placements = {
    name = "area_complete_trigger",
    data = {
        nextLevel = "",
        hasGoldenStrawberry = false,
        hasPinkPlatinumBerry = false,
        skipCredits = false,
        triggerOnce = true
    }
}

return areaCompleteTrigger
