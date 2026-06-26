local bellTrigger = {}

bellTrigger.name = "DZ/BellTrigger"
bellTrigger.depth = 0
bellTrigger.placements = {
    name = "bell_trigger",
    data = {
        canPlayerRing = true,
        maxRings = 3,
        shockwaveSpeed = 200,
        shockwaveRadius = 300,
        cooldownTime = 2
    }
}

return bellTrigger