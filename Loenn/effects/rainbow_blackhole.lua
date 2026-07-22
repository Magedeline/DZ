local blackHole = {}

blackHole.name = "DZ/RainbowBlackholeBG"
blackHole.depth = 10000
blackHole.texture = "objects/DZ/temple/portal/portal"

blackHole.fieldInformation = {
    strength = {
        options = { "Mild", "Medium", "High", "Wild", "Insane" },
        editable = false
    }
}

blackHole.placements = {
    name = "rainbow_blackhole",
    data = {
        alpha = 1.0,
        scale = 1.0,
        direction = 1.0,
        strength = "Mild",
        rainbowMode = false,
        centerOffsetX = 0.0,
        centerOffsetY = 0.0,
        offsetOffsetX = 0.0,
        offsetOffsetY = 0.0
    }
}

return blackHole
