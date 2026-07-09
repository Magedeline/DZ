local asrielAngelBossIntroTrigger = {}

asrielAngelBossIntroTrigger.name = "DZ/AsrielAngelBossIntroTrigger"
asrielAngelBossIntroTrigger.depth = 0
asrielAngelBossIntroTrigger.placements = {
    name = "asriel_angel_boss_intro_trigger",
    data = {
        width = 8,
        height = 8,
        triggerOnce = true,
        requireFlag = "",
        requireNotFlag = "asriel_angel_boss_intro",
        dialogKey = "ch20_asriel_angel_boss_intro",
        shakeIntensity = 1.0,
        zoomDuration = 0.6
    }
}

asrielAngelBossIntroTrigger.fieldInformation = {
    shakeIntensity = {
        minimumValue = 0.0
    },
    zoomDuration = {
        minimumValue = 0.0
    }
}

return asrielAngelBossIntroTrigger
