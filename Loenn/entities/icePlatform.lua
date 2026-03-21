local icePlatform = {}

icePlatform.name = "MaggyHelper/IcePlatform"
icePlatform.depth = -10
icePlatform.fillColor = {0.7, 0.9, 1.0, 0.4}
icePlatform.borderColor = {0.8, 1.0, 1.0, 0.7}
icePlatform.placements = {
    {
        name = "IcePlatform",
        data = { width = 32, friction = 0.98, canMelt = true }
    },
    {
        name = "permanent",
        data = { width = 32, friction = 0.98, canMelt = false }
    },
    {
        name = "super_slippery",
        data = { width = 32, friction = 0.995, canMelt = true }
    }
}
icePlatform.fieldInformation = {
    friction = { fieldType = "number", minimumValue = 0.9, maximumValue = 1.0 },
    canMelt = { fieldType = "boolean" }
}
icePlatform.fieldOrder = { "x", "y", "width", "friction", "canMelt" }

return icePlatform
