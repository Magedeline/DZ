local entity = {}

entity.name = "DZ/BossArenaTrigger"
entity.depth = 0
entity.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            bossName = "Boss",
            showHealthBar = true,
            createHealthUI = true,
            bossEntityType = "",
            startEncounter = false,
            triggerOnce = false
        }
    },
    {
        name = "Boss Actor Encounter",
        data = {
            width = 16,
            height = 16,
            bossName = "Boss",
            showHealthBar = true,
            createHealthUI = true,
            bossEntityType = "",
            startEncounter = true,
            triggerOnce = true
        }
    }
}

entity.fieldInformation = {
    bossName = {
        fieldType = "string"
    },
    bossEntityType = {
        fieldType = "string"
    }
}

entity.fieldOrder = {
    "x", "y", "width", "height",
    "bossName", "bossEntityType",
    "showHealthBar", "createHealthUI",
    "startEncounter", "triggerOnce"
}

return entity
