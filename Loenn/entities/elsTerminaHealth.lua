local elsTerminaHealth = {}

elsTerminaHealth.name = "DZ/ELSTerminaHealth"
elsTerminaHealth.depth = -100000
elsTerminaHealth.texture = "heartgem/0/0"

elsTerminaHealth.placements = {
    name = "els_termina_health",
    data = {
        maxHealth = 300.0,
        hardMode = false
    }
}

elsTerminaHealth.fieldInformation = {
    maxHealth = {
        minimumValue = 1.0
    }
}

return elsTerminaHealth
