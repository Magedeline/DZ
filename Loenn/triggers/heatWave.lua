local heatWave = {}

heatWave.name = "DZ/HeatWave"
heatWave.depth = 0
heatWave.placements = {
    name = "heat_wave",
    data = {
        isActive = true,
        maxRadius = 150,
        expansionSpeed = 100,
        pushForce = 150,
        interval = 5
    }
}

return heatWave