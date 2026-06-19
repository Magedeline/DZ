local paintCanvas = {}
paintCanvas.name = "DZ/PaintCanvas"
paintCanvas.depth = 5000
paintCanvas.placements = {
    { name = "main", data = { width = 64, height = 64, defaultColor = "ffffff", flag = "" } }
}
paintCanvas.fieldInformation = {
    defaultColor = { fieldType = "color" },
    flag = { fieldType = "string" }
}
paintCanvas.fieldOrder = { "x", "y", "width", "height", "defaultColor", "flag" }
return paintCanvas
