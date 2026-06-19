local wavePhazeMachine = {}

wavePhazeMachine.name = "DZ/WaveFazeTutorialMachine"
wavePhazeMachine.depth = 0

wavePhazeMachine.placements = {
    {
        name = "main",
        data = {
            isActive = true,
            machineType = "tutorial",
            difficulty = "easy"
        }
    },
    {
        name = "WaveFazeTutorialMachine_advanced",
        data = {
            isActive = true,
            machineType = "challenge",
            difficulty = "hard"
        }
    }
}

wavePhazeMachine.fieldInformation = {
    isActive = {
        fieldType = "boolean"
    },
    machineType = {
        options = {"tutorial", "challenge", "bonus"},
        editable = false
    },
    difficulty = {
        options = {"easy", "normal", "hard", "expert"},
        editable = false
    }
}

function wavePhazeMachine.sprite(room, entity)
    local color = {0.6, 0.4, 0.9, 1.0}
    
    if entity.difficulty == "hard" then
        color = {0.9, 0.4, 0.1, 1.0}
    elseif entity.difficulty == "expert" then
        color = {0.9, 0.1, 0.1, 1.0}
    end
    
    return {
        {
            texture = "objects/kevins_pc/pc_idle",
            x = entity.x,
            y = entity.y,
            color = color
        }
    }
end

function wavePhazeMachine.selection(room, entity)
    return {entity.x - 16, entity.y - 16, 32, 32}
end

return wavePhazeMachine
