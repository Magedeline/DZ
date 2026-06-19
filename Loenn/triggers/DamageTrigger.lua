local entity = {}

entity.name = "DZ/DamageTrigger"
entity.depth = 0
entity.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            damage = 1,
            cooldown = 1.0,
            removeAfterHit = false
        }
    },
    {
        name = "Damage Trigger (One Time)",
        data = {
            width = 16,
            height = 16,
            damage = 1,
            cooldown = 0.0,
            removeAfterHit = true
        }
    }
}

entity.fieldInformation = {
    damage = {
        fieldType = "integer",
        minimumValue = 1
    },
    cooldown = {
        fieldType = "number",
        minimumValue = 0
    }
}

entity.fieldOrder = {
    "x", "y", "width", "height",
    "damage", "cooldown", "removeAfterHit"
}

return entity
