local windTunnel = {}

windTunnel.name = "DZ/WindTunnel"
windTunnel.depth = -500
windTunnel.fillColor = {0.8, 0.9, 1.0, 0.15}
windTunnel.borderColor = {0.8, 0.9, 1.0, 0.4}
windTunnel.placements = {
    {
        name = "main",
        data = { width = 32, height = 64, direction = "Up", strength = 200.0, affectsKirbyMore = true }
    },
    {
        name = "down",
        data = { width = 32, height = 64, direction = "Down", strength = 200.0, affectsKirbyMore = true }
    },
    {
        name = "left",
        data = { width = 64, height = 32, direction = "Left", strength = 200.0, affectsKirbyMore = true }
    },
    {
        name = "right",
        data = { width = 64, height = 32, direction = "Right", strength = 200.0, affectsKirbyMore = true }
    }
}
windTunnel.fieldInformation = {
    direction = { options = { "Up", "Down", "Left", "Right" }, editable = false },
    strength = { fieldType = "number", minimumValue = 0.0 },
    affectsKirbyMore = { fieldType = "boolean" }
}
windTunnel.fieldOrder = { "x", "y", "width", "height", "direction", "strength", "affectsKirbyMore" }

return windTunnel
