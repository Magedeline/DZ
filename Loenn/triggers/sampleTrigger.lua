local sampleTrigger = {
    name = "MaggyHelper/SampleTrigger",
    placements = {
        {
            name = "SessionFlagTriggerLegacy",
            data = {
                width = 8,
                height = 8,
                sessionFlag = "",
                flagState = true,
                flagAction = "SetValue",
                triggerMode = "OnEnter",
                triggerOnce = true,
                requiredFlag = "",
                requiredFlagState = true,
                sampleProperty = 0,
            },
        },
    },
}

sampleTrigger.fieldInformation = {
    sessionFlag = { fieldType = "string" },
    flagState = { fieldType = "boolean" },
    flagAction = { fieldType = "string", options = { "SetValue", "Toggle" }, editable = false },
    triggerMode = { fieldType = "string", options = { "OnEnter", "OnLeave" }, editable = false },
    triggerOnce = { fieldType = "boolean" },
    requiredFlag = { fieldType = "string" },
    requiredFlagState = { fieldType = "boolean" },
    sampleProperty = { fieldType = "integer" },
}

sampleTrigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "sessionFlag",
    "flagAction",
    "flagState",
    "triggerMode",
    "triggerOnce",
    "requiredFlag",
    "requiredFlagState",
    "sampleProperty",
}

return sampleTrigger