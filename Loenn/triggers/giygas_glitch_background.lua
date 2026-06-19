local enums = require("consts.celeste_enums")

local giygasGlitchBackground = {}

giygasGlitchBackground.name = "DZ/GiygasGlitch"
giygasGlitchBackground.fieldInformation = {
    duration = {
        options = enums.giygas_glitch_background_trigger_durations,
        editable = true
    }
}
giygasGlitchBackground.placements = {
    name = "main",
    data = {
        duration = "Short",
        stay = false,
        glitch = true
    }
}

return giygasGlitchBackground