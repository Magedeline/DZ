local elementalPillar = {}
elementalPillar.name = "DZ/ElementalPillar"
elementalPillar.depth = -100
elementalPillar.placements = {
    { name = "main", data = { element = "Fire", puzzleId = "elem_1" } },
    { name = "ice", data = { element = "Ice", puzzleId = "elem_1" } },
    { name = "electric", data = { element = "Electric", puzzleId = "elem_1" } },
    { name = "wind", data = { element = "Wind", puzzleId = "elem_1" } }
}
elementalPillar.fieldInformation = {
    element = { fieldType = "string", options = { "Fire", "Ice", "Electric", "Wind" }, editable = false },
    puzzleId = { fieldType = "string" }
}
elementalPillar.fieldOrder = { "x", "y", "element", "puzzleId" }
return elementalPillar
