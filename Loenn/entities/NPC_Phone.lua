local npc_phone = {}

npc_phone.name = "DesoloZantas/NPC_Phone"
npc_phone.depth = 0
npc_phone.justification = {0.5, 1.0}
npc_phone.texture = "objects/payphone/payphone00"

npc_phone.placements = {
    {
        name = "Phone - Mom Call",
        data = {
            phoneType = "mom",
            dialogKey = "KIRBY_PAYPHONE_AWAKE_END"
        }
    },
    {
        name = "Phone - Ex Call",
        data = {
            phoneType = "ex", 
            dialogKey = "KIRBY_PAYPHONE_DREAM_END"
        }
    }
}

npc_phone.fieldInformation = {
    phoneType = {
        fieldType = "string",
        options = {
            "mom",
            "ex"
        }
    },
    dialogKey = {
        fieldType = "string",
        options = {
            "KIRBY_PAYPHONE_AWAKE_END",
            "KIRBY_PAYPHONE_DREAM_END"
        }
    }
}

return npc_phone