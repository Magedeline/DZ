-- Loenn plugin for DZ - Additional Enemies
local drawableSprite = require("structs.drawable_sprite")

-- Hot Head (Fire enemy)
local hotHead = {}
hotHead.name = "DZ/HotHead"
hotHead.depth = 0
hotHead.placements = {
    {
        name = "main",
        data = {
        health = 1,
        moveSpeed = 25.0,
        canBeInhaled = true
        }
    }
}
hotHead.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    moveSpeed = { minimumValue = 0 }
}
function hotHead.sprite(room, entity)
    local texture = "objects/enemies/hotHead/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

-- Chilly (Ice enemy)
local chilly = {}
chilly.name = "DZ/Chilly"
chilly.depth = 0
chilly.placements = {
    {
        name = "main",
        data = {
        health = 1,
        moveSpeed = 15.0,
        canBeInhaled = true
        }
    }
}
chilly.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    moveSpeed = { minimumValue = 0 }
}
function chilly.sprite(room, entity)
    local texture = "objects/enemies/chilly/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

-- Sparky (Spark enemy)
local sparky = {}
sparky.name = "DZ/Sparky"
sparky.depth = 0
sparky.placements = {
    {
        name = "main",
        data = {
        health = 1,
        moveSpeed = 20.0,
        canBeInhaled = true
        }
    }
}
sparky.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    moveSpeed = { minimumValue = 0 }
}
function sparky.sprite(room, entity)
    local texture = "objects/enemies/sparky/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

-- Blade Knight (Sword enemy)
local bladeKnight = {}
bladeKnight.name = "DZ/BladeKnight"
bladeKnight.depth = 0
bladeKnight.placements = {
    {
        name = "main",
        data = {
        health = 2,
        moveSpeed = 35.0,
        canBeInhaled = true
        }
    }
}
bladeKnight.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    moveSpeed = { minimumValue = 0 }
}
function bladeKnight.sprite(room, entity)
    local texture = "objects/enemies/bladeKnight/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

return {
    hotHead = hotHead,
    chilly = chilly,
    sparky = sparky,
    bladeKnight = bladeKnight
}
