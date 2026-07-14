local companionNPC = {}

companionNPC.name = "DZ/CompanionNPC"
companionNPC.depth = 0
companionNPC.texture = "objects/DZ/companion_npc"

companionNPC.placements = {
    name = "companion_npc",
    data = {
        companionType = "waddle_dee",
        sprite = "",
        canPressSwitch = true,
        followSpeed = 100,
        followDistance = 30
    }
}

return companionNPC