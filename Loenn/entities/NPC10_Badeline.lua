local npc10Badeline = {}

npc10Badeline.name = "MaggyHelper/NPC10_Badeline"
npc10Badeline.depth = 0
npc10Badeline.justification = {0.5, 1.0}
npc10Badeline.texture = "characters/badeline/idle00"

npc10Badeline.placements = {
    {
        name = "NPC10_Badeline",
        data = {
            dialogKey = "CH2_BADELINE_GRIEFA",
            flagName = "badeline_met"
        }
    }
}

npc10Badeline.fieldInformation = {
    dialogKey = {
        fieldType = "string"
    },
    flagName = {
        fieldType = "string"
    }
}

return npc10Badeline
