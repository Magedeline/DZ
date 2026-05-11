local kirbyMidBoss = {}

kirbyMidBoss.name = "MaggyHelper/KirbyMidBoss"
kirbyMidBoss.depth = -200

kirbyMidBoss.fieldInformation = {
    bossType = {
        options = {
            ["Whispy Woods"] = 0,
            ["Kracko"] = 1,
            ["Mr. Frosty"] = 2,
            ["Bonkers"] = 3,
            ["Bugzzy"] = 4,
            ["Fire Lion"] = 5,
            ["Iron Mam"] = 6,
            ["Grand Wheely"] = 7,
            ["Box Boxer"] = 8,
            ["Master Hand"] = 9,
            ["Custom"] = 10
        },
        editable = false
    }
}

kirbyMidBoss.placements = {
    {
        name = "Whispy Woods",
        data = {
            bossType = 0,
            maxHealth = 100,
            arenaWidth = 320,
            arenaHeight = 180,
            powerType = 0,
            canBeInhaled = false
        }
    },
    {
        name = "Kracko",
        data = {
            bossType = 1,
            maxHealth = 80,
            arenaWidth = 400,
            arenaHeight = 200,
            powerType = 3,
            canBeInhaled = false
        }
    },
    {
        name = "Mr. Frosty",
        data = {
            bossType = 2,
            maxHealth = 60,
            arenaWidth = 256,
            arenaHeight = 160,
            powerType = 2,
            canBeInhaled = true
        }
    },
    {
        name = "Bonkers",
        data = {
            bossType = 3,
            maxHealth = 80,
            arenaWidth = 256,
            arenaHeight = 160,
            powerType = 0,
            canBeInhaled = true
        }
    },
    {
        name = "Bugzzy",
        data = {
            bossType = 4,
            maxHealth = 70,
            arenaWidth = 256,
            arenaHeight = 160,
            powerType = 0,
            canBeInhaled = true
        }
    },
    {
        name = "Fire Lion",
        data = {
            bossType = 5,
            maxHealth = 75,
            arenaWidth = 320,
            arenaHeight = 160,
            powerType = 1,
            canBeInhaled = true
        }
    },
    {
        name = "Iron Mam",
        data = {
            bossType = 6,
            maxHealth = 100,
            arenaWidth = 256,
            arenaHeight = 180,
            powerType = 4,
            canBeInhaled = false
        }
    },
    {
        name = "Grand Wheely",
        data = {
            bossType = 7,
            maxHealth = 65,
            arenaWidth = 400,
            arenaHeight = 160,
            powerType = 9,
            canBeInhaled = true
        }
    },
    {
        name = "Box Boxer",
        data = {
            bossType = 8,
            maxHealth = 70,
            arenaWidth = 256,
            arenaHeight = 160,
            powerType = 0,
            canBeInhaled = true
        }
    },
    {
        name = "Master Hand",
        data = {
            bossType = 9,
            maxHealth = 120,
            arenaWidth = 400,
            arenaHeight = 240,
            powerType = 0,
            canBeInhaled = false
        }
    }
}

-- Texture based on boss type
function kirbyMidBoss.texture(room, entity)
    local bossType = entity.bossType or 0
    local textures = {
        [0] = "bosses/kirby/Maggy/DesoloZantas/whispy_woods/idle00",
        [1] = "bosses/kirby/Maggy/DesoloZantas/kracko/idle00",
        [2] = "bosses/kirby/Maggy/DesoloZantas/mr_frosty/idle00",
        [3] = "bosses/kirby/Maggy/DesoloZantas/bonkers/idle00",
        [4] = "bosses/Maggy/DesoloZantas/kirby/bugzzy/idle00",
        [5] = "bosses/Maggy/DesoloZantas/kirby/fire_lion/idle00",
        [6] = "bosses/Maggy/DesoloZantas/kirby/iron_mam/idle00",
        [7] = "bosses/Maggy/DesoloZantas/kirby/grand_wheely/idle00",
        [8] = "bosses/Maggy/DesoloZantas/kirby/box_boxer/idle00",
        [9] = "bosses/Maggy/DesoloZantas/kirby/master_hand/idle00"
    }
    return textures[bossType] or "characters/Maggy/DesoloZantas/Kglobal::Player/sitDown00"
end

-- Rectangle selection for bosses
function kirbyMidBoss.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = 32
    local height = 32
    
    -- Adjust size based on boss type
    local bossType = entity.bossType or 0
    if bossType == 0 then -- Whispy Woods
        width = 64
        height = 96
    elseif bossType == 1 then -- Kracko
        width = 48
        height = 32
    elseif bossType == 6 then -- Iron Mam
        width = 48
        height = 48
    elseif bossType == 9 then -- Master Hand
        width = 48
        height = 32
    end
    
    return utils.rectangle(x - width / 2, y - height, width, height)
end

return kirbyMidBoss


