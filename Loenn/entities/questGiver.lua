local questGiver = {}
questGiver.name = "DZ/QuestGiver"
questGiver.depth = -100
questGiver.justification = {0.5, 1.0}
questGiver.texture = "characters/oldlady/idle00"
questGiver.placements = {
    { name = "QuestGiver", data = { questId = "quest_1", dialogId = "", completionFlag = "quest_1_done", rewardType = "ability" } }
}
questGiver.fieldInformation = {
    questId = { fieldType = "string" },
    dialogId = { fieldType = "string" },
    completionFlag = { fieldType = "string" },
    rewardType = { fieldType = "string", options = { "ability", "item", "cosmetic", "health" }, editable = true }
}
questGiver.fieldOrder = { "x", "y", "questId", "dialogId", "completionFlag", "rewardType" }
return questGiver
