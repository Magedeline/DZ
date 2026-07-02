local kirbyPuffJumpRefill = {}

kirbyPuffJumpRefill.name = "DZ/KirbyPuffJumpRefill"
kirbyPuffJumpRefill.depth = 0
kirbyPuffJumpRefill.texture = "objects/DZ/DZ/DZ/kirby_puff_jump_refill"

kirbyPuffJumpRefill.placements = {
    name = "kirby_puff_jump_refill",
    data = {
        spriteVariant = "auto",
        oneUse = false,
        breakEvenWhenFull = false,
        puffCount = 3,
        respawnTime = 2.5
    }
}

return kirbyPuffJumpRefill
