local companionNPC = {}
companionNPC.name = "MaggyHelper/CompanionNPC"
companionNPC.depth = -100
companionNPC.justification = {0.5, 1.0}
companionNPC.texture = "characters/kirby/idle00"

-- Map companion types to their sprite textures
function companionNPC.texture(room, entity)
    local companionTextures = {
        Bandana_Dee = "characters/kirby_npc/bandana_dee/idle00",
        Ribbon = "characters/kirby/idle00",
        Adeleine = "characters/kirby/idle00",
        Marx = "characters/kirby/idle00",
        Magolor = "characters/kirby_npc/magolor/idle00",
        Taranza = "characters/kirby/idle00",
        Susie = "characters/kirby/idle00",
        Francisca = "characters/kirby/idle00",
        Flamberge = "characters/kirby/idle00",
        Zan_Partizanne = "characters/kirby/idle00"
    }
    local companionType = entity.companionType or "Bandana_Dee"
    return companionTextures[companionType] or "characters/kirby/idle00"
end

companionNPC.placements = {
    { name = "normal", data = { companionType = "Bandana_Dee", followDistance = 24.0, canFight = true, dialogId = "" } }
}
companionNPC.fieldInformation = {
    companionType = { fieldType = "string", options = { "Bandana_Dee", "Ribbon", "Adeleine", "Marx", "Magolor", "Taranza", "Susie", "Francisca", "Flamberge", "Zan_Partizanne" }, editable = true },
    followDistance = { fieldType = "number", minimumValue = 8.0 },
    canFight = { fieldType = "boolean" },
    dialogId = { fieldType = "string" }
}
companionNPC.fieldOrder = { "x", "y", "companionType", "followDistance", "canFight", "dialogId" }
return companionNPC
