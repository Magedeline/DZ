local bossTiers = {}

-- Tier 1 Boss
local bossTier1 = {}
bossTier1.name = "DZ/BossTier1"
bossTier1.depth = 0
bossTier1.placements = {
    {
        name = "main",
        data = {
            bossType = "BasicEnemy",
            tier = 1,
            gimmick = 0,
            health = 100,
            speed = 20.0,
            arenaRadius = 150.0
        }
    }
}

-- Tier 2 Boss
local bossTier2 = {}
bossTier2.name = "DZ/BossTier2"
bossTier2.depth = 0
bossTier2.placements = {
    {
        name = "main",
        data = {
            bossType = "ElementalGuardian",
            tier = 2,
            gimmick = 1,
            health = 200,
            speed = 30.0,
            arenaRadius = 175.0
        }
    }
}

-- Tier 3 Boss
local bossTier3 = {}
bossTier3.name = "DZ/BossTier3"
bossTier3.depth = 0
bossTier3.placements = {
    {
        name = "main",
        data = {
            bossType = "ShadowWarrior",
            tier = 3,
            gimmick = 2,
            health = 350,
            speed = 45.0,
            arenaRadius = 200.0
        }
    }
}

-- Tier 4 Boss
local bossTier4 = {}
bossTier4.name = "DZ/BossTier4"
bossTier4.depth = 0
bossTier4.placements = {
    {
        name = "main",
        data = {
            bossType = "CrystalLord",
            tier = 4,
            gimmick = 4,
            health = 500,
            speed = 60.0,
            arenaRadius = 225.0
        }
    }
}

-- Tier 5 Boss
local bossTier5 = {}
bossTier5.name = "DZ/BossTier5"
bossTier5.depth = 0
bossTier5.placements = {
    {
        name = "main",
        data = {
            bossType = "VoidKnight",
            tier = 5,
            gimmick = 5,
            health = 750,
            speed = 80.0,
            arenaRadius = 250.0
        }
    }
}

-- Tier 6 Boss (Final Boss)
local bossTier6 = {}
bossTier6.name = "DZ/BossTier6"
bossTier6.depth = 0
bossTier6.placements = {
    {
        name = "main",
        data = {
            bossType = "BlackholeAngel",
            tier = 6,
            gimmick = 6,
            health = 1000,
            speed = 100.0,
            arenaRadius = 300.0
        }
    }
}

-- Common field information for all tiers
local commonFieldInfo = {
    tier = {
        options = {
            ["Lowest"] = 1,
            ["Low"] = 2,
            ["Mid"] = 3,
            ["High"] = 4,
            ["Highest"] = 5,
            ["Final"] = 6
        }
    },
    gimmick = {
        options = {
            ["None"] = 0,
            ["Teleport"] = 1,
            ["Time Freeze"] = 2,
            ["Shield Breaker"] = 3,
            ["Elemental Fusion"] = 4,
            ["Gravity Control"] = 5,
            ["Dimension Rift"] = 6
        }
    }
}

bossTier1.fieldInformation = commonFieldInfo
bossTier2.fieldInformation = commonFieldInfo
bossTier3.fieldInformation = commonFieldInfo
bossTier4.fieldInformation = commonFieldInfo
bossTier5.fieldInformation = commonFieldInfo
bossTier6.fieldInformation = commonFieldInfo

bossTier1.texture = "objects/boss/tier1"
bossTier2.texture = "objects/boss/tier2"
bossTier3.texture = "objects/boss/tier3"
bossTier4.texture = "objects/boss/tier4"
bossTier5.texture = "objects/boss/tier5"
bossTier6.texture = "objects/boss/tier6"

return {
    bossTier1,
    bossTier2,
    bossTier3,
    bossTier4,
    bossTier5,
    bossTier6
}