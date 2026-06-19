local weatherChangeTrigger = {}
weatherChangeTrigger.name = "DZ/WeatherChangeTrigger"
weatherChangeTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, weatherType = "Rain", intensity = 0.5, transitionDuration = 2.0 } },
    { name = "snow", data = { width = 16, height = 16, weatherType = "Snow", intensity = 0.5, transitionDuration = 2.0 } },
    { name = "storm", data = { width = 16, height = 16, weatherType = "Storm", intensity = 1.0, transitionDuration = 1.0 } }
}
weatherChangeTrigger.fieldInformation = {
    weatherType = { fieldType = "string", options = { "None", "Rain", "Snow", "Storm", "Fog", "Ash" }, editable = false },
    intensity = { fieldType = "number", minimumValue = 0.0, maximumValue = 1.0 },
    transitionDuration = { fieldType = "number", minimumValue = 0.0 }
}
weatherChangeTrigger.fieldOrder = { "x", "y", "width", "height", "weatherType", "intensity", "transitionDuration" }
return weatherChangeTrigger
