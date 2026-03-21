-- Shared Kirby data for MaggyHelper Loenn plugins
-- Contains common constants and helper functions used across multiple plugins

local kirbyData = {}

-- Full list of Kirby power states matching KirbyPlayerExtension.KirbyPowerState enum
kirbyData.powerStates = {
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

-- Kirby intro types matching KirbyPlayerExtension.KirbyIntroType enum
kirbyData.introTypes = {
    "None",
    "WalkIn",
    "Fall",
    "FallSlow",
    "WarpStar",
    "Jump",
    "WakeUp",
    "Respawn",
    "ThinkIn",
    "FloatDown",
    "BubblePop",
    "DoorEnter",
    "PipeExit"
}

-- Color mapping for powers (for visual distinction in editor)
kirbyData.powerColors = {
    None = {1.0, 0.41, 0.71, 0.6},       -- Pink (default)
    Fire = {1.0, 0.27, 0.0, 0.6},        -- Orange-red
    Ice = {0.68, 0.85, 0.9, 0.6},        -- Light blue
    Spark = {1.0, 1.0, 0.0, 0.6},        -- Yellow
    Stone = {0.5, 0.5, 0.5, 0.6},        -- Gray
    Sword = {0.0, 1.0, 0.5, 0.6},        -- Spring green
    Beam = {0.93, 0.51, 0.93, 0.6},      -- Violet
    Cutter = {0.9, 0.9, 0.5, 0.6},       -- Pale yellow
    Hammer = {0.55, 0.27, 0.07, 0.6},    -- Brown
    Wing = {1.0, 1.0, 0.8, 0.6},         -- Light yellow
    Archer = {0.0, 0.8, 0.0, 0.6},       -- Green
    Leaf = {0.13, 0.55, 0.13, 0.6},      -- Forest green
    Water = {0.0, 0.5, 1.0, 0.6},        -- Blue
    Mirror = {0.8, 0.8, 1.0, 0.6},       -- Light purple
    Esp = {0.6, 0.2, 0.8, 0.6},          -- Purple
    Ranger = {0.4, 0.4, 0.4, 0.6},       -- Dark gray
    Mike = {0.9, 0.1, 0.1, 0.6},         -- Red
    Crash = {1.0, 0.0, 0.0, 0.6},        -- Bright red
    Bomb = {0.3, 0.3, 0.3, 0.6},         -- Dark gray
    Painter = {0.9, 0.4, 0.7, 0.6},      -- Pink-purple
    Cook = {1.0, 0.6, 0.4, 0.6},         -- Peach
    Bell = {1.0, 0.84, 0.0, 0.6},        -- Gold
    Light = {1.0, 1.0, 0.9, 0.6},        -- Cream
    Drill = {0.6, 0.4, 0.2, 0.6},        -- Brown
    Wheel = {0.7, 0.1, 0.1, 0.6},        -- Dark red
    Phase = {0.5, 0.0, 0.5, 0.6},        -- Purple
    Umbrella = {0.8, 0.4, 0.8, 0.6},     -- Light purple
    Recycler = {0.2, 0.7, 0.2, 0.6},     -- Green
    Mini = {0.9, 0.7, 0.9, 0.6},         -- Light pink
    TripleSwap = {0.6, 0.6, 0.9, 0.6},   -- Blue-purple
    TimeCrash = {0.2, 0.2, 0.4, 0.6},    -- Dark blue
    -- Super abilities (brighter/more distinct colors)
    InfernoSuper = {1.0, 0.4, 0.0, 0.8}, -- Bright orange
    GrandHammer = {0.7, 0.35, 0.1, 0.8}, -- Bronze
    MechaniZeranger = {0.6, 0.6, 0.7, 0.8}, -- Steel
    FrostMind = {0.4, 0.8, 1.0, 0.8},    -- Cyan
    UltraSword = {0.2, 1.0, 0.6, 0.8},   -- Bright green
    Knight = {1.0, 0.84, 0.0, 0.8}       -- Bright gold
}

-- Get color for a power state
function kirbyData.getPowerColor(powerState)
    return kirbyData.powerColors[powerState] or kirbyData.powerColors.None
end

-- Check if power is a super ability
function kirbyData.isSuperAbility(powerState)
    local superAbilities = {
        "InfernoSuper", "GrandHammer", "MechaniZeranger",
        "FrostMind", "UltraSword", "Knight"
    }
    for _, ability in ipairs(superAbilities) do
        if ability == powerState then
            return true
        end
    end
    return false
end

return kirbyData
