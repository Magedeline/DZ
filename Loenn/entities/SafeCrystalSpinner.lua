local safeCrystalSpinner = {}

safeCrystalSpinner.name = "MaggyHelper/SafeCrystalSpinner"
safeCrystalSpinner.depth = -8500
safeCrystalSpinner.placements = {
    {
        name = "blue",
        data = {
            attachToSolid = false,
            color = "Blue"
        }
    },
    {
        name = "red",
        data = {
            attachToSolid = false,
            color = "Red"
        }
    },
    {
        name = "purple",
        data = {
            attachToSolid = false,
            color = "Purple"
        }
    },
    {
        name = "rainbow",
        data = {
            attachToSolid = false,
            color = "Rainbow"
        }
    }
}

safeCrystalSpinner.fieldInformation = {
    color = {
        options = {"Blue", "Red", "Purple", "Rainbow"},
        editable = false
    }
}

local colorToTexture = {
    Blue = "Maggy/DesoloZantas/danger/crystal/fg_blue00",
    Red = "Maggy/DesoloZantas/danger/crystal/fg_red00",
    Purple = "Maggy/DesoloZantas/danger/crystal/fg_purple00",
    Rainbow = "Maggy/DesoloZantas/danger/crystal/fg_rainbow00"
}

function safeCrystalSpinner.texture(room, entity)
    local color = entity.color or "Blue"
    return colorToTexture[color] or colorToTexture.Blue
end

return safeCrystalSpinner

