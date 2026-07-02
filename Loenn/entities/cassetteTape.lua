local cassetteTape = {}

cassetteTape.name = "DZ/CassetteTape"
cassetteTape.depth = 0
cassetteTape.texture = "objects/DZ/DZ/DZ/cassette_tape"

cassetteTape.placements = {
    name = "cassette_tape",
    data = {
        tapeId = "tape_default",
        audioEvent = "",
        displayName = "Cassette Tape",
        description = "A mysterious cassette tape.",
        autoPlay = false,
        oneTimeUse = false,
        remixIndex = 0
    }
}

return cassetteTape