local enhancedPowerGenerator = {}

enhancedPowerGenerator.name = "MaggyHelper/EnhancedPowerGenerator"
enhancedPowerGenerator.depth = 0
enhancedPowerGenerator.texture = "objects/IngesteHelper/enhanced_power_generator"
enhancedPowerGenerator.justification = {0.5, 1.0}

enhancedPowerGenerator.fieldInformation = {
    generatorType = {
        options = {"solar", "wind", "fusion", "crystal", "magical"},
        editable = false
    },
    powerOutput = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 100.0
    },
    efficiency = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 1.0
    },
    isActive = {
        fieldType = "boolean"
    },
    requiresFuel = {
        fieldType = "boolean"
    },
    fuelType = {
        fieldType = "string",
        options = {
            "none",
            "coal",
            "uranium",
            "crystal",
            "magic"
        },
        editable = true
    },
    maxFuelCapacity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1000.0
    },
    currentFuel = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1000.0
    },
    showParticles = {
        fieldType = "boolean"
    },
    particleColor = {
        fieldType = "color"
    }
}

enhancedPowerGenerator.placements = {
    {
        name = "solar",
        data = {
            generatorType = "solar",
            powerOutput = 25.0,
            efficiency = 0.8,
            isActive = true,
            requiresFuel = false,
            fuelType = "none",
            maxFuelCapacity = 0.0,
            currentFuel = 0.0,
            showParticles = true,
            particleColor = "ffff44"
        }
    },
    {
        name = "fusion",
        data = {
            generatorType = "fusion",
            powerOutput = 80.0,
            efficiency = 0.9,
            isActive = false,
            requiresFuel = true,
            fuelType = "uranium",
            maxFuelCapacity = 500.0,
            currentFuel = 100.0,
            showParticles = true,
            particleColor = "44ffff"
        }
    },
    {
        name = "magical",
        data = {
            generatorType = "magical",
            powerOutput = 60.0,
            efficiency = 0.7,
            isActive = true,
            requiresFuel = true,
            fuelType = "magic",
            maxFuelCapacity = 200.0,
            currentFuel = 200.0,
            showParticles = true,
            particleColor = "ff44ff"
        }
    }
}

function enhancedPowerGenerator.sprite(room, entity)
    local generatorType = entity.generatorType or "solar"
    
    return {
        texture = "objects/IngesteHelper/enhanced_power_generator_" .. generatorType,
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return enhancedPowerGenerator