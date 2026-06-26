local playerAnimPrefixAddOnTrigger = {}

playerAnimPrefixAddOnTrigger.name = "DZ/PlayerAnimPrefixAddOnTrigger"
playerAnimPrefixAddOnTrigger.depth = 0
playerAnimPrefixAddOnTrigger.placements = {
    name = "player_anim_prefix_add_on_trigger",
    data = {
        animPrefixAddOn = "",
        characterId = "",
        characterAnimPrefix = "",
        revertOnLeave = false,
        useCharacterAnimations = true
    }
}

return playerAnimPrefixAddOnTrigger