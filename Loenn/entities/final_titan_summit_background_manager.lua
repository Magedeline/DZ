local finalTitanSummitBackgroundManager = {}

finalTitanSummitBackgroundManager.name = "MaggyHelper/FinalTitanSummitBackgroundManager"
finalTitanSummitBackgroundManager.depth = 0
finalTitanSummitBackgroundManager.texture = "@Internal@/summit_background_manager"

finalTitanSummitBackgroundManager.fieldInformation = {
    index = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 12,
    }
}

finalTitanSummitBackgroundManager.placements = {
    {
        name = "FinalTitanSummitBackgroundManager",
        data = {
            index = 0,
            cutscene = "",
            intro_launch = false,
            dark = false,
            ambience = ""
        }
    }
}

return finalTitanSummitBackgroundManager
