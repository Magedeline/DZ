local entity = {}

entity.name = "MaggyHelper/HealTrigger"
entity.depth = 0
entity.placements = {
    {
        name = "Heal Trigger",
        data = {
            width = 16,
            height = 16,
            healAmount = 1,
            fullHeal = false,
            removeAfterUse = true,
            onlyOnce = true
        }
    },
    {
        name = "Full Heal Trigger",
        data = {
            width = 16,
            height = 16,
            healAmount = 0,
            fullHeal = true,
            removeAfterUse = true,
            onlyOnce = true
        }
    }
}

entity.fieldInformation = {
    healAmount = {
        fieldType = "integer",
        minimumValue = 0
    }
}

entity.fieldOrder = {
    "x", "y", "width", "height",
    "healAmount", "fullHeal",
    "removeAfterUse", "onlyOnce"
}

return entity
