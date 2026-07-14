local superCoreBlock = {}

superCoreBlock.name = "DZ/SuperCoreBlock"
superCoreBlock.depth = 0
superCoreBlock.texture = "objects/DZ/super_core_block"

superCoreBlock.placements = {
    name = "super_core_block",
    data = {
        hotColor = "FF4500",
        coldColor = "00BFFF",
        superColor = "FFD700",
        requiresCoreMode = false,
        speedMultiplier = 3,
        launchRange = 400
    }
}

return superCoreBlock