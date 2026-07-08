local towerFountainTrigger = {}

towerFountainTrigger.name = "DZ/TowerFountainTrigger"
towerFountainTrigger.depth = 0
towerFountainTrigger.placements = {
    name = "tower_fountain_trigger",
    data = {
        activationFlag = "ch12_completion",
        onlyOnce = true,
        requiresChapter12 = true
    }
}

return towerFountainTrigger
