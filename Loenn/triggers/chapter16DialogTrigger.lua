local chapter16DialogTrigger = {}

chapter16DialogTrigger.name = "DZ/Chapter16DialogTrigger"
chapter16DialogTrigger.depth = 0
chapter16DialogTrigger.placements = {
    name = "chapter16_dialog_trigger",
    data = {
        dialogKey = "DZ_CH16_DEFAULT",
        characterState = "madeline_default",
        enableCharacterStates = true,
        enablePortraitChanges = true,
        enableEffects = true,
        triggerOnce = true,
        playerOnly = true,
        autoStart = false,
        madnessLevel = 3,
        enableTentacles = false
    }
}

return chapter16DialogTrigger
