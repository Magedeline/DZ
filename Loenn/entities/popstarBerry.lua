local popstarBerry = {}

popstarBerry.name = "MaggyHelper/PopstarBerry"
popstarBerry.depth = -100
popstarBerry.texture = "collectables/maggy/popstarberry/spin/000"

popstarBerry.fieldInformation = {
    collectSound = {
        editable = false,
        options = {
            ["Original"] = "Original",
            ["Elaborate"] = "Elaborate", 
            ["Minimalist"] = "Minimalist",
            ["Custom"] = "Custom"
        }
    }
}

popstarBerry.placements = {
    name = "PopstarBerry",
    data = {
        collectSound = "Original",
        customCollectSound = "",
        levelSet = "Maggy/Main/ZFinalChapter",
        maps = "",
        requires = ""
    }
}

return popstarBerry