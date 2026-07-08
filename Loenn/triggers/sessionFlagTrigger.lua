local sessionFlagTrigger = {}

sessionFlagTrigger.name = "DZ/SessionFlagTrigger"
sessionFlagTrigger.depth = 0
sessionFlagTrigger.placements = {
    name = "session_flag_trigger",
    data = {
        sessionFlag = "sample_trigger_0",
        flagState = true,
        triggerOnce = true,
        requiredFlag = "",
        requiredFlagState = true,
        flagAction = "SetValue",
        triggerMode = "OnEnter",
        sampleProperty = 0
    }
}

return sessionFlagTrigger
