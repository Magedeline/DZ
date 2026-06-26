local levelGlitchTrigger = {}

levelGlitchTrigger.name = "DZ/LevelGlitchTrigger"
levelGlitchTrigger.depth = 0
levelGlitchTrigger.placements = {
    name = "level_glitch_trigger",
    data = {
        NewRoomId = "",
        Delay = false,
        Time = 0,
        differentSpawn = false,
        playerX = 0,
        playerY = 0,
        DeleteAfterEnter = false,
        noMusicOnTeleport = false,
        lowPass = false,
        lowPassValue = 0
    }
}

return levelGlitchTrigger
