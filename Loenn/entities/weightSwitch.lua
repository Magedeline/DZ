local weightSwitch = {}
weightSwitch.name = "DZ/WeightSwitch"
weightSwitch.depth = 0
weightSwitch.placements = {
    { name = "WeightSwitch", data = { width = 16, requiredWeight = 1.0, flag = "weight_switch", persistent = false } }
}
weightSwitch.fieldInformation = {
    requiredWeight = { fieldType = "number", minimumValue = 0.1 },
    flag = { fieldType = "string" },
    persistent = { fieldType = "boolean" }
}
weightSwitch.fieldOrder = { "x", "y", "width", "requiredWeight", "flag", "persistent" }
return weightSwitch
