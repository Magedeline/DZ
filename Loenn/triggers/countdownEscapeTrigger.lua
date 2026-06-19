local countdownEscapeTrigger = {}
countdownEscapeTrigger.name = "DZ/CountdownEscapeTrigger"
countdownEscapeTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, countdown = 60.0, targetRoom = "", warningTime = 10.0, onlyOnce = true } }
}
countdownEscapeTrigger.fieldInformation = {
    countdown = { fieldType = "number", minimumValue = 5.0 },
    targetRoom = { fieldType = "string" },
    warningTime = { fieldType = "number", minimumValue = 1.0 },
    onlyOnce = { fieldType = "boolean" }
}
countdownEscapeTrigger.fieldOrder = { "x", "y", "width", "height", "countdown", "targetRoom", "warningTime", "onlyOnce" }
return countdownEscapeTrigger
