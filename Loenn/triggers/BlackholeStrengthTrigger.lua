local enums = require("consts.ingeste_enums")

local BlackholeStrengthTrigger = {}

BlackholeStrengthTrigger.name = "DZ/BlackholeStrengthTrigger"
BlackholeStrengthTrigger.placements = {
    name = "main",
    data = {
        strength = "Medium"
    }
}

BlackholeStrengthTrigger.fieldInformation = {
    strength = {
        options = enums.black_hole_strengths,
        editable = false
    }
}

return BlackholeStrengthTrigger