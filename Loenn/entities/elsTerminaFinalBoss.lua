local elsTerminaFinalBoss = {}

elsTerminaFinalBoss.name = "DZ/ELSTerminaFinalBoss"
elsTerminaFinalBoss.depth = 0
elsTerminaFinalBoss.texture = "objects/DZ/DZ/ghostbuster/idle"

elsTerminaFinalBoss.placements = {
    name = "els_termina_final_boss",
    data = {
        difficultyMode = 0,
        fromCutscene = false,
        hasFiveHeartGems = false
    }
}

elsTerminaFinalBoss.fieldInformation = {
    difficultyMode = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 2
    }
}

return elsTerminaFinalBoss
