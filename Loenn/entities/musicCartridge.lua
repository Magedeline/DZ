local musicCartridge = {}

musicCartridge.name = "DesoloZantas/MusicCartridge"
musicCartridge.depth = -100
musicCartridge.texture = "collectibles/desolozantas/musiccartridge/idle00"
musicCartridge.placements = {
    {
        name = "music_cartridge",
        data = {
            cartridgeId = "music_default",
            musicEvent = "",
            unlockFlag = "cartridge_music_default",
            name = "Music Cartridge",
            label = "???",
            color = "FF1493",
            playOnCollect = true,
            persistent = true,
            remixIndex = 0
        }
    },
    {
        name = "music_cartridge_repeatable",
        data = {
            cartridgeId = "music_repeat",
            musicEvent = "event:/music/yourtrack",
            unlockFlag = "cartridge_music_repeat",
            name = "Repeatable Cartridge",
            label = "MIX",
            color = "00FFFF",
            playOnCollect = true,
            persistent = false,
            remixIndex = 0
        }
    }
}

musicCartridge.fieldInformation = {
    color = {
        fieldType = "color",
        allowXNAColors = true
    },
    remixIndex = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 0
    }
}

return musicCartridge
