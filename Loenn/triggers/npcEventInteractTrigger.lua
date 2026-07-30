local npcEventInteractTrigger = {}

npcEventInteractTrigger.name = "DZ/NpcEventInteractTrigger"
npcEventInteractTrigger.depth = 0
npcEventInteractTrigger.placements = {
	name = "npc_event_interact_trigger",
	data = {
		npcName = "NPC",
		dialogKey = "",
		csFlag = "",
		triggerFlag = "",
		requireFlag = "",
		onlyOnce = true,
		mode = "OnEnter"
	}
}

npcEventInteractTrigger.fieldInformation = {
	mode = {
		options = {"OnEnter", "OnInteract"},
		editable = false
	}
}

return npcEventInteractTrigger
