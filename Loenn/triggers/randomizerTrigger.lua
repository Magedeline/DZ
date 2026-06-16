local randomizerTrigger = {}
randomizerTrigger.name = "DZ/RandomizerTrigger"
randomizerTrigger.placements = {
    { name = "RandomizerTrigger", data = { width = 16, height = 16, randomizeAbilities = true, randomizeGravity = false, randomizeSpeed = false, seed = 0 } }
}
randomizerTrigger.fieldInformation = {
    randomizeAbilities = { fieldType = "boolean" },
    randomizeGravity = { fieldType = "boolean" },
    randomizeSpeed = { fieldType = "boolean" },
    seed = { fieldType = "integer", minimumValue = 0 }
}
randomizerTrigger.fieldOrder = { "x", "y", "width", "height", "randomizeAbilities", "randomizeGravity", "randomizeSpeed", "seed" }
return randomizerTrigger
