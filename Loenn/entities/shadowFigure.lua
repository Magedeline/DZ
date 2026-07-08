local shadowFigure = {}

shadowFigure.name = "DZ/ShadowFigure"
shadowFigure.depth = 0
shadowFigure.texture = "objects/DZ/DZ/DZ/shadow_figure"

shadowFigure.placements = {
    name = "shadow_figure",
    data = {
        isHostile = false,
        detectionRange = 150,
        followDistance = 80
    }
}

return shadowFigure