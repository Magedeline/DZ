local virusCloud = {}

virusCloud.name = "DZ/VirusCloud"
virusCloud.depth = 0
virusCloud.texture = "objects/DZ/DZ/DZ/virus_cloud"

virusCloud.placements = {
    name = "virus_cloud",
    data = {
        health = 3,
        spreadRadius = 100,
        moveSpeed = 40,
        damageRate = 0.5
    }
}

return virusCloud