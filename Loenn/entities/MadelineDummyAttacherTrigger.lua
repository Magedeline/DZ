local madelineDummyAttacherTrigger = {}

madelineDummyAttacherTrigger.name = "DZ/MadelineDummyAttacherTrigger"
madelineDummyAttacherTrigger.placements = {
    {
        name = "main",
        data = {
        hoverOffsetX = -25,
        hoverOffsetY = -15,
        hoverSpeed = 2.5,
        hoverAmplitude = 5.0,
        once = true,
        removeOnExit = false
        }
    }
}

madelineDummyAttacherTrigger.fieldInformation = {
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

return madelineDummyAttacherTrigger