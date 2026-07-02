local tape = {}

tape.name = "DZ/Tape"
tape.depth = 0
tape.texture = "objects/DZ/DZ/DZ/tape"

tape.placements = {
    name = "tape",
    data = {
        spritePath = "collectables/cassette/",
        menuSprite = "collectables/tape",
        particleColor = "FF9CCF",
        2ToUnlock = "",
        unlockText = "",
        glowStrength = 1.0,
        bloomStrength = 0.8,
        wiggleIntensity = 0.35,
        floatSpeed = 2.0,
        floatRange = 2.0,
        previewParamValue = -1
    }
}

return tape