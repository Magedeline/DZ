local utils = require("utils")
local selectionBounds = require("libraries.selection_bounds")

local elementalEffectTrigger = {}

local defaultBounds = {
    width = 16,
    height = 16
}

elementalEffectTrigger.name = "MaggyHelper/ElementalEffectTrigger"
elementalEffectTrigger.depth = 0

elementalEffectTrigger.placements = {
    {
        name = "fire_effect",
        data = {
            width = 16,
            height = 16,
            effectType = "fire_burst",
            elementType = "Fire",
            intensity = 1.0,
            radius = 32.0,
            duration = 1.0,
            triggerOnEnter = true,
            triggerOnExit = false,
            oneUse = false
        }
    },
    {
        name = "ice_effect",
        data = {
            width = 16,
            height = 16,
            effectType = "ice_burst",
            elementType = "Ice",
            intensity = 1.0,
            radius = 32.0,
            duration = 1.0,
            triggerOnEnter = true,
            triggerOnExit = false,
            oneUse = false
        }
    },
    {
        name = "lightning_effect",
        data = {
            width = 16,
            height = 16,
            effectType = "chain_lightning",
            elementType = "Lightning",
            intensity = 1.0,
            radius = 64.0,
            duration = 1.0,
            triggerOnEnter = true,
            triggerOnExit = false,
            oneUse = false
        }
    },
    {
        name = "earth_effect",
        data = {
            width = 16,
            height = 16,
            effectType = "earthquake",
            elementType = "Earth",
            intensity = 1.0,
            radius = 48.0,
            duration = 3.0,
            triggerOnEnter = true,
            triggerOnExit = false,
            oneUse = false
        }
    }
}

elementalEffectTrigger.fieldInformation = {
    effectType = {
        options = {
            "fire_burst", "fire_explosion", "fire_pillar", "fire_trail", "magical_fire",
            "ice_burst", "freeze_explosion", "blizzard", "ice_wall", "icicle_spikes",
            "lightning_strike", "chain_lightning", "electric_field", "plasma_ball", "emp",
            "earthquake", "earth_spike", "boulder_throw", "crystal_formation",
            "tornado", "wind_gust", "air_current",
            "shadow_wave", "void_portal", "corruption",
            "light_beam", "healing_light", "radiance_burst"
        },
        editable = false
    },
    elementType = {
        options = {"Fire", "Ice", "Lightning", "Earth", "Wind", "Dark", "Light"},
        editable = false
    },
    intensity = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },
    radius = {
        fieldType = "number",
        minimumValue = 8.0,
        maximumValue = 128.0
    },
    duration = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    }
}

function elementalEffectTrigger.texture(room, entity)
    local elementType = entity.elementType or "Fire"
    
    if elementType == "Fire" then
        return "objects/trigger/fire"
    elseif elementType == "Ice" then
        return "objects/trigger/ice"
    elseif elementType == "Lightning" then
        return "objects/trigger/lightning"
    elseif elementType == "Earth" then
        return "objects/trigger/earth"
    elseif elementType == "Wind" then
        return "objects/trigger/wind"
    elseif elementType == "Dark" then
        return "objects/trigger/dark"
    elseif elementType == "Light" then
        return "objects/trigger/light"
    else
        return "objects/trigger/default"
    end
end

function elementalEffectTrigger.rectangle(room, entity)
    return selectionBounds.rectangle(entity, defaultBounds)
end

return elementalEffectTrigger