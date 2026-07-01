local payphone = {}

payphone.name = "DZ/Payphone"
payphone.depth = 0
payphone.texture = "objects/payphone"

payphone.placements = {
    name = "payphone",
    data = {
        flagToSet = "payphone_activated",
        onlyOnce = true
    }
}

return payphone