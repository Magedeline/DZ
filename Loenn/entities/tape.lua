local tape = {}

tape.name = "DZ/Tape"
tape.depth = -1000000
tape.nodeLineRenderType = "line"
tape.texture = "collectables/DZ/tape/idle00"
tape.nodeLimits = {2, 2}
tape.placements = {
    name = "tape",
    data = {
        spritePath = "collectables/cassette/",
        menuSprite = "collectables/tape",
        particleColor = "FF9CCF",
        cSideToUnlock = "",
        unlockText = "",
        glowStrength = 1.0,
        bloomStrength = 0.8,
        wiggleIntensity = 0.35,
        floatSpeed = 2.0,
        floatRange = 2.0,
        previewParamValue = -1.0
    }
}

return tape