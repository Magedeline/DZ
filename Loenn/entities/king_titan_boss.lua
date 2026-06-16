local kingTitanBoss = {}

kingTitanBoss.name = "DZ/KingTitanBoss"
kingTitanBoss.depth = 0
kingTitanBoss.texture = "characters/kingtitan/titan_idle00"
kingTitanBoss.justification = {0.5, 1.0}

kingTitanBoss.placements = {
    {
        name = "king_titan_boss",
        data = {
            health = 2000,
            maxHealth = 2000,
            currentPhase = 1
        }
    }
}

return kingTitanBoss