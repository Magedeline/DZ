local npc_event = {}

npc_event.name = "DesoloZantas/NPC_Event"
npc_event.depth = 0
npc_event.justification = {0.5, 1.0}
npc_event.texture = "characters/player/idle00"

npc_event.placements = {
    {
        name = "default",
        data = {
            dialogKey = "ingeste_npc_event_default",
            flagName = "npc_event_triggered",
            spriteId = "player",
            eventId = "default_event"
        }
    },
    {
        name = "NPC07_Badeline",
        data = {
            dialogKey = "CH7_BADELINE_INTRO",
            flagName = "npc_07_badeline",
            spriteId = "badeline",
            eventId = "badeline_07_event"
        }
    }
}

npc_event.fieldInformation = {
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
            "player",
            "theo",
            "madeline",
            "badeline",
            "chara",
            "kirby",
            "ralsei",
            "toriel",
            "asriel",
            "oshiro",
            "magolor",
            -- New characters from the request
            "rick",
            "kine",
            "coo",
            "bandana_waddle_dee",
            "king_ddd",
            "meta_knight",
            "adeline",
            "clover",
            "melody",
            "batty",
            "emily",
            "cody",
            "odin",
            "charlo",
            "frisk",
            "susie_haltmann",
            "ness",
            "taranza",
            "gooey",
            "squeaker",
            "dark_meta_knight",
            "marx",
            "fran_zalea",
            "flamberge_zalea",
            "hynes_zalea",
            "kirby_classic"
        }
    },
    eventId = {
        fieldType = "string"
    }
}

return npc_event
