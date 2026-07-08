local playerTrigger = {}

playerTrigger.name = "DZ/PlayerTrigger"
playerTrigger.depth = 0
playerTrigger.placements = {
    name = "player_trigger",
    data = {
        triggerOnEnter = true,
        triggerOnExit = true,
        onEnterFlag = "",
        onExitFlag = "",
        setFlagState = true,
        triggerOnce = false,
        requiredFlag = "",
        onEnterAction = "None",
        onExitAction = "None",
        kirbyPower = "None",
        maxDashes = 3,
        inventoryDashes = 1,
        inventoryDreamDash = false,
        inventoryNoRefills = false
    }
}

return playerTrigger
