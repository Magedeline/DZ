local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local obsidianShard = {}

obsidianShard.name = "DZ/ObsidianShard"
obsidianShard.depth = -50
obsidianShard.placements = {
    {
        name = "main",
        data = {}
    }
}

function obsidianShard.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/obsidian_shard/default", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.2, 0.0, 0.3, 1.0}
    return sprite
end

function obsidianShard.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return obsidianShard
