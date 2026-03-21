local darkMatterEnemy = {}

darkMatterEnemy.name = "MaggyHelper/DarkMatterEnemy"
darkMatterEnemy.depth = -10000
darkMatterEnemy.justification = {0.5, 1.0}
darkMatterEnemy.texture = "characters/darkmatter/idle00"

darkMatterEnemy.placements = {
    {
        name = "dark_matter_weak",
        data = {
            health = 8,
            minDamage = 2,
            maxDamage = 4,
            patrolRadius = 64
        }
    },
    {
        name = "dark_matter_normal",
        data = {
            health = 10,
            minDamage = 2,
            maxDamage = 5,
            patrolRadius = 64
        }
    },
    {
        name = "dark_matter_strong",
        data = {
            health = 15,
            minDamage = 4,
            maxDamage = 7,
            patrolRadius = 96
        }
    }
}

darkMatterEnemy.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 100
    },
    minDamage = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    maxDamage = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    patrolRadius = {
        fieldType = "number",
        minimumValue = 0,
        maximumValue = 200
    }
}

return darkMatterEnemy
