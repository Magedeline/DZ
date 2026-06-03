-- Audio VCA Library for KIRBY_CELESTE
-- FMOD VCA (Volume Control Architecture) definitions for Loenn editor integration
-- Generated from GUIDs.txt mappings

local audioVCA = {}

-- VCA definitions for volume control
audioVCA.master_vca = {
    path = "vca:/master",
    guid = "{2c854783-5e52-4a97-b5e2-7e4b7f8e9d6e}",
    description = "Master volume control for all audio"
}

audioVCA.music_vca = {
    path = "vca:/music",
    guid = "{1b6f5874-4e75-4b87-9a5f-3e5e6f9a0e7f}",
    description = "Music volume control"
}

audioVCA.sfx_vca = {
    path = "vca:/sfx",
    guid = "{3c859486-6f96-5c98-7b6g-4g6g0g1b8g2g}",
    description = "Sound effects volume control"
}

audioVCA.ambience_vca = {
    path = "vca:/ambience",
    guid = "{0d6f3975-4e86-4a78-9e4e-2e4e6e8b1f9e}",
    description = "Ambience volume control"
}

audioVCA.dialogue_vca = {
    path = "vca:/dialogue",
    guid = "{1e6f3976-4e86-4a78-9e4e-2e4e6e8b1f9f}",
    description = "Dialogue volume control"
}

audioVCA.ui_vca = {
    path = "vca:/ui",
    guid = "{2e6f3977-4e86-4a78-9e4e-2e4e6e8b1f9g}",
    description = "UI sound volume control"
}

audioVCA.boss_music_vca = {
    path = "vca:/boss_music",
    guid = "{3e6f3978-4e86-4a78-9e4e-2e4e6e8b1f9h}",
    description = "Boss battle music volume control"
}

audioVCA.boss_sfx_vca = {
    path = "vca:/boss_sfx",
    guid = "{4e6f3979-4e86-4a78-9e4e-2e4e6e8b1f9i}",
    description = "Boss battle SFX volume control"
}

audioVCA.water_vca = {
    path = "vca:/water",
    guid = "{5e6f397a-4e86-4a78-9e4e-2e4e6e8b1f9j}",
    description = "Underwater audio volume control"
}

audioVCA.dream_vca = {
    path = "vca:/dream",
    guid = "{6e6f397b-4e86-4a78-9e4e-2e4e6e8b1f9k}",
    description = "Dream block audio volume control"
}

-- Helper function to get VCA data by name
function audioVCA.getVCA(name)
    return audioVCA[name] or nil
end

-- Helper function to get VCA GUID by name
function audioVCA.getVCAGUID(name)
    local vca = audioVCA.getVCA(name)
    return vca and vca.guid or nil
end

-- Helper function to get VCA path by name
function audioVCA.getVCAPath(name)
    local vca = audioVCA.getVCA(name)
    return vca and vca.path or nil
end

-- Helper function to get all VCA names
function audioVCA.getAllVCAs()
    local names = {}
    for name, data in pairs(audioVCA) do
        if type(data) == "table" and data.path then
            table.insert(names, name)
        end
    end
    return names
end

return audioVCA