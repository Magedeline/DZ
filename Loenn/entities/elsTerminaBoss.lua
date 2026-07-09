local elsTerminaBoss = {}

elsTerminaBoss.name = "DZ/ELSTerminaBoss"
elsTerminaBoss.depth = 0
elsTerminaBoss.texture = "objects/DZ/DZ/ghostbuster/idle"

elsTerminaBoss.placements = {
    name = "els_termina_boss",
    data = {
        phase = 4,
        fromCutscene = false,
        hardMode = false
    }
}

elsTerminaBoss.fieldInformation = {
    phase = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 4
    }
}

return elsTerminaBoss
