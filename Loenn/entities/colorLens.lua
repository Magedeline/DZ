local colorLens = {}
colorLens.name = "DZ/ColorLens"
colorLens.depth = -50
colorLens.placements = {
    { name = "main", data = { color = "ff0000", width = 16, height = 16 } },
    { name = "blue", data = { color = "0000ff", width = 16, height = 16 } },
    { name = "green", data = { color = "00ff00", width = 16, height = 16 } }
}
colorLens.fieldInformation = {
    color = { fieldType = "color" }
}
colorLens.fieldOrder = { "x", "y", "width", "height", "color" }
return colorLens
