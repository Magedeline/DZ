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
        bossMusic = "event:/music/lvl9/main"
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
