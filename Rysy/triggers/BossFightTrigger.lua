local Trigger = {}

Trigger.name = "MaggyHelper/BossFightTrigger"
Trigger.canResize = {true, true}

function Trigger.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 16,
        height = entity.height or 16,
        bossId = entity.bossId or "",
        music = entity.music or "",
        introEvent = entity.introEvent or "",
        defeatEvent = entity.defeatEvent or ""
    }
end

function Trigger.draw(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 16
    local height = entity.height or 16
    return {
        type = "rectangle",
        x = x,
        y = y,
        width = width,
        height = height,
        color = {0.9, 0.2, 0.2, 0.4},
        borderColor = {1.0, 0.5, 0.5, 1.0}
    }
end

return Trigger
