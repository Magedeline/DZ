local magmaPool = {}

magmaPool.name = "DZ/MagmaPool"
magmaPool.depth = 0
magmaPool.texture = "objects/DZ/DZ/DZ/magma_pool"

magmaPool.placements = {
    name = "magma_pool",
    data = {
        isInstantDeath = true,
        bubbleInterval = 0.5,
        eruptInterval = 3
    }
}

return magmaPool