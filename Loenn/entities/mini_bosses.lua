-- Meta Knight Terminator Boss
local metaKnightTerminatorBoss = {}

metaKnightTerminatorBoss.name = "DZ/MetaKnightTerminatorBoss"
metaKnightTerminatorBoss.depth = 0
metaKnightTerminatorBoss.texture = "characters/metaknight/mk_idle00"
metaKnightTerminatorBoss.justification = {0.5, 1.0}

metaKnightTerminatorBoss.placements = {
    {
        name = "main",
        data = {
            health = 400,
            maxHealth = 400
        }
    }
}

-- Digital King DDD Boss
local digitalKingDDDBoss = {}

digitalKingDDDBoss.name = "DZ/DigitalKingDDDBoss"
digitalKingDDDBoss.depth = 0
digitalKingDDDBoss.texture = "characters/digitalddd/ddd_idle00"
digitalKingDDDBoss.justification = {0.5, 1.0}

digitalKingDDDBoss.placements = {
    {
        name = "main",
        data = {
            health = 600,
            maxHealth = 600
        }
    }
}

-- Martlet Bird Possess Boss
local martletBirdPossessBoss = {}

martletBirdPossessBoss.name = "DZ/MartletBirdPossessBoss"
martletBirdPossessBoss.depth = 0
martletBirdPossessBoss.texture = "characters/martlet/martlet_fly00"
martletBirdPossessBoss.justification = {0.5, 1.0}

martletBirdPossessBoss.placements = {
    {
        name = "main",
        data = {
            health = 350
        }
    }
}

-- Black/Dark Matter Boss
local blackDarkMatterBoss = {}

blackDarkMatterBoss.name = "DZ/BlackDarkMatterBoss"
blackDarkMatterBoss.depth = 0
blackDarkMatterBoss.texture = "characters/darkmatter/dm_idle00"
blackDarkMatterBoss.justification = {0.5, 0.5}

blackDarkMatterBoss.placements = {
    {
        name = "main",
        data = {
            health = 450
        }
    }
}

-- Dark Matter with Knife Boss
local darkMatterKnifeBoss = {}

darkMatterKnifeBoss.name = "DZ/DarkMatterKnifeBoss"
darkMatterKnifeBoss.depth = 0
darkMatterKnifeBoss.texture = "characters/darkmatter/dmk_idle00"
darkMatterKnifeBoss.justification = {0.5, 0.5}

darkMatterKnifeBoss.placements = {
    {
        name = "main",
        data = {
            health = 550
        }
    }
}

return {
    metaKnightTerminatorBoss,
    digitalKingDDDBoss,
    martletBirdPossessBoss,
    blackDarkMatterBoss,
    darkMatterKnifeBoss
}
