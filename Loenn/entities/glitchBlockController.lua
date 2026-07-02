local glitchBlockController = {}

glitchBlockController.name = "DZ/GlitchBlockController"
glitchBlockController.depth = 0
glitchBlockController.texture = "objects/DZ/DZ/DZ/glitch_block_controller"

glitchBlockController.placements = {
    name = "glitch_block_controller",
    data = {
        isPattern = false,
        stability = 0.7,
        glitchInterval = 3,
        visibleTime = 2,
        invisibleTime = 1
    }
}

return glitchBlockController