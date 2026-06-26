local titanThrone = {}

titanThrone.name = "DZ/TitanThrone"
titanThrone.depth = 0
titanThrone.texture = "objects/DZ/titan_throne"

titanThrone.placements = {
    name = "titan_throne",
    data = {
        bossEntity = "DZ/KingTitanBoss",
        cutsceneId = "DZ_CH15_ROARING_TITAN_KING_BATTLE",
        autoActivate = false,
        activationRadius = 100
    }
}

return titanThrone