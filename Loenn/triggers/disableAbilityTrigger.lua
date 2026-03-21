local disableAbilityTrigger = {}
disableAbilityTrigger.name = "MaggyHelper/DisableAbilityTrigger"
disableAbilityTrigger.placements = {
    { name = "no_dash", data = { width = 32, height = 32, disableDash = true, disableGrab = false, disableJump = false } },
    { name = "no_grab", data = { width = 32, height = 32, disableDash = false, disableGrab = true, disableJump = false } }
}
disableAbilityTrigger.fieldInformation = {
    disableDash = { fieldType = "boolean" },
    disableGrab = { fieldType = "boolean" },
    disableJump = { fieldType = "boolean" }
}
disableAbilityTrigger.fieldOrder = { "x", "y", "width", "height", "disableDash", "disableGrab", "disableJump" }
return disableAbilityTrigger
