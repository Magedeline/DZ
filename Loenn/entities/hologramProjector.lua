local hologramProjector = {}
hologramProjector.name = "DZ/HologramProjector"
hologramProjector.depth = -100
hologramProjector.placements = {
    { name = "HologramProjector", data = { message = "Hello!", displayTime = 5.0, flag = "" } }
}
hologramProjector.fieldInformation = {
    message = { fieldType = "string" },
    displayTime = { fieldType = "number", minimumValue = 1.0 },
    flag = { fieldType = "string" }
}
hologramProjector.fieldOrder = { "x", "y", "message", "displayTime", "flag" }
return hologramProjector
