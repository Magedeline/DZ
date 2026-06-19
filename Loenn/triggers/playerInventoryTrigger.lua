local playerInventoryTrigger = {}

playerInventoryTrigger.name = "DZ/PlayerInventoryTrigger"

local inventoryTypes = {
    "KirbyPlayer",
    "Default",
    "DZ_CH6End",
    "TheSummit",
    "Core",
    "OldSite",
    "Prologue",
    "Farewell",
    "Custom",
    "SayGoodbye",
    "TitanTowerClimbing",
    "Corruption",
    "TheEnd"
}

local playerStates = {
    "NoChange",
    "Enable",
    "Disable"
}

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
    "InfernoSuper",
    "GrandHammer",
    "MechaniZeranger",
    "FrostMind",
    "UltraSword",
    "Knight"
}

playerInventoryTrigger.fieldInformation = {
    inventoryType = {
        options = inventoryTypes,
        editable = false
    },
    playerState = {
        options = playerStates,
        editable = false
    },
    kirbyPower = {
        options = kirbyPowerStates,
        editable = false
    },
    dashes = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 10
    },
    dreamDash = {
        fieldType = "boolean"
    },
    backpack = {
        fieldType = "boolean"
    },
    noRefills = {
        fieldType = "boolean"
    },
    triggerOnce = {
        fieldType = "boolean"
    },
    requiredFlag = {
        fieldType = "string"
    }
}

playerInventoryTrigger.fieldOrder = {
    "x", "y", "width", "height",
    "playerState",
    "inventoryType",
    "kirbyPower",
    "dashes",
    "dreamDash",
    "backpack",
    "noRefills",
    "triggerOnce",
    "requiredFlag"
}

playerInventoryTrigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            inventoryType = "KirbyPlayer",
            playerState = "NoChange",
            kirbyPower = "None",
            dashes = 3,
            dreamDash = false,
            backpack = true,
            noRefills = false,
            triggerOnce = true,
            requiredFlag = ""
        }
    },
    {
        name = "enable_player",
        data = {
            width = 16,
            height = 16,
            inventoryType = "KirbyPlayer",
            playerState = "Enable",
            kirbyPower = "None",
            dashes = 3,
            dreamDash = false,
            backpack = true,
            noRefills = false,
            triggerOnce = true,
            requiredFlag = ""
        }
    },
    {
        name = "disable_player",
        data = {
            width = 16,
            height = 16,
            inventoryType = "Default",
            playerState = "Disable",
            kirbyPower = "None",
            dashes = 1,
            dreamDash = false,
            backpack = true,
            noRefills = false,
            triggerOnce = true,
            requiredFlag = ""
        }
    },
    {
        name = "custom_inventory",
        data = {
            width = 16,
            height = 16,
            inventoryType = "Custom",
            playerState = "NoChange",
            kirbyPower = "None",
            dashes = 2,
            dreamDash = true,
            backpack = true,
            noRefills = false,
            triggerOnce = true,
            requiredFlag = ""
        }
    }
}

return playerInventoryTrigger
