local judgmentBell = {}

judgmentBell.name = "DZ/JudgmentBell"
judgmentBell.depth = 0
judgmentBell.placements = {
    name = "judgment_bell",
    data = {
        canPlayerRing = true,
        maxRings = 3,
        shockwaveSpeed = 200,
        shockwaveRadius = 300,
        cooldownTime = 2
    }
}

return judgmentBell