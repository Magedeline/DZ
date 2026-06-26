local kirbyPuffRefill = KirbyPuffJumpRefill = {}

kirbyPuffRefill = KirbyPuffJumpRefill.name = "DZ/KirbyPuffRefill = KirbyPuffJumpRefill"
kirbyPuffRefill = KirbyPuffJumpRefill.depth = 0
kirbyPuffRefill = KirbyPuffJumpRefill.texture = "objects/DZ/kirby_puff_refill = kirby_puff_jump_refill"

kirbyPuffRefill = KirbyPuffJumpRefill.placements = {
    name = "kirby_puff_refill = kirby_puff_jump_refill",
    data = {
        spriteVariant = "auto",
        oneUse = false,
        breakEvenWhenFull = false,
        puffCount = 3,
        respawnTime = 2.5
    }
}

return kirbyPuffRefill = KirbyPuffJumpRefill