local torielStoveEntity = {}

torielStoveEntity.name = "DZ/TorielStoveEntity"
torielStoveEntity.depth = 0
torielStoveEntity.texture = "objects/DZ/toriel_stove_entity"

torielStoveEntity.placements = {
    name = "toriel_stove_entity",
    data = {
        dialogueId = "TORIEL_STOVE",
        canInteract = true,
        hasPie = true,
        healAmount = 3,
        cookDuration = 5
    }
}

return torielStoveEntity