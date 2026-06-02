local customBerry = {}

customBerry.name = "MaggyHelper/CustomBerry"
customBerry.depth = -100
customBerry.nodeLineRenderType = "fan"
customBerry.nodeLimits = {0, -1}

customBerry.fieldInformation = {
    berryType = {
        options = {
            ["strawberry"] = "strawberry",
            ["moonberry"] = "moonberry",
            ["voidstarberry"] = "voidstarberry",
            ["popstarberry"] = "popstarberry",
            ["pinkplatinumberry"] = "pinkplatinumberry",
    }
    },
    order = {
        fieldType = "integer",
    },
    checkpointID = {
        fieldType = "integer"
    },
    winged = {
        fieldType = "boolean"
    },
    collectSound = {
        options = {
            ["Original"] = "Original",
            ["Elaborate"] = "Elaborate",
            ["Minimalist"] = "Minimalist", 
            ["Custom"] = "Custom"
        }
    },
    customCollectSound = {
        fieldType = "string"
    },
    levelSet = {
        fieldType = "string"
    },
    maps = {
        fieldType = "string"
    },
    requires = {
        fieldType = "string"
    }
}

function customBerry.texture(room, entity)
    local berryType = entity.berryType or "strawberry"
    local winged = entity.winged
    local hasNodes = entity.nodes and #entity.nodes > 0

    if berryType == "moonberry" then
        if winged or hasNodes then
            return "collectables/moonBerry/ghost00"
        else
            return "collectables/moonBerry/normal00"
        end
    elseif berryType == "voidstarberry" then
        if winged or hasNodes then
            return "collectables/voidstarberry/ghost/000"
        else
            return "collectables/maggy/voidstarberry/spin/000"
        end
    elseif berryType == "popstarberry" then
        if winged or hasNodes then
            return "collectables/popstarberry/ghost/000"
        else
            return "collectables/popstarberry/spin/000"
        end
    elseif berryType == "pinkplatinumberry" then
        if winged or hasNodes then
            return "collectables/maggy/pinkplatberry/ghost00"
        else
            return "collectables/maggy/pinkplatberry/idle00"
        end
    else -- strawberry (default)
        if winged then
            if hasNodes then
                return "collectables/ghostberry/wings01"
            else
                return "collectables/strawberry/wings01"
            end
        else
            if hasNodes then
                return "collectables/ghostberry/idle00"
            else
                return "collectables/strawberry/normal00"
            end
        end
    end
end

function customBerry.nodeTexture(room, entity)
    local hasNodes = entity.nodes and #entity.nodes > 0
    local berryType = entity.berryType or "strawberry"

    if hasNodes then
        if berryType == "stawberry" then
            return "collectables/strawberry/seed00"
        end
    end
end

customBerry.placements = {
    {
        name = "strawberry",
        data = {
            berryType = "strawberry",
            winged = false,
            checkpointID = -1,
            order = -1,
            collectSound = "Original",
            customCollectSound = "",
            levelSet = "",
            maps = "",
            requires = ""
        },
    },
    {
        name = "strawberry_winged",
        data = {
            berryType = "strawberry",
            winged = true,
            checkpointID = -1,
            order = -1,
            collectSound = "Original",
            customCollectSound = "",
            levelSet = "",
            maps = "",
            requires = ""
        },
    },
    {
        name = "moonberry",
        data = {
            berryType = "moonberry",
            winged = false,
            checkpointID = -1,
            order = -1,
            collectSound = "Original",
            customCollectSound = "",
            levelSet = "",
            maps = "",
            requires = ""
        },
    },
    {
        name = "moonberry_winged",
        data = {
            berryType = "moonberry",
            winged = true,
            checkpointID = -1,
            order = -1,
            collectSound = "Original",
            customCollectSound = "",
            levelSet = "",
            maps = "",
            requires = ""
        },
    },
    {
        name = "voidstarberry",
        data = {
            berryType = "voidstarberry",
            winged = false,
            checkpointID = -1,
            order = -1,
            collectSound = "Custom",
            customCollectSound = "",
            levelSet = "Maggy/DESOLO_ZANTAS/19-Space",
            maps = "",
            requires = ""
        },
    },
    {
        name = "popstarberry",
        data = {
            berryType = "popstarberry",
            winged = false,
            checkpointID = -1,
            order = -1,
            collectSound = "Custom",
            customCollectSound = "",
            levelSet = "Maggy/DESOLO_ZANTAS/19-Space",
            maps = "",
            requires = ""
        },
    },
    {
        name = "pinkplatinumberry",
        data = {
            berryType = "pinkplatinumberry",
            winged = false,
            checkpointID = -1,
            order = -1,
            collectSound = "Custom",
            customCollectSound = "",
            levelSet = "",
            maps = "",
            requires = ""
        },
    }
}

return customBerry
