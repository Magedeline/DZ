-- RySy plugin for MaggyHelper - Cartridge
local Entity = {}

Entity.name = "MaggyHelper/Cartridge"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        spritePath = entity.spritePath or "collectables/cartridge/",
        menuSprite = entity.menuSprite or "collectables/cartridge",
        unlockText = entity.unlockText or "",
        remixExtraToUnlock = entity.remixExtraToUnlock or "",
        onCollect = entity.onCollect or "",
        customAudio = entity.customAudio or "",
        particleColor = entity.particleColor or "FFD700",
        glowStrength = entity.glowStrength or 1.5,
        bloomStrength = entity.bloomStrength or 1.0,
        wiggleIntensity = entity.wiggleIntensity or 0.5,
        floatSpeed = entity.floatSpeed or 1.5,
        floatRange = entity.floatRange or 3.0,
        collectDelay = entity.collectDelay or 0.5,
        persistent = entity.persistent ~= false,
        isChapter19Finale = entity.isChapter19Finale or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "collectables/cartridge/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
