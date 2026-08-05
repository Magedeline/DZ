local heartGem = {}

heartGem.name = "DZ/DZHeartGem"
heartGem.depth = 0
heartGem.texture = "objects/DZ/heart_gem"

heartGem.placements = {
    name = "heart_gem",
    data = {
        fake = false,
        color = "Blue",
        removeCameraTriggers = false
    }
}

heartGem.fieldInformation = {
    color = {
        editable = false,
        options = {
            "Blue",
            "Red",
            "Gold",
            "Orange",
            "Purple",
            "Pink",
            "White"
        }
    }
}

return heartGem