local powerGenerator = {}

powerGenerator.name = "MaggyHelper/PowerGenerator"
powerGenerator.depth = 0

powerGenerator.placements = {
    {
        name = "PowerGenerator",
        data = {
            isActive = true,
            generatorType = "basic",
            powerOutput = 100.0,
            fuelLevel = 100.0
        }
    },
    {
        name = "PowerGeneratoradvanced",
        data = {
            isActive = true,
            generatorType = "advanced",
            powerOutput = 250.0,
            fuelLevel = 100.0
        }
    },
    {
        name = "PowerGeneratormagical",
        data = {
            isActive = true,
            generatorType = "magical",
            powerOutput = 500.0,
            fuelLevel = 100.0
        }
    }
}

powerGenerator.fieldInformation = {
    isActive = {
        fieldType = "boolean"
    },
    generatorType = {
        options = {"basic", "advanced", "magical", "nuclear"},
        editable = false
    },
    powerOutput = {
        fieldType = "number",
        minimumValue = 50.0,
        maximumValue = 1000.0
    },
    fuelLevel = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 100.0
    }
}

function powerGenerator.sprite(room, entity)
    local colors = {
        basic = {0.7, 0.7, 0.8, 1.0},
        advanced = {0.3, 0.8, 0.9, 1.0},
        magical = {0.8, 0.3, 1.0, 1.0},
        nuclear = {0.9, 0.9, 0.3, 1.0}
    }
    
    local color = colors[entity.generatorType] or colors.basic
    
    if not entity.isActive then
        color = {0.5, 0.5, 0.5, 0.7}
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

function powerGenerator.selection(room, entity)
    return {entity.x - 16, entity.y - 16, 32, 32}
end

return powerGenerator
