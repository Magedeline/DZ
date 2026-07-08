local bossArenaTrigger = {}

bossArenaTrigger.name = "DZ/BossArenaTrigger"
bossArenaTrigger.depth = 0
bossArenaTrigger.placements = {
    name = "boss_arena_trigger",
    data = {
        bossName = "Boss",
        showHealthBar = true,
        createHealthUI = true,
        bossEntityType = "",
        startEncounter = false,
        triggerOnce = false
    }
}

return bossArenaTrigger
