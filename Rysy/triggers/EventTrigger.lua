local Trigger = {}

Trigger.name = "MaggyHelper/EventTrigger"
Trigger.canResize = {true, true}

function Trigger.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 16,
        height = entity.height or 16,
        event = entity.event or "",
        onSpawn = entity.onSpawn or false,
        onlyOnce = entity.onlyOnce or true
    }
end

function Trigger.draw(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 16
    local height = entity.height or 16
    local hasEvent = entity.event and entity.event ~= ""
    local color = hasEvent and {0.35, 0.8, 0.95, 0.8} or {0.85, 0.35, 0.35, 0.8}
    return {
        type = "rectangle",
        x = x,
        y = y,
        width = width,
        height = height,
        color = color,
        borderColor = {1.0, 1.0, 1.0, 1.0}
    }
end

return Trigger
