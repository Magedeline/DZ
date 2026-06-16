-- DZ/KirbyDreamBlock
-- A dream block that both Kirby (hover) and normal Madeline (dream dash) can pass through.
-- Kirby does not need the dream-dash inventory item.

return {
    name = "DZ/KirbyDreamBlock",
    depth = function(room, entity)
        return entity.below and 5000 or -11000
    end,
    -- Pink-tinted fill to visually distinguish from vanilla dream blocks.
    fillColor   = {1.0, 0.41, 0.71, 0.35},
    borderColor = {1.0, 0.08, 0.58, 1.0},
    fieldInformation = {
        oneUse = {
            fieldType = "boolean"
        },
        fastMoving = {
            fieldType = "boolean"
        },
        below = {
            fieldType = "boolean"
        },
    },
    fieldOrder = {
        "x", "y", "width", "height",
        "oneUse", "fastMoving", "below"
    },
    placements = {
        {
            name = "Kirby Dream Block",
            data = {
                width      = 16,
                height     = 16,
                oneUse     = false,
                fastMoving = false,
                below      = false,
    }
        }
    }
}
