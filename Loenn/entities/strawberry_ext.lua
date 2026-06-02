local strawberryExt = {}

strawberryExt.name = "MaggyHelper/StrawberryExt"
strawberryExt.depth = -100
strawberryExt.nodeLineRenderType = "line"
strawberryExt.texture = "collectables/strawberry/normal00"
strawberryExt.nodeLimits = {0, -1}

strawberryExt.fieldInformation = {
    winged = {
        fieldType = "boolean"
    },
    moon = {
        fieldType = "boolean"
    },
    popstar = {
        fieldType = "boolean"
    }
}

strawberryExt.fieldOrder = {
    "x", "y",
    "winged", "moon", "popstar"
}

strawberryExt.placements = {
    {
        name = "normal",
        data = {
            winged = false,
            moon = false,
            popstar = false
        }
    },
    {
        name = "winged",
        data = {
            winged = true,
            moon = false,
            popstar = false
        }
    },
    {
        name = "moon",
        data = {
            winged = false,
            moon = true,
            popstar = false
        }
    },
    {
        name = "popstar",
        data = {
            winged = false,
            moon = false,
            popstar = true
        }
    }
}

function strawberryExt.nodeTexture(room, entity, node, nodeIndex, viewport)
    return "collectables/strawberry/seed00"
end

return strawberryExt
