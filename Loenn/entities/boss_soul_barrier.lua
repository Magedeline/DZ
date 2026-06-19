local boss_soul_barrier = {}

boss_soul_barrier.name = "DZ/BossSoulBarrier"
boss_soul_barrier.depth = 0
boss_soul_barrier.placements = {
    {
        name = "Titan King",
        data = {
            width = 8,
            height = 32,
            bossType = 0,
            barrierId = "titan_king",
            breakAfterCutscene = true
        }
    },
    {
        name = "Guardian Titan",
        data = {
            width = 8,
            height = 32,
            bossType = 1,
            barrierId = "guardian_titan",
            breakAfterCutscene = true
        }
    },
    {
        name = "Chapter 16 Els",
        data = {
            width = 8,
            height = 32,
            bossType = 2,
            barrierId = "ch16_els",
            breakAfterCutscene = true
        }
    },
    {
        name = "Asriel Angel of Death",
        data = {
            width = 8,
            height = 32,
            bossType = 3,
            barrierId = "asriel_angel",
            breakAfterCutscene = true
        }
    },
    {
        name = "Els True Final",
        data = {
            width = 8,
            height = 32,
            bossType = 4,
            barrierId = "els_true_final",
            breakAfterCutscene = true
        }
    },
    {
        name = "Asriel Break Giygas",
        data = {
            width = 8,
            height = 32,
            bossType = 5,
            barrierId = "asriel_giygas",
            breakAfterCutscene = true
        }
    }
}

boss_soul_barrier.fieldInformation = {
    bossType = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 5,
        options = {
            ["Titan King"] = 0,
            ["Guardian Titan"] = 1,
            ["Chapter 16 Els"] = 2,
            ["Asriel Angel of Death"] = 3,
            ["Els True Final"] = 4,
            ["Asriel Break Giygas"] = 5
        },
        editable = false
    }
}

function boss_soul_barrier.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 8
    local height = entity.height or 32
    
    local colors = {
        {1.0, 0.5, 0.0, 0.8},
        {0.5, 0.5, 0.5, 0.8},
        {0.5, 0.0, 0.0, 0.8},
        {1.0, 0.8, 0.0, 0.8},
        {0.5, 0.0, 0.5, 0.8},
        {0.1, 0.1, 0.1, 0.8}
    }
    
    local color = colors[(entity.bossType or 0) + 1]
    
    for y = 0, height - 8, 8 do
        local sprite = require("structs.drawable_sprite").fromTexture("objects/DesoloZantas/soulBarrier/segment", entity)
        sprite:setPosition(entity.x + width / 2, entity.y + y + 4)
        sprite:setColor(color)
        table.insert(sprites, sprite)
    end
    
    return sprites
end

function boss_soul_barrier.rectangle(room, entity)
    return {
        x = entity.x,
        y = entity.y,
        width = entity.width or 8,
        height = entity.height or 32
    }
end

return boss_soul_barrier
