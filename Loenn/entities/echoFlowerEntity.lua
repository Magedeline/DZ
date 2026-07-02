local echoFlowerEntity = {}

echoFlowerEntity.name = "DZ/EchoFlowerEntity"
echoFlowerEntity.depth = 0
echoFlowerEntity.texture = "objects/DZ/DZ/DZ/echo_flower_entity"

echoFlowerEntity.placements = {
    name = "echo_flower_entity",
    data = {
        maxEchoes = 3,
        echoDelay = 0.5,
        echoSpeed = 200,
        cooldownTime = 1
    }
}

return echoFlowerEntity