-- RySy plugin for MaggyHelper - Cassette
local Entity = {}

Entity.name = "MaggyHelper/Cassette"
Entity.depth = -1000000

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        onCollect = entity.onCollect or "",
        customAudio = entity.customAudio or "",
        particleColor = entity.particleColor or "9CFCFF",
        glowStrength = entity.glowStrength or 1.0,
        bloomStrength = entity.bloomStrength or 0.8,
        wiggleIntensity = entity.wiggleIntensity or 0.35,
        floatSpeed = entity.floatSpeed or 2.0,
        floatRange = entity.floatRange or 2.0,
        collectDelay = entity.collectDelay or 0.3,
        persistent = entity.persistent ~= false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "collectables/cassette/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
