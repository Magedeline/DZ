-- Loenn Map Audio Integration for KIRBY_CELESTE
-- Provides extra music and SFX integration for the Loenn map editor
-- Works with audio_library.lua, audio_snapshots.lua, audio_vca.lua, audio_buses.lua, audio_banks.lua

local audioLib = require("libraries.audio_library")
local audioSnapshots = require("libraries.audio_snapshots")
local audioVCA = require("libraries.audio_vca")
local audioBuses = require("libraries.audio_buses")
local audioBanks = require("libraries.audio_banks")

local mapAudioIntegration = {}

-- ── Map-Specific Music Definitions ────────────────────────────────────────

mapAudioIntegration.mapMusic = {
    -- Chapter 1: Prologue
    ch1_prologue = {
        name = "Prologue - Forsaken City",
        track = "event:/music/prologue",
        fallback = "event:/music/c1/prologue",
        mood = "calm",
        instrument = "piano"
    },
    
    -- Chapter 2: The Core
    ch2_core = {
        name = "The Core",
        track = "event:/music/core",
        fallback = "event:/music/c2/core",
        mood = "intense",
        instrument = "electronic"
    },
    
    -- Chapter 3: Reflection
    ch3_reflection = {
        name = "Reflection",
        track = "event:/music/reflection",
        fallback = "event:/music/c3/reflection",
        mood = "melancholic",
        instrument = "orchestral"
    },
    
    -- Kirby-themed chapters
    kirby_popstar = {
        name = "Popstar - Green Greens",
        track = "event:/music/kirby/popstar",
        fallback = "event:/music/classic/popstar",
        mood = "cheerful",
        instrument = "chiptune"
    },
    
    kirby_dedede = {
        name = "Dedede's Theme",
        track = "event:/music/kirby/dedede",
        fallback = "event:/music/boss/dedede",
        mood = "energetic",
        instrument = "orchestral"
    },
    
    kirby_meta_knight = {
        name = "Meta Knight's Revenge",
        track = "event:/music/kirby/meta_knight",
        fallback = "event:/music/boss/meta_knight",
        mood = "heroic",
        instrument = "orchestral"
    },
    
    kirby_void = {
        name = "Void Termina",
        track = "event:/music/kirby/void",
        fallback = "event:/music/boss/final",
        mood = "epic",
        instrument = "orchestral"
    },
    
    -- Extra music tracks
    cassette_remix = {
        name = "Cassette Remix",
        track = "event:/music/cassette_remix",
        fallback = "event:/music/cassette",
        mood = "upbeat",
        instrument = "electronic"
    },
    
    boss_music = {
        name = "Boss Battle",
        track = "event:/music/boss",
        fallback = "event:/music/boss_default",
        mood = "intense",
        instrument = "electronic"
    },
}

-- ── Map-Specific Ambience Definitions ───────────────────────────────────────

mapAudioIntegration.mapAmbience = {
    -- Environmental ambience
    forest = {
        name = "Forest Ambience",
        track = "event:/ambience/forest",
        fallback = "event:/ambience/nature",
        mood = "peaceful"
    },
    
    cave = {
        name = "Cave Ambience",
        track = "event:/ambience/cave",
        fallback = "event:/ambience/underground",
        mood = "mysterious"
    },
    
    water = {
        name = "Water Ambience",
        track = "event:/ambience/water",
        fallback = "event:/ambience/underwater",
        mood = "calm"
    },
    
    space = {
        name = "Space Ambience",
        track = "event:/ambience/space",
        fallback = "event:/ambience/void",
        mood = "ethereal"
    },
    
    dream = {
        name = "Dream Ambience",
        track = "event:/ambience/dream",
        fallback = "event:/ambience/surreal",
        mood = "mystical"
    },
    
    -- Kirby-themed ambience
    popstar_surface = {
        name = "Popstar Surface",
        track = "event:/ambience/kirby/popstar_surface",
        fallback = "event:/ambience/grassland",
        mood = "cheerful"
    },
    
    dream_land = {
        name = "Dream Land",
        track = "event:/ambience/kirby/dream_land",
        fallback = "event:/ambience/fantasy",
        mood = "peaceful"
    },
}

-- ── Map-Specific SFX Definitions ───────────────────────────────────────────

mapAudioIntegration.mapSFX = {
    -- Kirby character sounds
    kirby_jump = {
        name = "Kirby Jump",
        track = "event:/sfx/kirby/jump",
        fallback = "event:/char/madeline/jump",
        category = "movement"
    },
    
    kirby_inhale = {
        name = "Kirby Inhale",
        track = "event:/sfx/kirby/inhale",
        fallback = "event:/sfx/classic/grab",
        category = "mechanics"
    },
    
    kirby_swallow = {
        name = "Kirby Swallow",
        track = "event:/sfx/kirby/swallow",
        fallback = "event:/sfx/classic/destroy",
        category = "mechanics"
    },
    
    kirby_star_blow = {
        name = "Star Blow",
        track = "event:/sfx/kirby/star_blow",
        fallback = "event:/sfx/classic/dash",
        category = "mechanics"
    },
    
    kirby_hurt = {
        name = "Kirby Hurt",
        track = "event:/sfx/kirby/hurt",
        fallback = "event:/char/madeline/death",
        category = "damage"
    },
    
    -- Ability-specific sounds
    ability_fire = {
        name = "Fire Ability",
        track = "event:/sfx/ability/fire",
        fallback = "event:/sfx/fire",
        category = "ability"
    },
    
    ability_ice = {
        name = "Ice Ability",
        track = "event:/sfx/ability/ice",
        fallback = "event:/sfx/ice",
        category = "ability"
    },
    
    ability_spark = {
        name = "Spark Ability",
        track = "event:/sfx/ability/spark",
        fallback = "event:/sfx/electric",
        category = "ability"
    },
    
    ability_cutter = {
        name = "Cutter Ability",
        track = "event:/sfx/ability/cutter",
        fallback = "event:/sfx/projectile",
        category = "ability"
    },
    
    -- Environmental interactions
    warp_star = {
        name = "Warp Star",
        track = "event:/sfx/warp_star",
        fallback = "event:/sfx/teleport",
        category = "mechanics"
    },
    
    warp_star_arrive = {
        name = "Warp Star Arrive",
        track = "event:/sfx/warp_star_arrive",
        fallback = "event:/sfx/land",
        category = "mechanics"
    },
    
    copy_ability_gain = {
        name = "Copy Ability Gain",
        track = "event:/sfx/copy_ability_gain",
        fallback = "event:/sfx/powerup",
        category = "mechanics"
    },
}

