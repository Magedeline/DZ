local kirbyPlayerCore = {}

kirbyPlayerCore.name = "MaggyHelper/KirbyPlayerCore"
kirbyPlayerCore.depth = -100
kirbyPlayerCore.texture = "characters/kirby/idle00"
kirbyPlayerCore.justification = {0.5, 1.0}

kirbyPlayerCore.nodeLineRenderType = "line"
kirbyPlayerCore.nodeLimits = {0, -1}
kirbyPlayerCore.nodeVisibility = "always"

kirbyPlayerCore.placements = {
    {
        name = "default",
        data = {
            maxHealth = 6,
            power = "None",
            inventory = "KirbyPlayer",
            introType = "None",
            useSpawnPoints = true
        }
    }
}

kirbyPlayerCore.fieldInformation = {
    power = {
        fieldType = "string",
        options = {
            "None", "Fire", "Ice", "Spark", "Stone", "Sword", "Beam", "Cutter", "Hammer", "Wing",
            "Archer", "Leaf", "Water", "Mirror", "Esp", "Ranger", "Mike", "Crash", "Bomb", "Painter",
            "Cook", "Bell", "Light", "Drill", "Wheel", "Phase", "Umbrella", "Recycler", "Mini", "TripleSwap",
            "TimeCrash", "InfernoSuper", "GrandHammer", "MechaniZeranger", "FrostMind", "UltraSword", "Knight"
        },
        editable = false
    },
    inventory = {
        fieldType = "string",
        options = {
            "None", "KirbyPlayer", "KirbyCompanion", "KirbyModeOnly"
        },
        editable = false
    },
    introType = {
        fieldType = "string",
        options = {
            "None", "WalkIn", "Fall", "FallSlow", "WarpStar", "Jump", "WakeUp", "Respawn", "ThinkIn", "FloatDown", "BubblePop", "DoorEnter", "PipeExit"
        },
        editable = false
    }
}

kirbyPlayerCore.fieldOrder = {
    "x",
    "y",
    "maxHealth",
    "power",
    "inventory",
    "introType",
    "useSpawnPoints"
}

return kirbyPlayerCore
