local Entity = {}

Entity.name = "MaggyHelper/PowerGenerator"
Entity.depth = 0

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        flipX = entity.flipX or false,
        flag = entity.flag or false,
        music = entity.music or "",
        musicProgress = entity.musicProgress or -1,
        musicStoreInSession = entity.musicStoreInSession or false,
        canTeleport = entity.canTeleport or false,
        endX = entity.endX or 0,
        endY = entity.endY or 0
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/powergenerator/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
