local npc05_oshiro_lobby = {}

npc05_oshiro_lobby.name = "MaggyHelper/NPC05_Oshiro_Lobby"
npc05_oshiro_lobby.depth = 0
npc05_oshiro_lobby.justification = {0.5, 1.0}
npc05_oshiro_lobby.texture = "characters/Maggy/DesoloZantas/oshiro/oshiro25"

npc05_oshiro_lobby.placements = {
    {
        name = "NPC05_Oshiro_Lobby",
        data = {
            dialogKey = "ingeste_oshiro_05_lobby",
            flagName = "oshiro_05_lobby",
            spriteId = "oshiro"
        }
    }
}

npc05_oshiro_lobby.fieldInformation = {
    dialogKey = {
        fieldType = "string"
    },
    flagName = {
        fieldType = "string"
    },
    spriteId = {
        fieldType = "string",
        editable = true,
        options = {
            "oshiro",
            "theo",
            "Kglobal::Player"
        }
    }
}

return npc05_oshiro_lobby

