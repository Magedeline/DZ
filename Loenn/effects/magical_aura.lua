local magicalAura = {}

magicalAura.name = "MaggyHelper/MagicalAura"
magicalAura.canForeground = false
magicalAura.canBackground = true

magicalAura.defaultData = {
    color = "purple",
    intensity = 1.0,
    speed = 1.0,
    particleCount = 50
}

magicalAura.fieldInformation = {
    color = {
        options = {"purple", "blue", "green", "red", "gold", "rainbow"},
        editable = false
    },
    intensity = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 3.0
    },
    speed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },
    particleCount = {
        fieldType = "integer",
        minimumValue = 10,
        maximumValue = 200
    }
}

return magicalAura
