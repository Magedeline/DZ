local memoryTile = {}
memoryTile.name = "DZ/MemoryTile"
memoryTile.depth = 0
memoryTile.placements = {
    { name = "main", data = { tileId = 0, puzzleId = "memory_1", width = 16, height = 16 } }
}
memoryTile.fieldInformation = {
    tileId = { fieldType = "integer", minimumValue = 0 },
    puzzleId = { fieldType = "string" }
}
memoryTile.fieldOrder = { "x", "y", "width", "height", "tileId", "puzzleId" }
return memoryTile
