-- Tenna TV Boss
local tennaTVBoss = {}

tennaTVBoss.name = "MaggyHelper/TennaTVBoss"
tennaTVBoss.depth = 0
tennaTVBoss.texture = "characters/tenna/tv_idle00"
tennaTVBoss.justification = {0.5, 1.0}

tennaTVBoss.placements = {
    {
        name = "tenna_tv_boss",
        data = {
            health = 300,
            maxHealth = 300
        }
    }
}

return tennaTVBoss
