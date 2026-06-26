local ceremonyFlame = {}

ceremonyFlame.name = "DZ/CeremonyFlame"
ceremonyFlame.depth = 0
ceremonyFlame.texture = "objects/DZ/ceremony_flame"

ceremonyFlame.placements = {
    name = "ceremony_flame",
    data = {
        isSource = true,
        canSpread = true,
        spreadSpeed = 20,
        maxSpreadDistance = 200
    }
}

return ceremonyFlame