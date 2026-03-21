local cowboyTargetPracticeTrigger = {}

cowboyTargetPracticeTrigger.name = "MaggyHelper/CowboyTargetPracticeTrigger"
cowboyTargetPracticeTrigger.placements = {
    {
        name = "trigger",
        data = {
            width = 16,
            height = 16,
            requiredTargets = 10,
            timeLimit = 60.0,
            movingTargets = false,
            practiceType = "A",
            flagToCheck = ""
        }
    }
}

cowboyTargetPracticeTrigger.fieldInformation = {
    requiredTargets = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 50
    },
    timeLimit = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 300.0
    },
    movingTargets = {
        fieldType = "boolean"
    },
    practiceType = {
        fieldType = "string",
        options = {
            "A",
            "B",
            "C",
            "passed",
            "truely_passed"
        },
        editable = false
    },
    flagToCheck = {
        fieldType = "string"
    }
}

cowboyTargetPracticeTrigger.fieldOrder = {
    "x", "y", "width", "height",
    "requiredTargets",
    "timeLimit",
    "movingTargets",
    "practiceType",
    "flagToCheck"
}

return cowboyTargetPracticeTrigger