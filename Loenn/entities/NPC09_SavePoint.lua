-- NPC09_SavePoint.lua
-- Loenn entity definition for the Fake Save Point NPC
-- This save point triggers the CS09_FakeSavePoint cutscene sequence

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local npc09_savepoint = {}

npc09_savepoint.name = "MaggyHelper/NPC09_SavePoint"
npc09_savepoint.depth = 100
npc09_savepoint.justification = {0.5, 1.0}
npc09_savepoint.texture = "characters/savepoint/save00"

-- Placements for Loenn
npc09_savepoint.placements = {
    {
        name = "NPC09_SavePoint",
        data = {
            -- No additional data needed, the NPC handles its own state
        }
    },
    {
        name = "NPC09_SavePoint_Trap",
        data = {
            -- Variant that starts in trap mode
        }
    }
}

-- Field information (even though minimal)
npc09_savepoint.fieldInformation = {
}

-- Selection box
function npc09_savepoint.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

-- Optional: custom sprite rendering
function npc09_savepoint.sprite(room, entity)
    local sprites = {}
    
    -- Main save point sprite
    local mainSprite = drawableSprite.fromTexture("characters/savepoint/save00", entity)
    mainSprite:setJustification(0.5, 1.0)
    table.insert(sprites, mainSprite)
    
    -- Add a warning indicator above it
    local warningSprite = drawableSprite.fromTexture("util/ingeste/trap_warning", entity)
    warningSprite:setJustification(0.5, 1.0)
    warningSprite:setPosition(entity.x, entity.y - 24)
    warningSprite:setScale(0.5, 0.5)
    warningSprite:setColor({1.0, 0.3, 0.3, 0.6})
    table.insert(sprites, warningSprite)
    
    return sprites
end

return npc09_savepoint
