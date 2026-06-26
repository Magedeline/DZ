local chapter16CutsceneTrigger = {}

chapter16CutsceneTrigger.name = "DZ/Chapter16CutsceneTrigger"
chapter16CutsceneTrigger.depth = 0
chapter16CutsceneTrigger.placements = {
    name = "chapter16_cutscene_trigger",
    data = {
        cutsceneId = "ch16_default",
        dialogKey = "DZ_CH16_DEFAULT",
        triggerOnce = true,
        playerOnly = true,
        autoStart = false,
        enableEffects = true,
        enableTentacles = true,
        madnessLevel = 3,
        corruptionLevel = 5
    }
}

return chapter16CutsceneTrigger