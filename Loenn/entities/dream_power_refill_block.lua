-- DZ/DreamPowerRefillBlock
-- Context-aware refill pick-up entity:
--   • Kirby mode  → restores hover stamina; optionally grants a copy ability.
--   • Normal Madeline → restores one dash charge (when refillDash = true).
-- Respawns after ~2.5 s unless oneUse is enabled.

local kirbyPowers = {
    "None",
    "Fire", "Ice", "Spark", "Stone", "Sword", "Archer",
    "Leaf", "Water", "Esp", "Hammer", "Ranger", "Mike",
    "Crash", "Bomb", "Cutter", "Painter", "Cook", "Bell",
    "Light", "Drill", "Beam", "Wheel", "Phase",
    "TripleSwap", "TimeCrash", "Umbrella", "Mirror", "Recycler",
    "Mini", "InfernoLight", "GrandHammer", "MechanizeRanger",
    "FrostMind", "UltraSword", "Knight",
    }

return {
    name      = "DZ/DreamPowerRefillBlock",
    depth     = -100,
    -- Use the vanilla refill sprite as the editor stand-in.
    texture        = "objects/refill/idle00",
    justification  = {0.5, 0.5},
    color          = {1.0, 0.41, 0.71, 1.0},   -- pink tint in editor
    fieldInformation = {
        oneUse = {
            fieldType = "boolean"
        },
        refillDash = {
            fieldType = "boolean"
        },
        grantPower = {
            fieldType = "string",
            options   = kirbyPowers,
            editable  = false,
    },
    },
    fieldOrder = {
        "x", "y",
        "oneUse", "refillDash", "grantPower"
    },
    placements = {
        {
            name = "Dream Power Refill (default)",
            data = {
                oneUse     = false,
                refillDash = true,
                grantPower = "None",
    }
        },
        {
            name = "Dream Power Refill (one-use)",
            data = {
                oneUse     = true,
                refillDash = true,
                grantPower = "None",
    }
        }
    }
}
