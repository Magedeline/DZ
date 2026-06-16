local ancientRunes = {}

ancientRunes.name = "DZ/AncientRunes"
ancientRunes.canForeground = true
ancientRunes.canBackground = true

ancientRunes.defaultData = {
    runeType = "standard",
    glowIntensity = 0.8,
    animationSpeed = 1.0,
    fadeDistance = 200.0
}

ancientRunes.fieldInformation = {
    runeType = {
        options = {"standard", "power", "wisdom", "courage", "time"},
        editable = false
    },
    glowIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0
    },
    animationSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 3.0
    },
    fadeDistance = {
        fieldType = "number",
        minimumValue = 50.0,
        maximumValue = 500.0
    }
}

return ancientRunes
