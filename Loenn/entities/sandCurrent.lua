local sandCurrent = {}
sandCurrent.name = "MaggyHelper/SandCurrent"
sandCurrent.depth = -50
sandCurrent.placements = {
    { name = "right", data = { width = 64, height = 64, directionX = 1.0, directionY = 0.0, strength = 80.0 } },
    { name = "left", data = { width = 64, height = 64, directionX = -1.0, directionY = 0.0, strength = 80.0 } },
    { name = "up", data = { width = 64, height = 64, directionX = 0.0, directionY = -1.0, strength = 80.0 } }
}
sandCurrent.fieldInformation = {
    directionX = { fieldType = "number" },
    directionY = { fieldType = "number" },
    strength = { fieldType = "number", minimumValue = 10.0 }
}
sandCurrent.fieldOrder = { "x", "y", "width", "height", "directionX", "directionY", "strength" }
return sandCurrent
