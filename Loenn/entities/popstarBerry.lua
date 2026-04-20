local popstarBerry = {}

popstarBerry.name = "MaggyHelper/PopstarBerry"
popstarBerry.depth = -100
popstarBerry.texture = "collectables/maggy/popstarberry/spin/000"

popstarBerry.fieldInformation = {
    collectSound = {
        editable = false,
        options = {
            ["Original"]   = "Original",   -- strawberry_get + strawberry_blue_touch
            ["Elaborate"]  = "Elaborate",   -- strawberry_get + desolozantas/game/general/strawberry_get
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
        levelSet           = "Maggy/ASide/19_Space",
        maps               = "",
        requires           = ""
    }
}

return popstarBerry