local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local oshiroLobbyBell = {}

oshiroLobbyBell.name = "MaggyHelper/OshiroLobbyBell"
oshiroLobbyBell.depth = 0
oshiroLobbyBell.justification = {0.5, 1.0}

-- NPC05_Oshiro_Lobby spawns this bell at (npc.X - 14, npc.Y)
-- CS05_OshiroLobby cutscene triggers on bell talk, flag: "oshiro_resort_talked_1"
-- After cutscene, bell talk becomes active (plays "event:/game/05_restore/deskbell_again")

oshiroLobbyBell.placements = {
    {
        name = "oshiroLobbyBell",
        data = {}
    }
}

-- Render a desk bell sprite with a ghost Oshiro NPC offset to the right
-- matching the NPC05_Oshiro_Lobby relationship (NPC is +14px to the right of the bell)
function oshiroLobbyBell.sprite(room, entity)
    local sprites = {}

    -- Oshiro ghost sprite (NPC05_Oshiro_Lobby stands 14px to the right of the bell)
    local oshiroSprite = drawableSprite.fromTexture("characters/Maggy/DesoloZantas/oshiro/oshiro25", entity)
    if oshiroSprite then
        oshiroSprite:setJustification(0.5, 1.0)
        oshiroSprite:addPosition(14, 0)
        oshiroSprite:setColor({1.0, 1.0, 1.0, 0.4})
        table.insert(sprites, oshiroSprite)
    end

    -- Bell sprite (the actual entity visual)
    local bellSprite = drawableSprite.fromTexture("objects/introscene/deskbell", entity)
    if bellSprite then
        bellSprite:setJustification(0.5, 1.0)
        table.insert(sprites, bellSprite)
    end

    if #sprites == 0 then
        local fallback = drawableSprite.fromTexture("characters/Maggy/DesoloZantas/oshiro/oshiro25", entity)
        if fallback then
            fallback:setJustification(0.5, 1.0)
            table.insert(sprites, fallback)
        end
    end

    return sprites
end

function oshiroLobbyBell.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return oshiroLobbyBell
