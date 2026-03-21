local gravityZoneTrigger = {}
gravityZoneTrigger.name = "MaggyHelper/GravityZoneTrigger"
gravityZoneTrigger.placements = {
    { name = "GravityZoneTrigger", data = { width = 32, height = 32, gravityDirection = "Up", gravityStrength = 1.0 } },
    { name = "low_gravity", data = { width = 32, height = 32, gravityDirection = "Down", gravityStrength = 0.3 } },
    { name = "zero_gravity", data = { width = 32, height = 32, gravityDirection = "None", gravityStrength = 0.0 } }
}
gravityZoneTrigger.fieldInformation = {
    gravityDirection = { fieldType = "string", options = { "Up", "Down", "Left", "Right", "None" }, editable = false },
    gravityStrength = { fieldType = "number", minimumValue = 0.0 }
}
gravityZoneTrigger.fieldOrder = { "x", "y", "width", "height", "gravityDirection", "gravityStrength" }
return gravityZoneTrigger
