local phaseBlock = {}

phaseBlock.name = "DZ/PhaseBlock"
phaseBlock.depth = 0
phaseBlock.fillColor = {0.5, 0.3, 0.7, 0.4}
phaseBlock.borderColor = {0.7, 0.4, 1.0, 0.6}
phaseBlock.placements = {
    {
        name = "PhaseBlock",
        data = { width = 16, height = 16, phaseSpeed = 1.0, phaseOffset = 0.0 }
    },
    {
        name = "fast",
        data = { width = 16, height = 16, phaseSpeed = 2.0, phaseOffset = 0.0 }
    },
    {
        name = "offset",
        data = { width = 16, height = 16, phaseSpeed = 1.0, phaseOffset = 0.5 }
    }
}
phaseBlock.fieldInformation = {
    phaseSpeed = { fieldType = "number", minimumValue = 0.1 },
    phaseOffset = { fieldType = "number", minimumValue = 0.0, maximumValue = 1.0 }
}
phaseBlock.fieldOrder = { "x", "y", "width", "height", "phaseSpeed", "phaseOffset" }

return phaseBlock
