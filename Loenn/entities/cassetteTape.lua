local cassetteTape = {}

cassetteTape.name = "DesoloZantas/CassetteTape"
cassetteTape.depth = -100
cassetteTape.texture = "collectibles/desolozantas/cassettetape/idle00"
cassetteTape.placements = {
    {
        name = "cassette_tape",
        data = {
            tapeId = "tape_default",
            audioEvent = "",
            displayName = "Cassette Tape",
            description = "A mysterious cassette tape.",
            color = "FFA500",
            autoPlay = false,
            oneTimeUse = false,
            remixIndex = 0
        }
    },
    {
        name = "cassette_tape_auto",
        data = {
            tapeId = "tape_auto",
            audioEvent = "event:/music/yourtrack",
            displayName = "Auto-Play Tape",
            description = "Plays automatically when collected.",
            color = "FF6B9D",
            autoPlay = true,
            oneTimeUse = true,
            remixIndex = 1
        }
    }
}

cassetteTape.fieldInformation = {
    color = {
        fieldType = "color",
        allowXNAColors = true
    },
    remixIndex = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 17
    }
}

return cassetteTape
