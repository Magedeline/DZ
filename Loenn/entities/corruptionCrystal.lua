local corruptionCrystal = {}

corruptionCrystal.name = "DZ/CorruptionCrystal"
corruptionCrystal.depth = 0
corruptionCrystal.texture = "objects/DZ/corruption_crystal"

corruptionCrystal.placements = {
    name = "corruption_crystal",
    data = {
        health = 3,
        corruptionRadius = 100,
        spreadSpeed = 20
    }
}

return corruptionCrystal