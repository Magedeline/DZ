local throneRoomController = {}

throneRoomController.name = "DZ/ThroneRoomController"
throneRoomController.depth = 0
throneRoomController.texture = "objects/DZ/DZ/DZ/throne_room_controller"

throneRoomController.placements = {
    name = "throne_room_controller",
    data = {
        bossEntity = "DZ/KingTitanBoss",
        cutsceneId = "DZ_CH15_ROARING_TITAN_KING_BATTLE",
        autoActivate = false,
        activationRadius = 100
    }
}

return throneRoomController