local kirbyPuffRefill = {}

kirbyPuffRefill.name = "DZ/KirbyPuffRefill"
kirbyPuffRefill.depth = 0
kirbyPuffRefill.texture = "objects/DZ/DZ/DZ/kirby_puff_refill"

kirbyPuffRefill.placements = {
    name = "kirby_puff_refill",
    data = {
        spriteVariant = "auto",
        oneUse = false,
        breakEvenWhenFull = false,
        puffCount = 3,
        respawnTime = 2.5
    }
}

return kirbyPuffRefill
