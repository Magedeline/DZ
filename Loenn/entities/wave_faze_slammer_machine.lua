local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local waveFazeSlammerMachine = {}

waveFazeSlammerMachine.name = "DZ/WaveFazeSlammerTutorialMachine"
waveFazeSlammerMachine.depth = 1000
waveFazeSlammerMachine.placements = {
    {
        name = "main",
        data = {}
    }
}

-- Fixed width from the C# constructor
local machineWidth = 88
local machineHeight = 60

-- Sprite information for rendering
function waveFazeSlammerMachine.sprite(room, entity)
    local sprites = {}
    
    -- Back sprite (building_back)
    local backSprite = drawableSprite.fromTexture("objects/wavedashtutorial/building_back", entity)
    backSprite:setJustification(0.5, 1.0)
    backSprite:setPosition(entity.x, entity.y)
    table.insert(sprites, backSprite)
    
    -- Front left sprite
    local frontLeftSprite = drawableSprite.fromTexture("objects/wavedashtutorial/building_front_left", entity)
    frontLeftSprite:setJustification(0.5, 1.0)
    frontLeftSprite:setPosition(entity.x, entity.y)
    table.insert(sprites, frontLeftSprite)
    
    -- Front right sprite (rendered at lower depth in game)
    local frontRightSprite = drawableSprite.fromTexture("objects/wavedashtutorial/building_front_right", entity)
    frontRightSprite:setJustification(0.5, 1.0)
    frontRightSprite:setPosition(entity.x, entity.y)
    table.insert(sprites, frontRightSprite)
    
    -- Noise sprite (animated in game, just show first frame)
    local noiseSprite = drawableSprite.fromTexture("objects/wavedashtutorial/noise00", entity)
    noiseSprite:setJustification(0.5, 0.5)
    noiseSprite:setPosition(entity.x, entity.y - 30)
    noiseSprite:setColor({1.0, 1.0, 1.0, 0.5})
    table.insert(sprites, noiseSprite)
    
    -- Neon sprite (animated in game, just show first frame)
    local neonSprite = drawableSprite.fromTexture("objects/wavedashtutorial/neon_00", entity)
    neonSprite:setJustification(0.5, 1.0)
    neonSprite:setPosition(entity.x, entity.y)
    table.insert(sprites, neonSprite)
    
    return sprites
end

-- Selection box for the entity
-- Hitbox is at position (-41, -59) with size 88x60 from the C# code
function waveFazeSlammerMachine.selection(room, entity)
    return utils.rectangle(entity.x - 41, entity.y - 59, machineWidth, machineHeight)
end

-- Render a rectangle for the jump-thru platform area
function waveFazeSlammerMachine.rectangle(room, entity)
    return utils.rectangle(entity.x - 41, entity.y - 59, machineWidth, 8)
end

return waveFazeSlammerMachine
