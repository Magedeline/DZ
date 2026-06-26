local cardShark = {}

cardShark.name = "DZ/CardShark"
cardShark.depth = 0
cardShark.texture = "objects/DZ/card_shark"

cardShark.placements = {
    name = "card_shark",
    data = {
        health = 2,
        cardsPerThrow = 3,
        detectionRange = 180,
        throwInterval = 1.5,
        patrolDistance = 80
    }
}

return cardShark