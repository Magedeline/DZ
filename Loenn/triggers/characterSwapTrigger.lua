local characterSwapTrigger = {}
characterSwapTrigger.name = "DZ/CharacterSwapTrigger"
characterSwapTrigger.placements = {
    { name = "CharacterSwapTrigger", data = { width = 16, height = 16, targetCharacter = "Kirby", onlyOnce = false } }
}
characterSwapTrigger.fieldInformation = {
    targetCharacter = { fieldType = "string", options = { "Madeline", "Kirby", "Badeline" }, editable = true },
    onlyOnce = { fieldType = "boolean" }
}
characterSwapTrigger.fieldOrder = { "x", "y", "width", "height", "targetCharacter", "onlyOnce" }
return characterSwapTrigger
