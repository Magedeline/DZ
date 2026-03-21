local dialogNPC = {}

dialogNPC.name = "MaggyHelper/DialogNPC"
dialogNPC.depth = -8500
dialogNPC.texture = "characters/player/idle00"
dialogNPC.justification = {0.5, 1.0}

dialogNPC.placements = {
    {
        name = "dialog_npc",
        data = {
            x = 0,
            y = 0,
            dialogId = "DIALOG_KEY",
            npcName = "NPC",
            onlyOnce = false
        }
    }
}

dialogNPC.fieldInformation = {
    dialogId = {
        fieldType = "string"
    },
    npcName = {
        fieldType = "string"
    },
    onlyOnce = {
        fieldType = "boolean"
    }
}

return dialogNPC