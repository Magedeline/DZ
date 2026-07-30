local sessionFlagTrigger = {}

sessionFlagTrigger.name = "DZ/SessionFlagTrigger"
sessionFlagTrigger.depth = 0
sessionFlagTrigger.placements = {
    name = "session_flag_trigger",
    data = {
        sessionFlag = "",
        flagState = true,
        triggerOnce = true,
        requiredFlag = "",
        requiredFlagState = true,
        flagAction = "SetValue",
        triggerMode = "OnEnter"
    }
}

sessionFlagTrigger.fieldInformation = {
    flagAction = {
        options = { "SetValue", "Toggle" },
        editable = false
    },
    triggerMode = {
        options = { "OnEnter", "OnLeave" },
        editable = false
    }
}

return sessionFlagTrigger
