local glitchBlock = {}

glitchBlock.name = "DZ/GlitchBlock"
glitchBlock.depth = 0
glitchBlock.texture = "objects/DZ/glitch_block"

glitchBlock.placements = {
    name = "glitch_block",
    data = {
        isPattern = false,
        stability = 0.7,
        glitchInterval = 3,
        visibleTime = 2,
        invisibleTime = 1
    }
}

return glitchBlock