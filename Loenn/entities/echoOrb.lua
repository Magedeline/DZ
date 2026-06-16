local echoOrb = {}
echoOrb.name = "DZ/EchoOrb"
echoOrb.depth = -100
echoOrb.placements = {
    { name = "normal", data = { revealRadius = 80.0, revealDuration = 3.0 } }
}
echoOrb.fieldInformation = {
    revealRadius = { fieldType = "number", minimumValue = 20.0 },
    revealDuration = { fieldType = "number", minimumValue = 0.5 }
}
echoOrb.fieldOrder = { "x", "y", "revealRadius", "revealDuration" }
return echoOrb
