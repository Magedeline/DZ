local wishOrb = {}

wishOrb.name = "DZ/WishOrb"
wishOrb.depth = 0
wishOrb.texture = "objects/DZ/wish_orb"

wishOrb.placements = {
    name = "wish_orb",
    data = {
        dialoguePrefix = "WISH_ALTAR",
        canInteract = true,
        requiredHearts = 0
    }
}

return wishOrb