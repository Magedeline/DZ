local Trigger = {}

Trigger.name = "DZ/WeatherChangeTrigger"
Trigger.canResize = {true, true}

function Trigger.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 16,
        height = entity.height or 16,
        weather = entity.weather or "None",
        intensity = entity.intensity or 1.0,
        transitionTime = entity.transitionTime or 1.0
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
        color = {0.4, 0.6, 0.9, 0.4},
        borderColor = {0.6, 0.8, 1.0, 1.0}
    }
end

return Trigger
