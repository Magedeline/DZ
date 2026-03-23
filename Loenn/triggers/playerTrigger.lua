local playerTrigger = {}

playerTrigger.name = "MaggyHelper/PlayerTrigger"

playerTrigger.placements = {
    {
        name = "default",
        data = {
            width = 16,
            height = 16,
            triggerOnEnter = true,
            triggerOnExit = true,
            onEnterAction = "None",
            onExitAction = "None",
            onEnterFlag = "",
            onExitFlag = "",
            kirbyPower = "None",
            maxDashes = 3,
            setFlagState = true,
            triggerOnce = false,
            requiredFlag = ""
        }
    },
    {
        name = "enter_only",
        data = {
            width = 16,
            height = 16,
            triggerOnEnter = true,
            triggerOnExit = false,
            onEnterAction = "None",
            onExitAction = "None",
            onEnterFlag = "player_entered_trigger",
            onExitFlag = "",
            kirbyPower = "None",
            maxDashes = 3,
            setFlagState = true,
            triggerOnce = false,
            requiredFlag = ""
        }
    },
    {
        name = "exit_only",
        data = {
            width = 16,
            height = 16,
            triggerOnEnter = false,
            triggerOnExit = true,
            onEnterAction = "None",
            onExitAction = "None",
            onEnterFlag = "",
            onExitFlag = "player_exited_trigger",
            kirbyPower = "None",
            maxDashes = 3,
            setFlagState = true,
            triggerOnce = false,
            requiredFlag = ""
        }
    }
}

playerTrigger.fieldInformation = {
    triggerOnEnter = {
        fieldType = "boolean"
    },
    triggerOnExit = {
        fieldType = "boolean"
    },
    onEnterAction = {
        options = {
            "None",
            "EnableKirbyMode",
            "DisableKirbyMode",
            "SetKirbyPower",
            "SetMaxDashes"
        },
        editable = false
    },
    onExitAction = {
        options = {
            "None",
            "EnableKirbyMode",
            "DisableKirbyMode",
            "SetKirbyPower",
            "SetMaxDashes"
        },
        editable = false
    },
    onEnterFlag = {
        fieldType = "string"
    },
    onExitFlag = {
        fieldType = "string"
    },
    kirbyPower = {
        options = {
            "None",
            "Fire",
            "Ice",
            "Spark",
            "Sword",
            "Cutter",
            "Beam",
            "Stone",
            "Needle",
            "Parasol",
            "Wheel",
            "Bomb",
            "Fighter",
            "Suplex",
            "Ninja",
            "Mirror",
            "Hammer",
            "Knight",
            "Wing",
            "UFO",
            "Sleep"
        },
        editable = false
    },
    maxDashes = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    setFlagState = {
        fieldType = "boolean"
    },
    triggerOnce = {
        fieldType = "boolean"
    },
    requiredFlag = {
        fieldType = "string"
    }
}

playerTrigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "triggerOnEnter",
    "triggerOnExit",
    "onEnterAction",
    "onExitAction",
    "onEnterFlag",
    "onExitFlag",
    "kirbyPower",
    "maxDashes",
    "setFlagState",
    "triggerOnce",
    "requiredFlag"
}

return playerTrigger