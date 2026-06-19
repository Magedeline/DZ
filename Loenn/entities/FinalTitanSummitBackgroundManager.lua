local finalTitanSummitBackgroundManager = {}

finalTitanSummitBackgroundManager.name = "DZ/FinalTitanSummitBackgroundManager"
finalTitanSummitBackgroundManager.depth = 8900
finalTitanSummitBackgroundManager.texture = "@Internal@/summit_background_manager"
finalTitanSummitBackgroundManager.fieldInformation = {
    index = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 12
    },
    cloudStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    debrisStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    creatureStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    giygasStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    thunderStrengthMultiplier = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    cutscene = {
        fieldType = "string"
    },
    intro_launch = {
        fieldType = "boolean"
    },
    dark = {
        fieldType = "boolean"
    },
    ambience = {
        fieldType = "string"
    }
}
finalTitanSummitBackgroundManager.fieldOrder = {
    "x", "y",
    "width", "height",
    "index",
    "cloudStrengthMultiplier",
    "debrisStrengthMultiplier",
    "creatureStrengthMultiplier",
    "giygasStrengthMultiplier",
    "thunderStrengthMultiplier",
    "cutscene",
    "intro_launch",
    "dark",
    "ambience"
}
finalTitanSummitBackgroundManager.placements = {
    {
        name = "main",
        data = {
            width = 320,
            height = 180,
            index = 0,
            cloudStrengthMultiplier = 1.0,
            debrisStrengthMultiplier = 1.0,
            creatureStrengthMultiplier = 1.0,
            giygasStrengthMultiplier = 1.0,
            thunderStrengthMultiplier = 1.0,
            cutscene = "",
            intro_launch = false,
            dark = false,
            ambience = ""
        }
    }
}

return finalTitanSummitBackgroundManager
