local oshiro_hotel_cutscene = {}

oshiro_hotel_cutscene.name = "DesoloZantas/OshiroHotelCutscene"
oshiro_hotel_cutscene.depth = 0
oshiro_hotel_cutscene.justification = {0.5, 0.5}
oshiro_hotel_cutscene.texture = "characters/oshiro/idle00"
oshiro_hotel_cutscene.nodeLimits = {0, 0}

oshiro_hotel_cutscene.placements = {
    {
        name = "Oshiro Front Desk",
        data = {
            phase = "front_desk"
        }
    },
    {
        name = "Oshiro Hallway A",
        data = {
            phase = "hallway_a"
        }
    },
    {
        name = "Oshiro Hallway B",
        data = {
            phase = "hallway_b"
        }
    },
    {
        name = "Oshiro Clutter",
        data = {
            phase = "clutter"
        }
    },
    {
        name = "Hotel Guestbook",
        data = {
            phase = "guestbook"
        }
    },
    {
        name = "Hotel Memo",
        data = {
            phase = "memo"
        }
    }
}

oshiro_hotel_cutscene.fieldInformation = {
    phase = {
        fieldType = "string",
        options = {
            "front_desk",
            "hallway_a",
            "hallway_b",
            "clutter",
            "guestbook",
            "memo"
        }
    }
}

return oshiro_hotel_cutscene