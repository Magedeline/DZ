local ruinsSentinel = {}

ruinsSentinel.name = "DZ/RuinsSentinel"
ruinsSentinel.depth = 0
ruinsSentinel.texture = "objects/DZ/ruins_sentinel"

ruinsSentinel.placements = {
    name = "ruins_sentinel",
    data = {
        health = 3,
        detectionRange = 150,
        attackRange = 60,
        moveSpeed = 50,
        patrolDistance = 100
    }
}

return ruinsSentinel