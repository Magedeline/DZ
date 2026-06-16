-- Glitch Effect for Lönn
local glitchEffect = {}

glitchEffect.name = "DZ/GlitchEffect"
glitchEffect.canForeground = true
glitchEffect.canBackground = true

glitchEffect.fieldInformation = {
    glitchAmount = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    amplitude = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 0.5
    }
}

glitchEffect.placements = {
    {
        name = "glitch",
        data = {
            glitchAmount = 0.0,
            amplitude = 0.05
        }
    }
}

return glitchEffect
