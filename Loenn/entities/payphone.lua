local payphone = {}

payphone.name = "DZ/Payphone"
payphone.depth = 0
payphone.texture = "objects/DZ/payphone"

payphone.placements = {
    name = "payphone",
    data = {
        dreamDialogId = "DZ_CH2_DREAM_PHONECALL_TRAP",
        awakeDialogId = "DZ_CH2_CALLING_KIRBY_ENDING",
        flagToSet = "",
        onlyOnce = true
    }
}

return payphone