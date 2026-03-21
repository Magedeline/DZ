local memory_crystal = {}

memory_crystal.name = "DesoloZantas/MemoryCrystal"
memory_crystal.depth = -100
memory_crystal.placements = {
    {
        name = "Memory Crystal",
        data = {
            memoryId = "",
            oneTime = true,
            flashbackDuration = 3.0
        }
    }
}

memory_crystal.fieldInformation = {
    flashbackDuration = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 10.0
    }
}

function memory_crystal.sprite(room, entity)
    local sprite = require("structs.drawable_sprite").fromTexture("collectables/cassette/idle00", entity)
    sprite:setPosition(entity.x + 8, entity.y + 8)
    sprite:setColor({0.2, 0.8, 0.9, 1.0})
    sprite:setScale(1.2, 1.2)
    
    return sprite
end

memory_crystal.offset = {8, 8}

return memory_crystal
