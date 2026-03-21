-- Meta Knight Terminator Boss
local metaKnightTerminatorBoss = {}

metaKnightTerminatorBoss.name = "MaggyHelper/MetaKnightTerminatorBoss"
metaKnightTerminatorBoss.depth = 0
metaKnightTerminatorBoss.texture = "characters/metaknight/mk_idle00"
metaKnightTerminatorBoss.justification = {0.5, 1.0}

metaKnightTerminatorBoss.placements = {
    {
        name = "meta_knight_terminator_boss",
        data = {
            health = 400,
            maxHealth = 400
        }
    }
}

-- Digital King DDD Boss
local digitalKingDDDBoss = {}

digitalKingDDDBoss.name = "MaggyHelper/DigitalKingDDDBoss"
digitalKingDDDBoss.depth = 0
digitalKingDDDBoss.texture = "characters/digitalddd/ddd_idle00"
digitalKingDDDBoss.justification = {0.5, 1.0}

digitalKingDDDBoss.placements = {
    {
        name = "digital_king_ddd_boss",
        data = {
            health = 600,
            maxHealth = 600
        }
    }
}

-- Martlet Bird Possess Boss
local martletBirdPossessBoss = {}

martletBirdPossessBoss.name = "MaggyHelper/MartletBirdPossessBoss"
martletBirdPossessBoss.depth = 0
martletBirdPossessBoss.texture = "characters/martlet/martlet_fly00"
martletBirdPossessBoss.justification = {0.5, 1.0}

martletBirdPossessBoss.placements = {
    {
        name = "martlet_bird_possess_boss",
        data = {
            health = 350
        }
    }
}

-- Black/Dark Matter Boss
local blackDarkMatterBoss = {}

blackDarkMatterBoss.name = "MaggyHelper/BlackDarkMatterBoss"
blackDarkMatterBoss.depth = 0
blackDarkMatterBoss.texture = "characters/darkmatter/dm_idle00"
blackDarkMatterBoss.justification = {0.5, 0.5}

blackDarkMatterBoss.placements = {
    {
        name = "black_dark_matter_boss",
        data = {
            health = 450
        }
    }
}

-- Dark Matter with Knife Boss
local darkMatterKnifeBoss = {}

darkMatterKnifeBoss.name = "MaggyHelper/DarkMatterKnifeBoss"
darkMatterKnifeBoss.depth = 0
darkMatterKnifeBoss.texture = "characters/darkmatter/dmk_idle00"
darkMatterKnifeBoss.justification = {0.5, 0.5}

darkMatterKnifeBoss.placements = {
    {
        name = "dark_matter_knife_boss",
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
