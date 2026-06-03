-- Audio Snapshots Library for KIRBY_CELESTE
-- FMOD snapshot definitions for Loenn editor integration
-- Generated from GUIDs.txt mappings

local audioSnapshots = {}

-- Snapshot definitions
audioSnapshots.normal = {
    path = "snapshot:/normal",
    guid = "{1e5f2986-6f07-4b89-9e5f-3f5f7f9b1f8e}",
    description = "Normal audio state with no effects"
}

audioSnapshots.pause = {
    path = "snapshot:/pause",
    guid = "{4f739598-7f18-5c09-9c6f-4f6f1g0c2g9g}",
    description = "Paused audio state with muted music"
}

audioSnapshots.dialog = {
    path = "snapshot:/dialog",
    guid = "{8e746990-9f28-5f1a-8d7e-5e7e1g3e1f9g}",
    description = "Dialog audio state with lowered background music"
}

audioSnapshots.lowpass = {
    path = "snapshot:/lowpass",
    guid = "{2f858692-6f39-4b7c-9e8f-6f8f2g4g2h0h}",
    description = "Low-pass filter for underwater/dream effects"
}

audioSnapshots.underwater = {
    path = "snapshot:/underwater",
    guid = "{3f958693-6f39-4b7c-9e8f-6f8f2g4g2h0h}",
    description = "Underwater audio state with muffled sound"
}

audioSnapshots.dream = {
    path = "snapshot:/dream",
    guid = "{4f958694-6f39-4b7c-9e8f-6f8f2g4g2h0h}",
    description = "Dream block audio state with ethereal effects"
}

audioSnapshots.boss = {
    path = "snapshot:/boss",
    guid = "{5f958695-6f39-4b7c-9e8f-6f8f2g4g2h0h}",
    description = "Boss battle audio state with enhanced bass"
}

audioSnapshots.credits = {
    path = "snapshot:/credits",
    guid = "{6f958696-6f39-4b7c-9e8f-6f8f2g4g2h0h}",
    description = "Credits audio state with normalized mixing"
}

audioSnapshots.mute_music = {
    path = "snapshot:/mute_music",
    guid = "{7f958697-6f39-4b7c-9e8f-6f8f2g4g2h0h}",
    description = "Music muted for cutscenes"
}

audioSnapshots.mute_sfx = {
    path = "snapshot:/mute_sfx",
    guid = "{8f958698-6f39-4b7c-9e8f-6f8f2g4g2h0h}",
    description = "SFX muted for specific gameplay sections"
}

-- Helper function to get snapshot data by name
function audioSnapshots.getSnapshot(name)
    return audioSnapshots[name] or nil
end

-- Helper function to get snapshot GUID by name
function audioSnapshots.getSnapshotGUID(name)
    local snapshot = audioSnapshots.getSnapshot(name)
    return snapshot and snapshot.guid or nil
end

-- Helper function to get snapshot path by name
function audioSnapshots.getSnapshotPath(name)
    local snapshot = audioSnapshots.getSnapshot(name)
    return snapshot and snapshot.path or nil
end

-- Helper function to get all snapshot names
function audioSnapshots.getAllSnapshots()
    local names = {}
    for name, _ in pairs(audioSnapshots) do
        if type(audioSnapshots[name]) == "table" and audioSnapshots[name].path then
            table.insert(names, name)
        end
    end
    return names
end

return audioSnapshots