local dualCharacterTrigger = {}
dualCharacterTrigger.name = "MaggyHelper/DualCharacterTrigger"
dualCharacterTrigger.placements = {
    { name = "DualCharacterTrigger", data = { width = 16, height = 16, secondCharacter = "Kirby", swapKey = "Tab" } }
}
dualCharacterTrigger.fieldInformation = {
    secondCharacter = { fieldType = "string", options = { "Madeline", "Kirby", "Badeline" }, editable = true },
    swapKey = { fieldType = "string" }
}
dualCharacterTrigger.fieldOrder = { "x", "y", "width", "height", "secondCharacter", "swapKey" }
return dualCharacterTrigger
