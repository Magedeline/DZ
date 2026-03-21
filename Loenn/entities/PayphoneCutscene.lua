local payphone_cutscene = {}

payphone_cutscene.name = "DesoloZantas/PayphoneCutscene"
payphone_cutscene.depth = 0
payphone_cutscene.justification = {0.5, 0.5}
payphone_cutscene.texture = "objects/payphone/payphone00"
payphone_cutscene.nodeLimits = {0, 0}

payphone_cutscene.placements = {
    {
        name = "Payphone Dream Cutscene",
        data = {
            cutsceneType = "dream"
        }
    },
    {
        name = "Payphone Awake Cutscene",
        data = {
            cutsceneType = "awake"
        }
    }
}

payphone_cutscene.fieldInformation = {
    cutsceneType = {
        fieldType = "string",
        options = {
            "dream",
            "awake"
        }
    }
}

return payphone_cutscene