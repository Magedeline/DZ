local kirbyPlayerTrigger = {}

kirbyPlayerTrigger.name = "DZ/Kirby_Player_Trigger"
kirbyPlayerTrigger.depth = 0
kirbyPlayerTrigger.placements = {
    name = "kirby_player_trigger",
    data = {
        activationType = "OnEnter",
        transformationType = "Animated",
        oneUse = false,
        transformAnimation = "transform",
        transformDuration = 1,
        preserveVelocity = true,
        requiredFlag = "",
        playSound = true,
        initialPower = "None"
    }
}

return kirbyPlayerTrigger
