-- Loenn plugin for MaggyHelper - Boss Fight Trigger
local trigger = {}

trigger.name = "MaggyHelper/BossFightTrigger"
trigger.placements = {
    name = "BossFightTrigger",
    data = {
        width = 16,
        height = 16,
        bossType = "KirbyBoss",
        lockRoom = true,
        playMusic = true,
        bossMusic = "guid://{38e2f39c-382d-4136-86fd-e24520f3b71e}"
    }
}

trigger.fieldInformation = {
    bossType = {
        options = {
            "KirbyBoss",
            "DededeBoss",
            "MetaKnightBoss"
        },
        editable = false
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "bossType",
    "lockRoom",
    "playMusic",
    "bossMusic"
}

return trigger

