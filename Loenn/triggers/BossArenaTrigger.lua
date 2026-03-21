local entity = {}

entity.name = "MaggyHelper/BossArenaTrigger"
entity.depth = 0
entity.placements = {
    {
        name = "Boss Arena Trigger",
        data = {
            width = 16,
            height = 16,
            bossName = "Boss",
            showHealthBar = true,
            createHealthUI = true,
            bossEntityType = ""
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
    "showHealthBar", "createHealthUI"
}

return entity
