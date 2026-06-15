local Entity = {}

Entity.name = "MaggyHelper/KirbyPlayerSpawner"
Entity.depth = -1000000

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        enableKirbyMode = entity.enableKirbyMode or true,
        spawnCompanion = entity.spawnCompanion or false,
        startingAbility = entity.startingAbility or "None"
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local texture = "characters/kirby/idle00"
    if entity.enableKirbyMode == false then
        texture = "characters/player/sitDown00"
    end
    return {
        texture = texture,
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
