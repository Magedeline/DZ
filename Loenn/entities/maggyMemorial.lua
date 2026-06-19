local maggyMemorial = {}

maggyMemorial.name = "DZ/MaggyMemorial"
maggyMemorial.depth = 100
maggyMemorial.texture = "scenery/memorial/memorial"
maggyMemorial.justification = {0.5, 1.0}

maggyMemorial.placements = {
    {
        name = "main",
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
