local dialogTrigger = {}

dialogTrigger.name = "DZ/DialogTrigger"
dialogTrigger.depth = 0
dialogTrigger.placements = {
    name = "dialog_trigger",
    data = {
        dialogKey = "DIALOG_DEFAULT",
        triggerOnce = true,
        requireInteraction = false,
        npcName = ""
    }
}

return dialogTrigger
