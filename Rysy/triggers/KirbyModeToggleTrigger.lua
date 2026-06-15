local Trigger = {}

Trigger.name = "MaggyHelper/Kirby_Mode_Toggle_Trigger"
Trigger.canResize = {true, true}

function Trigger.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 16,
        height = entity.height or 16,
        enableKirbyMode = entity.enableKirbyMode or true,
        oneTime = entity.oneTime or false,
        restoreOnLeave = entity.restoreOnLeave or false
    }
end

function Trigger.draw(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 16
    local height = entity.height or 16
    local enabled = entity.enableKirbyMode ~= false
    local color = enabled and {0.2, 0.8, 0.2, 0.4} or {0.8, 0.2, 0.2, 0.4}
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
