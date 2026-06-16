local soul_barrier = {}

soul_barrier.name = "DesoloZantas/SoulBarrier"
soul_barrier.depth = 0
soul_barrier.placements = {
    {
        name = "Soul Barrier",
        data = {
            width = 8,
            height = 32,
            fragmentsRequired = 3,
            barrierId = "",
            dissolveTime = 1.0
        }
    }
}

soul_barrier.fieldInformation = {
    fragmentsRequired = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    dissolveTime = {
        fieldType = "number",
        minimumValue = 0.1
    }
}

function soul_barrier.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 8
    local height = entity.height or 32
    
    -- Draw barrier segments
    for y = 0, height - 8, 8 do
        local sprite = require("structs.drawable_sprite").fromTexture("objects/DesoloZantas/soulBarrier/segment", entity)
        sprite:setPosition(entity.x + width / 2, entity.y + y + 4)
        sprite:setColor({1.0, 0.4, 0.6, 0.8})
        table.insert(sprites, sprite)
    end
    
    return sprites
end

function soul_barrier.rectangle(room, entity)
    return {
        x = entity.x,
        y = entity.y,
        width = entity.width or 8,
        height = entity.height or 32
    }
end

return soul_barrier
