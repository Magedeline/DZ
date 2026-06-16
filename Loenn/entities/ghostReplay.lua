local ghostReplay = {}
ghostReplay.name = "DZ/GhostReplay"
ghostReplay.depth = -100
ghostReplay.justification = {0.5, 1.0}
ghostReplay.texture = "characters/Kglobal::Player/idle00"
ghostReplay.placements = {
    { name = "normal", data = { recordOnFlag = "record_ghost", replayOnFlag = "replay_ghost", ghostAlpha = 0.5 } }
}
ghostReplay.fieldInformation = {
    recordOnFlag = { fieldType = "string" },
    replayOnFlag = { fieldType = "string" },
    ghostAlpha = { fieldType = "number", minimumValue = 0.1, maximumValue = 1.0 }
}
ghostReplay.fieldOrder = { "x", "y", "recordOnFlag", "replayOnFlag", "ghostAlpha" }
return ghostReplay
