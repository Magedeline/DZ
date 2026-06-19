local flower_vines = {}

local fearDistanceOptions = {
    None = "",
    Close = "close",
    Medium = "medium",
    Far = "far"
}

flower_vines.name = "DZ/CharaFlowerVines"
flower_vines.depth = 0
flower_vines.texture = "@Internal@/tentacles"
flower_vines.nodeLineRenderType = "line"
flower_vines.nodeLimits = {1, -1}
flower_vines.fieldInformation = {
    slide_until = {
        fieldType = "integer",
    },
    fear_distance = {
        options = fearDistanceOptions,
        editable = false
    }
}
flower_vines.placements = {
    {
        name = "main",
        data = {
        fear_distance = "",
        slide_until = 0
        }
    }
}

return flower_vines