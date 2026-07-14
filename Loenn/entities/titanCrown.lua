local titanCrown = {}

titanCrown.name = "DZ/TitanCrown"
titanCrown.depth = 0
titanCrown.texture = "objects/DZ/titan_crown"

titanCrown.placements = {
    name = "titan_crown",
    data = {
        bossEntity = "DZ/KingTitanBoss",
        cutsceneId = "DZ_CH15_ROARING_TITAN_KING_BATTLE",
        autoActivate = false,
        activationRadius = 100
    }
}

return titanCrown