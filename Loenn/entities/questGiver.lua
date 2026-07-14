local questGiver = {}

questGiver.name = "DZ/QuestGiver"
questGiver.depth = 0
questGiver.texture = "objects/DZ/quest_giver"

questGiver.placements = {
    name = "quest_giver",
    data = {
        companionType = "waddle_dee",
        sprite = "",
        canPressSwitch = true,
        followSpeed = 100,
        followDistance = 30
    }
}

return questGiver