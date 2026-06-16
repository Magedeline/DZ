local payphone = {}

payphone.name = "DZ/Payphone"
payphone.depth = 8999
payphone.justification = {0.5, 1.0}
payphone.texture = "scenery/payphone"

payphone.placements = {
    {
        name = "payphone_dream",
        data = {
            dreamDialogId = "DZ_CH2_DREAM_PHONECALL_TRAP",
            awakeDialogId = "DZ_CH2_AWAKE_PHONECALL_TRAP",
            flagToSet = "",
            onlyOnce = true
        }
    },
    {
        name = "payphone_custom",
        data = {
            dreamDialogId = "DZ_CH2_DREAM_PHONECALL_TRAP",
            awakeDialogId = "DZ_CH2_AWAKE_PHONECALL_TRAP",
            flagToSet = "",
            onlyOnce = true
        }
    }
}

payphone.fieldInformation = {
    dreamDialogId = {
        fieldType = "string",
        description = "Dialog ID to show when using the payphone in dream state"
    },
    awakeDialogId = {
        fieldType = "string",
        description = "Dialog ID to show when using the payphone in awake state"
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