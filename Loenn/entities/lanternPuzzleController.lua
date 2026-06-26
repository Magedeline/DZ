local lanternPuzzleController = {}

lanternPuzzleController.name = "DZ/LanternPuzzleController"
lanternPuzzleController.depth = 0
lanternPuzzleController.texture = "objects/DZ/lantern_puzzle_controller"

lanternPuzzleController.placements = {
    name = "lantern_puzzle_controller",
    data = {
        lanternId = "",
        startLit = false,
        lightRadius = 80,
        flickerIntensity = 0.1
    }
}

return lanternPuzzleController