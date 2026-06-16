local timerStartTrigger = {}
timerStartTrigger.name = "DZ/TimerStartTrigger"
timerStartTrigger.placements = {
    { name = "TimerStartTrigger", data = { width = 16, height = 16, timerId = "timer_1", duration = 60.0, showDisplay = true } }
}
timerStartTrigger.fieldInformation = {
    timerId = { fieldType = "string" },
    duration = { fieldType = "number", minimumValue = 1.0 },
    showDisplay = { fieldType = "boolean" }
}
timerStartTrigger.fieldOrder = { "x", "y", "width", "height", "timerId", "duration", "showDisplay" }
return timerStartTrigger
