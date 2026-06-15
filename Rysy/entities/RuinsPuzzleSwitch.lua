-- RySy plugin for MaggyHelper - RuinsPuzzleSwitch (Ch10 Ruins)
local Entity = {}

Entity.name = "MaggyHelper/RuinsPuzzleSwitch"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        switchType = entity.switchType or "Simple",
        gateId = entity.gateId or "",
        sequenceOrder = entity.sequenceOrder or 0,
        holdTime = entity.holdTime or 1.0,
        timerDuration = entity.timerDuration or 3.0
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/ruins_puzzle_switch/inactive",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
