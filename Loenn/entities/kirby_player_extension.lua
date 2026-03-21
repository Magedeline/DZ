local drawableSprite = require("structs.drawable_sprite")

local kirbyPlayerExtension = {}

kirbyPlayerExtension.name = "MaggyHelper/KirbyPlayerExtension"
kirbyPlayerExtension.depth = -100
kirbyPlayerExtension.texture = "characters/kirby/idle00"
kirbyPlayerExtension.justification = {0.5, 1.0}

-- Node support for spawn points
kirbyPlayerExtension.nodeLineRenderType = "line"
kirbyPlayerExtension.nodeLimits = {0, -1}
kirbyPlayerExtension.nodeVisibility = "always"

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

-- Kirby intro types matching KirbyPlayerExtension.KirbyIntroType enum
local kirbyIntroTypes = {
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

kirbyPlayerExtension.placements = {
    {
        name = "default",
        data = {
            maxHealth = 6,
            power = "None",
            inventory = "KirbyPlayer",
            introType = "None",
            useSpawnPoints = true
        }
    },
    {
        name = "walk_in",
        data = {
            maxHealth = 6,
            power = "None",
            inventory = "KirbyPlayer",
            introType = "WalkIn",
            useSpawnPoints = true
        }
    },
    {
        name = "warp_star",
        data = {
            maxHealth = 6,
            power = "None",
            inventory = "KirbyPlayer",
            introType = "WarpStar",
            useSpawnPoints = true
        }
    },
    {
        name = "float_down",
        data = {
            maxHealth = 6,
            power = "None",
            inventory = "KirbyPlayer",
            introType = "FloatDown",
            useSpawnPoints = true
        }
    },
    {
        name = "with_fire",
        data = {
            maxHealth = 6,
            power = "Fire",
            inventory = "KirbyPlayer",
            introType = "None",
            useSpawnPoints = true
        }
    },
    {
        name = "with_sword",
        data = {
            maxHealth = 6,
            power = "Sword",
            inventory = "KirbyPlayer",
            introType = "None",
            useSpawnPoints = true
        }
    },
    {
        name = "knight_mode",
        data = {
            maxHealth = 8,
            power = "Knight",
            inventory = "KirbyPlayer",
            introType = "WarpStar",
            useSpawnPoints = true
        }
    }
}

kirbyPlayerExtension.fieldInformation = {
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 20
    },
    power = {
        options = kirbyPowerStates,
        editable = false
    },
    inventory = {
        options = {
            "Default",
            "KirbyPlayer",
            "Heart"
        },
        editable = false
    },
    introType = {
        options = kirbyIntroTypes,
        editable = false
    },
    useSpawnPoints = {
        fieldType = "boolean"
    }
}

kirbyPlayerExtension.fieldOrder = {
    "x", "y",
    "maxHealth",
    "power",
    "inventory",
    "introType",
    "useSpawnPoints"
}

-- Node rendering for spawn points
function kirbyPlayerExtension.nodeSprite(room, entity, node, nodeIndex)
    local sprite = drawableSprite.fromTexture("characters/player/sitDown00", {x = node.x, y = node.y})
    sprite:setColor({1.0, 0.6, 0.8, 0.8})
    sprite:setJustification(0.5, 1.0)
    sprite:setScale(0.7, 0.7)
    return sprite
end

-- Color based on power for visual distinction
function kirbyPlayerExtension.color(room, entity)
    local power = entity.power or "None"

    if power == "Knight" then
        return {1.0, 0.84, 0.0}  -- Gold
    elseif power == "Fire" or power == "InfernoSuper" then
        return {1.0, 0.27, 0.0}  -- Orange-red
    elseif power == "Ice" or power == "FrostMind" then
        return {0.68, 0.85, 0.9}  -- Light blue
    elseif power == "Sword" or power == "UltraSword" then
        return {0.0, 1.0, 0.5}   -- Spring green
    elseif power == "Spark" then
        return {1.0, 1.0, 0.0}   -- Yellow
    elseif power ~= "None" then
        return {1.0, 0.6, 0.8}   -- Light pink for other powers
    end

    return {1.0, 0.41, 0.71}  -- Default pink
end

return kirbyPlayerExtension
