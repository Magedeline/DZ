local payphone = {}

payphone.name = "MaggyHelper/Payphone"
payphone.depth = 8999
payphone.justification = {0.5, 1.0}
payphone.texture = "scenery/payphone"

payphone.placements = {
    {
        name = "payphone",
        data = {
            dialogId = "CH2_DREAM_PHONECALL_TRAP",
            flagToSet = "",
            onlyOnce = true
        }
    }
}

payphone.fieldInformation = {
    dialogId = {
        fieldType = "string",
        description = "Dialog ID to show when using the payphone"
    },
    flagToSet = {
        fieldType = "string",
        description = "Optional flag to set when payphone is used"
    },
    onlyOnce = {
        fieldType = "boolean",
        description = "Whether the payphone can only be used once"
    }
}

return payphone