-- ── Integration Functions for Loenn ────────────────────────────────────────

-- Get music track for a specific map/area
function mapAudioIntegration.getMapMusic(mapId)
    return mapAudioIntegration.mapMusic[mapId] or mapAudioIntegration.mapMusic.cassette_remix
end

-- Get ambience for a specific environment
function mapAudioIntegration.getMapAmbience(environment)
    return mapAudioIntegration.mapAmbience[environment] or mapAudioIntegration.mapAmbience.forest
end

-- Get SFX for a specific action
function mapAudioIntegration.getMapSFX(sfxId)
    return mapAudioIntegration.mapSFX[sfxId]
end

-- Get audio configuration for a room (combines music, ambience, and SFX)
function mapAudioIntegration.getRoomAudioConfig(room)
    local config = {
        music = nil,
        ambience = nil,
        snapshots = {},
        vca = {},
        buses = {}
    }
    
    -- Determine music based on room properties
    if room.music then
        config.music = mapAudioIntegration.getMapMusic(room.music)
    end
    
    -- Determine ambience based on room environment
    if room.environment then
        config.ambience = mapAudioIntegration.getMapAmbience(room.environment)
    end
    
    -- Set default snapshots based on room type
    if room.underwater then
        table.insert(config.snapshots, audioSnapshots.underwater)
    elseif room.dream_block then
        table.insert(config.snapshots, audioSnapshots.dream)
    elseif room.boss then
        table.insert(config.snapshots, audioSnapshots.boss)
    elseif room.dialog then
        table.insert(config.snapshots, audioSnapshots.dialog)
    end
    
    -- Configure VCA for room-specific mixing
    if room.music_mute then
        table.insert(config.vca, audioVCA.music_vca)
    end
    
    if room.sfx_mute then
        table.insert(config.vca, audioVCA.sfx_vca)
    end
    
    -- Configure bus routing
    table.insert(config.buses, audioBuses.master)
    
    if room.music_bus then
        table.insert(config.buses, audioBuses.music)
    end
    
    if room.sfx_bus then
        table.insert(config.buses, audioBuses.sfx)
    end
    
    return config
end

-- Apply audio configuration to a room
function mapAudioIntegration.applyRoomAudioConfig(room)
    local config = mapAudioIntegration.getRoomAudioConfig(room)
    
    -- Apply music
    if config.music then
        room.audioMusic = config.music.track
        room.audioMusicFallback = config.music.fallback
    end
    
    -- Apply ambience
    if config.ambience then
        room.audioAmbience = config.ambience.track
        room.audioAmbienceFallback = config.ambience.fallback
    end
    
    -- Apply snapshots
    room.audioSnapshots = {}
    for _, snapshot in ipairs(config.snapshots) do
        table.insert(room.audioSnapshots, snapshot.path)
    end
    
    -- Apply VCA settings
    room.audioVCA = {}
    for _, vca in ipairs(config.vca) do
        table.insert(room.audioVCA, vca.path)
    end
    
    -- Apply bus routing
    room.audioBuses = {}
    for _, bus in ipairs(config.buses) do
        table.insert(room.audioBuses, bus.path)
    end
    
    return config
end

-- Get audio options for Loenn UI (dropdown menus)
function mapAudioIntegration.getMusicOptions()
    local options = {}
    for id, music in pairs(mapAudioIntegration.mapMusic) do
        table.insert(options, {
            id = id,
            name = music.name,
            track = music.track,
            mood = music.mood,
            instrument = music.instrument
        })
    end
    return options
end

function mapAudioIntegration.getAmbienceOptions()
    local options = {}
    for id, ambience in pairs(mapAudioIntegration.mapAmbience) do
        table.insert(options, {
            id = id,
            name = ambience.name,
            track = ambience.track,
            mood = ambience.mood
        })
    end
    return options
end

function mapAudioIntegration.getSFXOptions()
    local options = {}
    for id, sfx in pairs(mapAudioIntegration.mapSFX) do
        table.insert(options, {
            id = id,
            name = sfx.name,
            track = sfx.track,
            category = sfx.category
        })
    end
    return options
end

-- Validate audio configuration
function mapAudioIntegration.validateConfig(config)
    local errors = {}
    
    if config.music and not audioLib.getEventPath(config.music) then
        table.insert(errors, "Invalid music track: " .. tostring(config.music))
    end
    
    if config.ambience and not audioLib.getEventPath(config.ambience) then
        table.insert(errors, "Invalid ambience track: " .. tostring(config.ambience))
    end
    
    return errors
end

-- Export for Loenn usage
mapAudioIntegration.audioLib = audioLib
mapAudioIntegration.audioSnapshots = audioSnapshots
mapAudioIntegration.audioVCA = audioVCA
mapAudioIntegration.audioBuses = audioBuses
mapAudioIntegration.audioBanks = audioBanks

return mapAudioIntegration