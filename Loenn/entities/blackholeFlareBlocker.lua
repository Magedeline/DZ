local blackholeFlareBlocker = {}

blackholeFlareBlocker.name = "DZ/BlackholeFlareBlocker"
blackholeFlareBlocker.depth = 0
blackholeFlareBlocker.texture = "objects/DZ/DZ/DZ/blackhole_flare_blocker"

blackholeFlareBlocker.placements = {
    name = "blackhole_flare_blocker",
    data = {
        behavior = "Stop",
        blockerColor = "8B00FF",
        affectsSideway = true,
        affectsRiser = true,
        visualEffect = true,
        width = 16,
        height = 16
    }
}

return blackholeFlareBlocker