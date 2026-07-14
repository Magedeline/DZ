local flameGeyser = {}

flameGeyser.name = "DZ/FlameGeyser"
flameGeyser.depth = 0
flameGeyser.texture = "objects/DZ/flame_geyser"

flameGeyser.placements = {
    name = "flame_geyser",
    data = {
        eruptInterval = 4,
        eruptDuration = 1,
        warningTime = 1,
        flameHeight = 200,
        damageRadius = 30
    }
}

return flameGeyser