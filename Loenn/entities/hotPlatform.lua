local hotPlatform = {}

hotPlatform.name = "DZ/HotPlatform"
hotPlatform.depth = 0
hotPlatform.texture = "objects/DZ/hot_platform"

hotPlatform.placements = {
    name = "hot_platform",
    data = {
        heatRate = 20,
        coolRate = 10,
        maxHeat = 100
    }
}

return hotPlatform