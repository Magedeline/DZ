local soundPuzzleBlock = {}
soundPuzzleBlock.name = "DZ/SoundPuzzleBlock"
soundPuzzleBlock.depth = 0
soundPuzzleBlock.placements = {
    { name = "SoundPuzzleBlock", data = { noteIndex = 0, puzzleId = "puzzle_1", width = 16, height = 16 } }
}
soundPuzzleBlock.fieldInformation = {
    noteIndex = { fieldType = "integer", minimumValue = 0, maximumValue = 7 },
    puzzleId = { fieldType = "string" }
}
soundPuzzleBlock.fieldOrder = { "x", "y", "width", "height", "noteIndex", "puzzleId" }
return soundPuzzleBlock
