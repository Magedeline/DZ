local mainCharaVisionActor = {}

mainCharaVisionActor.name = "DZ/MainCharaVisionActor"
mainCharaVisionActor.depth = 0
mainCharaVisionActor.texture = "objects/DZ/mainDZ_CHara_vision_actor"

mainCharaVisionActor.placements = {
    name = "mainDZ_CHara_vision_actor",
    data = {
        facing = "down",
        clampToRoomBounds = true,
        driveCameraWhenControlled = true,
        playerControlled = false
    }
}

return mainCharaVisionActor