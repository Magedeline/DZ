local npc06_theo = {}

npc06_theo.name = "MaggyHelper/NPC06_Theo"
npc06_theo.depth = 0
npc06_theo.justification = {0.5, 1.0}
npc06_theo.texture = "characters/Maggy/DesoloZantas/theo/theo00"

npc06_theo.placements = {
    {
        name = "NPC06_Theo",
        data = {
            conversationStage = 1
        }
    }
}

npc06_theo.fieldInformation = {
    conversationStage = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 3,
        editable = true
    }
}

return npc06_theo
