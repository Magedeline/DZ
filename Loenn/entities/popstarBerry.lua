local popstarBerry = {}

popstarBerry.name = "MaggyHelper/PopstarBerry"
popstarBerry.depth = -100
popstarBerry.texture = "collectables/popstarberry/spin/000"

popstarBerry.fieldInformation = {
    collectSound = {
        editable = false,
        options = {
            ["Original"]   = "Original",   -- strawberry_get + strawberry_blue_touch
            ["Elaborate"]  = "Elaborate",   -- strawberry_get + Maggy_DesoloZantas/game/general/strawberry_get
            ["Minimalist"] = "Minimalist",  -- strawberry_get only
            ["Custom"]     = "Custom"       -- strawberry_get + customCollectSound event path
        }
    },
    customCollectSound = {
        editable = true  -- visible/active only when collectSound == "Custom"
    }
}

popstarBerry.placements = {
    name = "PopstarBerry",
    data = {
        collectSound       = "Elaborate",
        customCollectSound = "",
        levelSet           = "Maggy/DESOLO_ZANTAS/19-Space",
        maps               = "",
        requires           = ""
    }
}

return popstarBerry
