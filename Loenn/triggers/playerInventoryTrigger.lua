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

return playerInventoryTrigger