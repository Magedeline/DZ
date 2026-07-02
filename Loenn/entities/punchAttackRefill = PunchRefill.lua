local punchAttackRefill = {}

punchAttackRefill.name = "DZ/PunchAttackRefill"
punchAttackRefill.depth = 0
punchAttackRefill.texture = "objects/DZ/DZ/DZ/punch_attack_refill"

punchAttackRefill.placements = {
    name = "punch_attack_refill",
    data = {
        spriteVariant = "auto",
        oneUse = false,
        breakEvenWhenFull = false,
        punchCount = 3,
        respawnTime = 2.5
    }
}

return punchAttackRefill
