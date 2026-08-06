local playerInventoryTrigger = {}

playerInventoryTrigger.name = "DZ/PlayerInventoryTrigger"
playerInventoryTrigger.depth = 0
playerInventoryTrigger.placements = {
    name = "player_inventory_trigger",
    data = {
        inventoryType = "KirbyPlayer",
        playerState = "NoChange",
        kirbyPower = "None",
        dreamDash = false,
        backpack = true,
        noRefills = false,
        triggerOnce = true,
        dashes = 3,
        requiredFlag = ""
    }
}

playerInventoryTrigger.fieldInformation = {
    inventoryType = {
        options = {
            "Default", "CH6End", "TheSummit", "Core", "OldSite", "Prologue",
            "Farewell", "Custom", "KirbyPlayer", "SayGoodbye", "TitanTowerClimbing",
            "Corruption", "TheEnd"
        },
        editable = false
    },
    playerState = {
        options = { "NoChange", "Enable", "Disable" },
        editable = false
    },
    kirbyPower = {
        options = {
            "None", "Fire", "Ice", "Spark", "Sword", "Cutter", "Beam", "Stone",
            "Needle", "Parasol", "Wheel", "Bomb", "Fighter", "Suplex", "Ninja",
            "Mirror", "Hammer", "Knight", "Wing", "UFO", "Sleep"
        },
        editable = false
    },
    dashes = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 10
    }
}

return playerInventoryTrigger