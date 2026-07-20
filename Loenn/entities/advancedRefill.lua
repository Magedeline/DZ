local advancedRefill = {}

advancedRefill.name = "DZ/AdvancedRefill"
advancedRefill.depth = 0
advancedRefill.texture = "objects/DZ/advanced_refill"

advancedRefill.placements = {
    name = "advanced_refill",
    data = {
        oneUse = false,
        respectInventoryLimits = true,
        dashCount = 1,
        refillStamina = true
    }
}

return advancedRefill