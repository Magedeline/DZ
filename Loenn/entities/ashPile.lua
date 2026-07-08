local ashPile = {}

ashPile.name = "DZ/AshPile"
ashPile.depth = 0
ashPile.texture = "objects/DZ/DZ/DZ/ash_pile"

ashPile.placements = {
    name = "ash_pile",
    data = {
        isClimbable = true,
        climbSlowFactor = 0.5,
        shiftInterval = 3
    }
}

return ashPile