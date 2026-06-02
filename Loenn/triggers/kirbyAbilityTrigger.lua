-- Loenn plugin for MaggyHelper - Kirby Ability Trigger
-- Gives or removes Kirby copy abilities

local trigger = {}

trigger.name = "MaggyHelper/KirbyAbilityTrigger"

-- Full list of Kirby power states matching KirbyPlayerExtension.KirbyPowerState enum
local kirbyAbilities = {
    "None",
    "Fire",
    "Ice",
    "Spark",
    "Stone",
    "Sword",
    "Beam",
    "Cutter",
    "Hammer",
    "Wing",
    "Archer",
    "Leaf",
    "Water",
    "Mirror",
    "Esp",
    "Ranger",
    "Mike",
    "Crash",
    "Bomb",
    "Painter",
    "Cook",
    "Bell",
    "Light",
    "Drill",
    "Wheel",
    "Phase",
    "Umbrella",
    "Recycler",
    "Mini",
    "TripleSwap",
    "TimeCrash",
    -- Super abilities
    "InfernoSuper",
    "GrandHammer",
    "MechaniZeranger",
    "FrostMind",
    "UltraSword",
    "Knight"
}

trigger.fieldInformation = {
    action = {
        options = {
            "Give",
            "Remove",
            "ToggleFloat",
            "ToggleInhale"
        },
        editable = false
    },
    ability = {
        options = kirbyAbilities,
        editable = false
    },
    onlyOnce = {
        fieldType = "boolean"
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "action",
    "ability",
    "onlyOnce"
}

trigger.placements = {
    {
        name = "give_ability",
        data = {
            width = 16,
            height = 16,
            action = "Give",
            ability = "Fire",
            onlyOnce = false
        }
    },
    {
        name = "remove_ability",
        data = {
            width = 16,
            height = 16,
            action = "Remove",
            ability = "None",
            onlyOnce = false
        }
    },
    {
        name = "toggle_float",
        data = {
            width = 16,
            height = 16,
            action = "ToggleFloat",
            ability = "None",
            onlyOnce = false
        }
    },
    {
        name = "toggle_inhale",
        data = {
            width = 16,
            height = 16,
            action = "ToggleInhale",
            ability = "None",
            onlyOnce = false
        }
    },
    -- Common ability presets
    {
        name = "give_fire",
        data = {
            width = 16,
            height = 16,
            action = "Give",
            ability = "Fire",
            onlyOnce = false
        }
    },
    {
        name = "give_ice",
        data = {
            width = 16,
            height = 16,
            action = "Give",
            ability = "Ice",
            onlyOnce = false
        }
    },
    {
        name = "give_sword",
        data = {
            width = 16,
            height = 16,
            action = "Give",
            ability = "Sword",
            onlyOnce = false
        }
    },
    {
        name = "give_beam",
        data = {
            width = 16,
            height = 16,
            action = "Give",
            ability = "Beam",
            onlyOnce = false
        }
    },
    {
        name = "give_hammer",
        data = {
            width = 16,
            height = 16,
            action = "Give",
            ability = "Hammer",
            onlyOnce = false
        }
    },
    {
        name = "give_knight",
        data = {
            width = 16,
            height = 16,
            action = "Give",
            ability = "Knight",
            onlyOnce = true
        }
    }
}

-- Color based on action and ability for visual distinction
function trigger.color(room, entity)
    local action = entity.action or "Give"
    local ability = entity.ability or "None"

    if action == "Remove" then
        return {0.5, 0.5, 0.5, 0.6}  -- Gray
    elseif action == "ToggleFloat" then
        return {0.6, 0.8, 1.0, 0.6}  -- Light blue
    elseif action == "ToggleInhale" then
        return {1.0, 0.6, 0.8, 0.6}  -- Light pink
    end

    -- Colors for Give action based on ability
    if ability == "Fire" or ability == "InfernoSuper" then
        return {1.0, 0.27, 0.0, 0.6}  -- Orange-red
    elseif ability == "Ice" or ability == "FrostMind" then
        return {0.68, 0.85, 0.9, 0.6}  -- Light blue
    elseif ability == "Spark" then
        return {1.0, 1.0, 0.0, 0.6}   -- Yellow
    elseif ability == "Sword" or ability == "UltraSword" then
        return {0.0, 1.0, 0.5, 0.6}   -- Spring green
    elseif ability == "Hammer" or ability == "GrandHammer" then
        return {0.55, 0.27, 0.07, 0.6}  -- Brown
    elseif ability == "Knight" then
        return {1.0, 0.84, 0.0, 0.6}  -- Gold
    elseif ability == "Beam" then
        return {0.93, 0.51, 0.93, 0.6}  -- Violet
    elseif ability == "Water" then
        return {0.0, 0.5, 1.0, 0.6}   -- Blue
    elseif ability == "Wing" then
        return {1.0, 1.0, 0.8, 0.6}   -- Light yellow
    elseif ability == "Stone" then
        return {0.5, 0.5, 0.5, 0.6}   -- Gray
    end

    return {1.0, 0.41, 0.71, 0.6}  -- Default pink
end

return trigger
