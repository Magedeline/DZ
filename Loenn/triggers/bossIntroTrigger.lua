local bossIntroTrigger = {}
bossIntroTrigger.name = "DZ/BossIntroTrigger"
bossIntroTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, bossName = "", dialogId = "", musicEvent = "", flag = "" } }
}
bossIntroTrigger.fieldInformation = {
    bossName = { fieldType = "string" },
    dialogId = { fieldType = "string" },
    musicEvent = { fieldType = "string" },
    flag = { fieldType = "string" }
}
bossIntroTrigger.fieldOrder = { "x", "y", "width", "height", "bossName", "dialogId", "musicEvent", "flag" }
return bossIntroTrigger
