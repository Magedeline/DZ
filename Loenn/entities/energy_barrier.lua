local energyBarrier = {}

energyBarrier.name = "DZ/EnergyBarrier"
energyBarrier.depth = 0
energyBarrier.texture = "objects/IngesteHelper/energy_barrier"
energyBarrier.justification = {0.5, 0.5}
energyBarrier.nodeLineRenderType = "line"
energyBarrier.nodeLimits = {0, 1}

energyBarrier.fieldInformation = {
    barrierType = {
        options = {"normal", "damage", "blocking", "one_way"},
        editable = false
    },
    color = {
        fieldType = "color"
    },
    isActive = {
        fieldType = "boolean"
    },
    strength = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    flickerRate = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0
    },
    requiresPower = {
        fieldType = "boolean"
    },
    flagName = {
        fieldType = "string"
    }
}

energyBarrier.placements = {
    {
        name = "normal",
        data = {
            barrierType = "normal",
            color = "00ffff",
            isActive = true,
            strength = 5.0,
            flickerRate = 0.0,
            requiresPower = false,
            flagName = ""
        }
    },
    {
        name = "damage",
        data = {
            barrierType = "damage",
            color = "ff4444",
            isActive = true,
            strength = 8.0,
            flickerRate = 1.0,
            requiresPower = false,
            flagName = ""
        }
    },
    {
        name = "one_way",
        data = {
            barrierType = "one_way",
            color = "44ff44",
            isActive = true,
            strength = 3.0,
            flickerRate = 0.0,
            requiresPower = false,
            flagName = ""
        }
    }
}

return energyBarrier