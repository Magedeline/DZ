local enums = require("consts.celeste_enums")

local giygasGlitchBackground = {}

giygasGlitchBackground.name = "MaggyHelper/GiygasGlitch"
giygasGlitchBackground.fieldInformation = {
    duration = {
        options = enums.giygas_glitch_background_trigger_durations,
        editable = true
    }
}
giygasGlitchBackground.placements = {
    name = "giygas_glitch_background",
    data = {
        duration = "Short",
        stay = false,
        glitch = true
    }
}

return giygasGlitchBackground