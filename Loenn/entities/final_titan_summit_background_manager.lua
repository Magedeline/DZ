local finalTitanSummitBackgroundManager = {}

finalTitanSummitBackgroundManager.name = "MaggyHelper/FinalTitanSummitBackgroundManager"
finalTitanSummitBackgroundManager.depth = 0
finalTitanSummitBackgroundManager.texture = "@Internal@/summit_background_manager"

finalTitanSummitBackgroundManager.fieldInformation = {
    index = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 12,
    },
    cloudStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
    },
    debrisStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
    },
    creatureStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
    },
    giygasStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
    },
    thunderStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
    }
}

finalTitanSummitBackgroundManager.fieldOrder = {
    "x",
    "y",
    "index",
    "cutscene",
    "intro_launch",
    "dark",
    "ambience",
    "cloudStrengthMultiplier",
    "debrisStrengthMultiplier",
    "creatureStrengthMultiplier",
    "giygasStrengthMultiplier",
    "thunderStrengthMultiplier"
}

finalTitanSummitBackgroundManager.placements = {
    {
        name = "FinalTitanSummitBackgroundManager",
        data = {
            index = 0,
            cutscene = "",
            intro_launch = false,
            dark = false,
            ambience = "",
            cloudStrengthMultiplier = 1.0,
            debrisStrengthMultiplier = 1.0,
            creatureStrengthMultiplier = 1.0,
            giygasStrengthMultiplier = 1.0,
            thunderStrengthMultiplier = 1.0
        }
    }
}

return finalTitanSummitBackgroundManager
