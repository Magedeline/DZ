local punchRefill = {}

punchRefill.name = "DZ/PunchRefill"
punchRefill.depth = 0
punchRefill.texture = "objects/DZ/punch_refill"

punchRefill.placements = {
    name = "punch_refill",
    data = {
        spriteVariant = "auto",
        oneUse = false,
        breakEvenWhenFull = false,
        punchCount = 3,
        respawnTime = 2.5
    }
}

return punchRefill
