local timerStopTrigger = {}
timerStopTrigger.name = "MaggyHelper/TimerStopTrigger"
timerStopTrigger.placements = {
    { name = "TimerStopTrigger", data = { width = 16, height = 16, timerId = "timer_1" } }
}
timerStopTrigger.fieldInformation = {
    timerId = { fieldType = "string" }
}
timerStopTrigger.fieldOrder = { "x", "y", "width", "height", "timerId" }
return timerStopTrigger
