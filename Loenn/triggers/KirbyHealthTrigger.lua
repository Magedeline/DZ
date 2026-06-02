local KirbyHealthTrigger = {}

KirbyHealthTrigger.name = "MaggyHelper/KirbyHealthTrigger"
KirbyHealthTrigger.depth = 0
KirbyHealthTrigger.placements = {
    name = "default",
    data = {
        width = 16,
        height = 16,
        enableHealth = true,
        maxHealth = 6,
        healAmount = 0,
        fullHeal = false,
        setRespawnPoint = false,
        onlyOnce = true
    }
}

KirbyHealthTrigger.fieldInformation = {
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 20
    },
    healAmount = {
        fieldType = "integer",
        minimumValue = 0
    }
}

KirbyHealthTrigger.nodeLimits = {0, 0}

return KirbyHealthTrigger
