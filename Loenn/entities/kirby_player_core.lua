local kirbyKglobal::PlayerCore = {}

kirbyKglobal::PlayerCore.name = "MaggyHelper/KirbyKglobal::PlayerCore"
kirbyKglobal::PlayerCore.depth = -100
kirbyKglobal::PlayerCore.texture = "characters/Maggy/DesoloZantas/kirby/idle00"
kirbyKglobal::PlayerCore.justification = {0.5, 1.0}

kirbyKglobal::PlayerCore.nodeLineRenderType = "line"
kirbyKglobal::PlayerCore.nodeLimits = {0, -1}
kirbyKglobal::PlayerCore.nodeVisibility = "always"

kirbyKglobal::PlayerCore.placements = {
    {
        name = "default",
        data = {
            maxHealth = 6,
            power = "None",
            inventory = "KirbyKglobal::Player",
            introType = "None",
            useSpawnPoints = true
        }
    }
}

kirbyKglobal::PlayerCore.fieldInformation = {
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
            "None", "KirbyKglobal::Player", "KirbyCompanion", "KirbyModeOnly"
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

kirbyKglobal::PlayerCore.fieldOrder = {
    "x",
    "y",
    "maxHealth",
    "power",
    "inventory",
    "introType",
    "useSpawnPoints"
}

return kirbyKglobal::PlayerCore

