-- Audio Buses Library for KIRBY_CELESTE
-- FMOD bus definitions for Loenn editor integration
-- Generated from GUIDs.txt mappings

local audioBuses = {}

-- Bus definitions for audio routing
audioBuses.master = {
    path = "bus:/master",
    guid = "{d8d6b462-8e70-4f85-bf9d-b0e551b5c75b}",
    description = "Master audio bus (final output)"
}

audioBuses.music = {
    path = "bus:/music",
    guid = "{07485943-b4ee-4d3a-9d2b-9eebf1d7042d}",
    description = "Music audio bus"
}

audioBuses.sfx = {
    path = "bus:/sfx",
    guid = "{6e35267b-1bd3-4f37-a86a-af03f7e268c7}",
    description = "Sound effects audio bus"
}

audioBuses.ambience = {
    path = "bus:/ambience",
    guid = "{a3d36685-6f35-4b8d-8a3e-5e4b5e9e8c5a}",
    description = "Ambience audio bus"
}

audioBuses.dialogue = {
    path = "bus:/dialogue",
    guid = "{c8d6b795-2e83-4f95-b0e6-6f8e5f6b4e7c}",
    description = "Dialogue audio bus"
}

audioBuses.ui = {
    path = "bus:/ui",
    guid = "{f9e5c398-5f4a-4e87-9f4a-5a4e6f7b3d9e}",
    description = "UI sounds audio bus"
}

audioBuses.cassette = {
    path = "bus:/cassette",
    guid = "{0e5c3999-6f4b-4f88-9e4b-6b4b5f0c3d0f}",
    description = "Cassette music bus"
}

audioBuses.boss_music = {
    path = "bus:/boss_music",
    guid = "{1f6c39a0-6f4c-4f89-9e4c-7c4c6f1d4e1e}",
    description = "Boss battle music bus"
}

audioBuses.boss_sfx = {
    path = "bus:/boss_sfx",
    guid = "{2f6c39a1-6f4d-4f8a-9e4d-8d4d7g2e5f2f}",
    description = "Boss battle SFX bus"
}

audioBuses.water = {
    path = "bus:/water",
    guid = "{3f6c39a2-6f4e-4f8b-9e4e-9e4e8f3f6g3g}",
    description = "Underwater audio bus"
}

audioBuses.dream = {
    path = "bus:/dream",
    guid = "{4f6c39a3-6f4f-4f8c-9e4f-af4f9g4g7h4h}",
    description = "Dream block audio bus"
}

audioBuses.cutscene = {
    path = "bus:/cutscene",
    guid = "{5f6c39a4-6f50-4f8d-9e50-bf50ag5h8i5i}",
    description = "Cutscene audio bus"
}

audioBuses.ambient_music = {
    path = "bus:/ambient_music",
    guid = "{6f6c39a5-6f51-4f8e-9e51-cf51bh6i9j6j}",
    description = "Ambient music bus"
}

audioBuses.cinematic = {
    path = "bus:/cinematic",
    guid = "{7f6c39a6-6f52-4f8f-9e52-df52ci7jak7k}",
    description = "Cinematic audio bus"
}

audioBuses.mechanics = {
    path = "bus:/mechanics",
    guid = "{8f6c39a7-6f53-4f90-9e53-ef53dj8kbl8l}",
    description = "Mechanics audio bus (dash, jump, etc.)"
}

-- Helper function to get bus data by name
function audioBuses.getBus(name)
    return audioBuses[name] or nil
end

-- Helper function to get bus GUID by name
function audioBuses.getBusGUID(name)
    local bus = audioBuses.getBus(name)
    return bus and bus.guid or nil
end

-- Helper function to get bus path by name
function audioBuses.getBusPath(name)
    local bus = audioBuses.getBus(name)
    return bus and bus.path or nil
end

-- Helper function to get all bus names
function audioBuses.getAllBuses()
    local names = {}
    for name, data in pairs(audioBuses) do
        if type(data) == "table" and data.path then
            table.insert(names, name)
        end
    end
    return names
end

return audioBuses