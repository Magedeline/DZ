local timePlatform = {}

timePlatform.name = "DZ/TimePlatform"
timePlatform.depth = 0
timePlatform.fillColor = {0.3, 0.5, 0.7, 0.4}
timePlatform.borderColor = {0.4, 0.6, 0.9, 0.7}
timePlatform.placements = {
    {
        name = "past",
        data = { width = 32, height = 8, timeEra = "past", flagName = "time_state_future" }
    },
    {
        name = "future",
        data = { width = 32, height = 8, timeEra = "future", flagName = "time_state_future" }
    }
}
timePlatform.fieldInformation = {
    timeEra = { options = { "past", "future" }, editable = false },
    flagName = { fieldType = "string" }
}
timePlatform.fieldOrder = { "x", "y", "width", "height", "timeEra", "flagName" }

return timePlatform
