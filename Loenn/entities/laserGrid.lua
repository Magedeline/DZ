local laserGrid = {}
laserGrid.name = "DZ/LaserGrid"
laserGrid.depth = -50
laserGrid.placements = {
    { name = "main", data = { width = 8, height = 64, onTime = 2.0, offTime = 2.0, startOn = true, color = "ff0000" } },
    { name = "lazpewpewalternating", data = { width = 8, height = 64, onTime = 1.5, offTime = 1.5, startOn = false, color = "00ff00" } }
}
laserGrid.fieldInformation = {
    onTime = { fieldType = "number", minimumValue = 0.1 },
    offTime = { fieldType = "number", minimumValue = 0.1 },
    startOn = { fieldType = "boolean" },
    color = { fieldType = "color" }
}
laserGrid.fieldOrder = { "x", "y", "width", "height", "onTime", "offTime", "startOn", "color" }
return laserGrid
