local maggyMemorial = {}

maggyMemorial.name = "MaggyHelper/MaggyMemorial"
maggyMemorial.depth = 100
maggyMemorial.texture = "scenery/memorial/memorial"
maggyMemorial.justification = {0.5, 1.0}

maggyMemorial.placements = {
    {
        name = "normal",
        data = {
            dialogKey = "MAGGY_MEMORIAL_DEFAULT",
            spritePath = "scenery/memorial/memorial",
            dreamy = false
        }
    },
    {
        name = "dreamy",
        data = {
            dialogKey = "MAGGY_MEMORIAL_DEFAULT",
            spritePath = "scenery/memorial/memorial",
            dreamy = true
        }
    }
}

maggyMemorial.fieldInformation = {
    dialogKey = {
        fieldType = "string"
    },
    spritePath = {
        fieldType = "string"
    },
    dreamy = {
        fieldType = "boolean"
    }
}

maggyMemorial.fieldOrder = {
    "x", "y", "dialogKey", "spritePath", "dreamy"
}

return maggyMemorial
