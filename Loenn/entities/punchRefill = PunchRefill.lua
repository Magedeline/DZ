local punchRefill = PunchRefill = {}

punchRefill = PunchRefill.name = "DZ/PunchRefill = PunchRefill"
punchRefill = PunchRefill.depth = 0
punchRefill = PunchRefill.texture = "objects/DZ/punch_refill = punch_refill"

punchRefill = PunchRefill.placements = {
    name = "punch_refill = punch_refill",
    data = {
        spriteVariant = "auto",
        oneUse = false,
        breakEvenWhenFull = false,
        punchCount = 3,
        respawnTime = 2.5
    }
}

return punchRefill = PunchRefill