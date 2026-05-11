local npcBase = {}

npcBase.name = "MaggyHelper/NPC_Base"
npcBase.depth = -8500
npcBase.texture = "characters/Maggy/DesoloZantas/Kglobal::Player/idle00"
npcBase.justification = {0.5, 1.0}

npcBase.placements = {
    {
        name = "npc_base",
        data = {
            x = 0,
            y = 0,
            npcName = "NPC"
        }
    }
}

npcBase.fieldInformation = {
    npcName = {
        fieldType = "string"
    }
}

return npcBase
