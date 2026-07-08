local torielOven = {}

torielOven.name = "DZ/TorielOven"
torielOven.depth = 0
torielOven.texture = "objects/DZ/DZ/DZ/toriel_oven"

torielOven.placements = {
    name = "toriel_oven",
    data = {
        dialogueId = "TORIEL_STOVE",
        canInteract = true,
        hasPie = true,
        healAmount = 3,
        cookDuration = 5
    }
}

return torielOven