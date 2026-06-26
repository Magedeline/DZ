local darkernerFountain = {}

darkernerFountain.name = "DZ/DarkernerFountain"
darkernerFountain.depth = 0
darkernerFountain.texture = "objects/DZ/darkerner_fountain"

darkernerFountain.placements = {
    name = "darkerner_fountain",
    data = {
        fountainType = "Shadow",
        requiresFlag = "",
        soundEffect = "event:/game/general/seed_poof",
        autoActivate = false,
        transformsPlayer = false,
        persistentEffect = false,
        particleCount = 30,
        activationRadius = 64,
        intensity = 1,
        duration = 5
    }
}

return darkernerFountain