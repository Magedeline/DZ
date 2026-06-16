local npc = {
    name = "DesoloZantas/NPC",
    depth = 1000,
    texture = "characters/theo/idle00",
    fieldInformation = {
        spriteId = {
            fieldType = "string",
            options = {
                "theo",
                "chara",
                "kirby",
                "ralsei",
                "madeline",
                "badeline",
                "maggy",
                "magolor",
                "toriel",
                "asriel",
                "oshiro",
                "granny",
                "metaknight",
                "roxus",
                "temmie",
                "axis",
                "els",
                "digitalguide",
                "phone",
                "titancouncil"
            },
            editable = false
        },
        dialogKey = {
            fieldType = "string"
        },
        flagName = {
            fieldType = "string"
        },
        eventId = {
            fieldType = "string"
        }
    },
    fieldOrder = {
        "x", "y",
        "spriteId",
        "dialogKey",
        "flagName",
        "eventId"
    },
    placements = {
        {
            name = "NPC (Theo)",
            data = {
                spriteId = "theo",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Chara)",
            data = {
                spriteId = "chara",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Kirby)",
            data = {
                spriteId = "kirby",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Ralsei)",
            data = {
                spriteId = "ralsei",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Madeline)",
            data = {
                spriteId = "madeline",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Badeline)",
            data = {
                spriteId = "badeline",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Maggy)",
            data = {
                spriteId = "maggy",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Magolor)",
            data = {
                spriteId = "magolor",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Toriel)",
            data = {
                spriteId = "toriel",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Asriel)",
            data = {
                spriteId = "asriel",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Oshiro)",
            data = {
                spriteId = "oshiro",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Granny)",
            data = {
                spriteId = "granny",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Meta Knight)",
            data = {
                spriteId = "metaknight",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Roxus)",
            data = {
                spriteId = "roxus",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Temmie)",
            data = {
                spriteId = "temmie",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Axis)",
            data = {
                spriteId = "axis",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Els)",
            data = {
                spriteId = "els",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Digital Guide)",
            data = {
                spriteId = "digitalguide",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Phone)",
            data = {
                spriteId = "phone",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        },
        {
            name = "NPC (Titan Council)",
            data = {
                spriteId = "titancouncil",
                dialogKey = "",
                flagName = "",
                eventId = ""
            }
        }
    }
}

npc.npcClasses = {
    -- Generic NPCs
    "Npc_Theo",
    "Npc_Chara",
    "Npc_Kirby",
    "Npc_Ralsei",
    "Npc_MetaKnight",
    "Npc_DigitalGuide",
    "Npc_Phone",
    "Npc_Roxus",
    "Npc_Temmie",
    "Npc_Axis",
    "Npc_Els",
    "Npc_TitanCouncilMember",

    -- Chapter-specific NPCs
    "Npc00_Theo",
    "Npc01_Maggy",
    "Npc02_Maggy",
    "Npc03_Maggy",
    "Npc03_Theo",
    "Npc05_Magolor_Vents",
    "Npc05_Magolor_Escape",
    "Npc05_Oshiro_Breakdown",
    "Npc05_Oshiro_Clutter",
    "Npc05_Oshiro_Hallway1",
    "Npc05_Oshiro_Hallway2",
    "Npc05_Oshiro_Lobby",
    "Npc05_Oshiro_Rooftop",
    "Npc05_Oshiro_Suite",
    "Npc06_Magolor",
    "Npc06_Theo",
    "Npc07_Chara",
    "Npc07_Maddy_Mirror",
    "Npc08_Chara_Crying",
    "Npc08_Maddy_and_Theo_Ending",
    "Npc08_Madeline_Plateau",
    "Npc08_Maggy_Ending",
    "Npc08_Theo_Ending",
    "Npc17_Kirby",
    "Npc17_Oshiro",
    "Npc17_Ralsei",
    "Npc17_Theo",
    "Npc17_Toriel",
    "Npc18_Toriel_Inside",
    "Npc18_Toriel_Outside",
    "Npc19_Gravestone",
    "Npc19_Maggy_Loop",
    "Npc20_Asriel",
    "Npc20_Granny",
    "Npc20_Madeline",

    -- Special
    "NPCEventInteract"
}

return npc
