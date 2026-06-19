local ralseiDummyAttacherTrigger = {}

ralseiDummyAttacherTrigger.name = "DZ/RalseiDummyAttacherTrigger"
ralseiDummyAttacherTrigger.placements = {
    {
        name = "main",
        data = {
        hoverOffsetX = 20,
        hoverOffsetY = -10,
        hoverSpeed = 2.0,
        hoverAmplitude = 4.0,
        once = true,
        removeOnExit = false
        }
    }
}

ralseiDummyAttacherTrigger.fieldInformation = {
    hoverOffsetX = {
        fieldType = "number",
        minimumValue = -100,
        maximumValue = 100
    },
    hoverOffsetY = {
        fieldType = "number",
        minimumValue = -100,
        maximumValue = 100
    },
    hoverSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    hoverAmplitude = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 20.0
    },
    once = {
        fieldType = "boolean"
    },
    removeOnExit = {
        fieldType = "boolean"
    }
}

return ralseiDummyAttacherTrigger