local beyondsummitBackgroundManager = {}

beyondsummitBackgroundManager.name = "DZ/BeyondSummitCloud"
beyondsummitBackgroundManager.depth = 0
beyondsummitBackgroundManager.texture = "@Internal@/summit_background_manager"
beyondsummitBackgroundManager.fieldInformation = {
    index = {
        fieldType = "integer",
    }
}
beyondsummitBackgroundManager.placements = {
    name = "BeyondSummitCloud",
    data = {
        index = 0,
        cutscene = "",
        intro_launch = false,
        dark = false,
        ambience = ""
    }
}

return beyondsummitBackgroundManager