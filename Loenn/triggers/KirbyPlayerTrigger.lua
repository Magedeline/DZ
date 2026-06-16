-- Loenn integration for Kirby Player Trigger
-- Allows transformation between normal and Kirby player modes

local kirbyPlayerTrigger = {}

kirbyPlayerTrigger.name = "DZ/Kirby_Player_Trigger"

-- Full list of Kirby power states matching KirbyPlayerExtension.KirbyPowerState enum
local kirbyPowerStates = {
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

kirbyPlayerTrigger.fieldInformation = {
    activationType = {
        options = {
            "OnEnter",
            "OnExit", 
            "Toggle"
        },
        editable = false
    },
    transformationType = {
        options = {
            "Instant",
            "Animated",
            "Fade"
        },
        editable = false
    },
    oneUse = {
        fieldType = "boolean"
    },
    transformAnimation = {
        fieldType = "string"
    },
    transformDuration = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    preserveVelocity = {
        fieldType = "boolean"
    },
    requiredFlag = {
        fieldType = "string"
    },
    playSound = {
        fieldType = "boolean"
    },
    initialPower = {
        options = kirbyPowerStates,
        editable = false
    }
}

kirbyPlayerTrigger.fieldOrder = {
    "x", "y", "width", "height",
    "activationType",
    "transformationType",
    "oneUse",
    "transformAnimation",
    "transformDuration",
    "preserveVelocity",
    "requiredFlag",
    "playSound",
    "initialPower"
}

kirbyPlayerTrigger.placements = {
    {
        name = "on_enter",
        data = {
            width = 16,
            height = 16,
            activationType = "OnEnter",
            transformationType = "Animated",
            oneUse = false,
            transformAnimation = "transform_to_kirby",
            transformDuration = 1.0,
            preserveVelocity = true,
            requiredFlag = "",
            playSound = true,
            initialPower = "None"
        }
    },
    {
        name = "on_exit",
        data = {
            width = 16,
            height = 16,
            activationType = "OnExit",
            transformationType = "Fade",
            oneUse = false,
            transformAnimation = "transform_to_normal",
            transformDuration = 0.5,
            preserveVelocity = true,
            requiredFlag = "",
            playSound = true,
            initialPower = "None"
        }
    },
    {
        name = "toggle",
        data = {
            width = 16,
            height = 16,
            activationType = "Toggle",
            transformationType = "Instant",
            oneUse = true,
            transformAnimation = "transform",
            transformDuration = 0.0,
            preserveVelocity = false,
            requiredFlag = "",
            playSound = true,
            initialPower = "None"
        }
    },
    {
        name = "fire_power_enable",
        data = {
            width = 16,
            height = 16,
            activationType = "OnEnter",
            transformationType = "Animated",
            oneUse = false,
            transformAnimation = "transform_to_kirby",
            transformDuration = 1.0,
            preserveVelocity = true,
            requiredFlag = "",
            playSound = true,
            initialPower = "Fire"
        }
    },
    {
        name = "sword_power_enable",
        data = {
            width = 16,
            height = 16,
            activationType = "OnEnter",
            transformationType = "Animated",
            oneUse = false,
            transformAnimation = "transform_to_kirby",
            transformDuration = 1.0,
            preserveVelocity = true,
            requiredFlag = "",
            playSound = true,
            initialPower = "Sword"
        }
    },
    {
        name = "knight_mode_enable",
        data = {
            width = 16,
            height = 16,
            activationType = "OnEnter",
            transformationType = "Animated",
            oneUse = true,
            transformAnimation = "transform_to_knight",
            transformDuration = 1.5,
            preserveVelocity = true,
            requiredFlag = "",
            playSound = true,
            initialPower = "Knight"
        }
    }
}

-- Color based on activation type for visual distinction in editor
function kirbyPlayerTrigger.color(room, entity)
    local activationType = entity.activationType or "OnEnter"
    local initialPower = entity.initialPower or "None"

    -- Special colors for super abilities
    if initialPower == "Knight" then
        return {1.0, 0.84, 0.0, 0.6}  -- Gold for Knight
    elseif initialPower == "UltraSword" then
        return {1.0, 0.5, 0.0, 0.6}   -- Orange for UltraSword
    elseif initialPower ~= "None" then
        return {1.0, 0.75, 0.8, 0.6}  -- Light pink for powers
    end

    if activationType == "OnEnter" then
        return {1.0, 0.41, 0.71, 0.6}  -- Pink
    elseif activationType == "OnExit" then
        return {0.71, 0.41, 1.0, 0.6}  -- Purple
    elseif activationType == "Toggle" then
        return {0.41, 0.71, 1.0, 0.6}  -- Blue
    end

    return {1.0, 0.41, 0.71, 0.6}  -- Default pink
end

return kirbyPlayerTrigger
