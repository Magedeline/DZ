local ascendManagerBeyond = {}

ascendManagerBeyond.name = "MaggyHelper/AscendManagerBeyond"
ascendManagerBeyond.depth = 8900
ascendManagerBeyond.texture = "@Internal@/summit_background_manager"
ascendManagerBeyond.minimumSize = {8, 8}

ascendManagerBeyond.fieldInformation = {
    index = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 9,
    }
}

ascendManagerBeyond.placements = {
    {
        name = "AscendManagerBeyond",
        data = {
            width = 32,
            height = 32,
            index = 0,
            cutscene = "",
            intro_launch = false,
            dark = false,
            arrivial = false,
            ambience = "",
        }
    },
    {
        name = "AscendManagerBeyond_dark",
        data = {
            width = 32,
            height = 32,
            index = 0,
            cutscene = "",
            intro_launch = false,
            dark = true,
            arrivial = false,
            ambience = "",
        }
    },
    {
        name = "AscendManagerBeyond_ch19_ending",
        data = {
            width = 32,
            height = 32,
            index = 9,
            cutscene = "",
            intro_launch = false,
            dark = false,
            arrivial = true,
            ambience = "",
        }
    },
}

function ascendManagerBeyond.selection(room, entity)
    return require("structs.rectangle")(entity.x, entity.y, entity.width or 32, entity.height or 32)
end

return ascendManagerBeyond
