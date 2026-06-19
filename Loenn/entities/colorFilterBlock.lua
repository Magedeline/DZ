local colorFilterBlock = {}
colorFilterBlock.name = "DZ/ColorFilterBlock"
colorFilterBlock.depth = 0
colorFilterBlock.placements = {
    { name = "main", data = { width = 16, height = 16, color = "ff0000" } },
    { name = "blue", data = { width = 16, height = 16, color = "0000ff" } },
    { name = "green", data = { width = 16, height = 16, color = "00ff00" } }
}
colorFilterBlock.fieldInformation = {
    color = { fieldType = "color" }
}
colorFilterBlock.fieldOrder = { "x", "y", "width", "height", "color" }
return colorFilterBlock
