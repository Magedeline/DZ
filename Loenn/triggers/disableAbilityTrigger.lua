local disableAbilityTrigger = {}

disableAbilityTrigger.name = "DZ/DisableAbilityTrigger"
disableAbilityTrigger.depth = 0
disableAbilityTrigger.placements = {
    name = "disable_ability_trigger",
    data = {
        disableDash = false,
        disableClimb = false,
        disableFloat = false,
        disableInhale = false,
        disableGrab = false
    }
}

return disableAbilityTrigger
