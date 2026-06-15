local Entity = {}

Entity.name = "MaggyHelper/AbilityStar"
Entity.depth = -50

local abilityTextures = {
    ["None"] = "items/abilitystar/none00",
    ["Fire"] = "items/abilitystar/fire00",
    ["Ice"] = "items/abilitystar/ice00",
    ["Spark"] = "items/abilitystar/spark00",
    ["Sword"] = "items/abilitystar/sword00",
    ["Cutter"] = "items/abilitystar/cutter00",
    ["Beam"] = "items/abilitystar/beam00",
    ["Stone"] = "items/abilitystar/stone00",
    ["Needle"] = "items/abilitystar/needle00",
    ["Parasol"] = "items/abilitystar/parasol00",
    ["Wheel"] = "items/abilitystar/wheel00",
    ["Bomb"] = "items/abilitystar/bomb00",
    ["Fighter"] = "items/abilitystar/fighter00",
    ["Suplex"] = "items/abilitystar/suplex00",
    ["Ninja"] = "items/abilitystar/ninja00",
    ["Mirror"] = "items/abilitystar/mirror00",
    ["Hammer"] = "items/abilitystar/hammer00",
    ["Knight"] = "items/abilitystar/knight00",
    ["Wing"] = "items/abilitystar/wing00",
    ["UFO"] = "items/abilitystar/ufo00",
    ["Sleep"] = "items/abilitystar/sleep00"
}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        ability = entity.ability or "None"
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local ability = entity.ability or "None"
    local texture = abilityTextures[ability] or "items/abilitystar/none00"
    return {
        texture = texture,
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
