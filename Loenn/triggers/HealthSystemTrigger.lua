local entity = {}

entity.name = "DZ/HealthSystemTrigger"
entity.depth = 0
entity.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            maxHP = 6,
            kirbyMode = false,
            showUI = true,
            persistent = true,
            displayMode = 0,
            trackBosses = true,
            healOnEnter = false,
            healAmount = 0
        }
    },
    {
        name = "Health System Trigger (Kirby Mode)",
        data = {
            width = 16,
            height = 16,
            maxHP = 6,
            kirbyMode = true,
            showUI = true,
            persistent = true,
            displayMode = 0,
            trackBosses = true,
            healOnEnter = false,
            healAmount = 0
        }
    }
}

entity.fieldInformation = {
    maxHP = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 20
    },
    displayMode = {
        fieldType = "integer",
        options = {
            {"Hearts", 0},
            {"Bar", 1},
            {"Numeric", 2},
            {"Hearts and Bar", 3}
        },
        editable = false
    },
    healAmount = {
        fieldType = "integer",
        minimumValue = 0
    }
}

entity.fieldOrder = {
    "x", "y", "width", "height",
    "maxHP", "kirbyMode", "displayMode",
    "showUI", "trackBosses", "persistent",
    "healOnEnter", "healAmount"
}

return entity
