local warpStarReturnController = {}

warpStarReturnController.name = "DZ/WarpStarReturnController"
warpStarReturnController.depth = -10001
warpStarReturnController.texture = "objects/DZ/warpstars/idle00"

warpStarReturnController.placements = {
    name = "warp_star_return_controller",
    data = {
        returnRoom = "",
        returnSpawnX = 0,
        returnSpawnY = 0,
        phase2ActivateFlag = "ch21_els_termina_phase2_active",
        autoListen = true
    }
}

warpStarReturnController.fieldInformation = {
    returnSpawnX = {
        fieldType = "integer"
    },
    returnSpawnY = {
        fieldType = "integer"
    }
}

return warpStarReturnController
