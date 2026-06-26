local punchAttackRefill = PunchRefill = {}

punchAttackRefill = PunchRefill.name = "DZ/PunchAttackRefill = PunchRefill"
punchAttackRefill = PunchRefill.depth = 0
punchAttackRefill = PunchRefill.texture = "objects/DZ/punch_attack_refill = punch_refill"

punchAttackRefill = PunchRefill.placements = {
    name = "punch_attack_refill = punch_refill",
    data = {
        spriteVariant = "auto",
        oneUse = false,
        breakEvenWhenFull = false,
        punchCount = 3,
        respawnTime = 2.5
    }
}

return punchAttackRefill = PunchRefill