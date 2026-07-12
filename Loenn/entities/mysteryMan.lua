local mysteryMan = {}

mysteryMan.name = "DZ/MysteryMan"
mysteryMan.depth = 100
mysteryMan.texture = "characters/DZ/mysteryman/00"

mysteryMan.placements = {
    name = "mystery_man",
    data = {
        dialogKey = "DZ_MYSTERYMAN_GASTER",
        audioEvent = "event:/DZ/game/08_edge/mysterygo",
        flagName = "",
        onlyOnce = true
    }
}

return mysteryMan
