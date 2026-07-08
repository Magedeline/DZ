local waterCurrentZone = {}

waterCurrentZone.name = "DZ/WaterCurrentZone"
waterCurrentZone.depth = 0
waterCurrentZone.placements = {
    name = "water_current_zone",
    data = {
        flowStrength = 80,
        rushInterval = 5,
        rushDuration = 2,
        width = 16,
        height = 16
    }
}

return waterCurrentZone