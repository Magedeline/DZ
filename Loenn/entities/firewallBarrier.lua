local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local firewallBarrier = {}

firewallBarrier.name = "DZ/FirewallBarrier"
firewallBarrier.depth = -9000
firewallBarrier.minimumSize = {8, 8}
firewallBarrier.fieldInformation = {
    maxEnergy = {
        minimumValue = 0
    },
    energyDrain = {
        minimumValue = 0
    }
}
firewallBarrier.placements = {
    {
        name = "main",
        data = {
            width = 32,
            height = 32,
            maxEnergy = 100,
            energyDrain = 5,
            startActive = true
        }
    },
    {
        name = "inactive",
        data = {
            width = 32,
            height = 32,
            maxEnergy = 100,
            energyDrain = 5,
            startActive = false
        }
    }
}

function firewallBarrier.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 32
    local height = entity.height or 32
    local state = entity.startActive and "active" or "inactive"
    
    for x = 0, width - 8, 8 do
        for y = 0, height - 8, 8 do
            local sprite = drawableSprite.fromTexture("objects/firewall_barrier/" .. state, entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            if entity.startActive then
                sprite.color = {1.0, 0.5, 0.0, 0.8}
            end
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function firewallBarrier.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 32, entity.height or 32)
end

return firewallBarrier
