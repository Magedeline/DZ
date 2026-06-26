local heatZone = {}

heatZone.name = "DZ/HeatZone"
heatZone.depth = 0
heatZone.placements = {
    name = "heat_zone",
    data = {
        isActive = true,
        maxRadius = 150,
        expansionSpeed = 100,
        pushForce = 150,
        interval = 5
    }
}

return heatZone