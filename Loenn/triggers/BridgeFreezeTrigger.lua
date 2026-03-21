local BridgeFreezeTrigger = {}
BridgeFreezeTrigger.name = "MaggyHelper/BridgeFreezeTrigger"
BridgeFreezeTrigger.placements = {
    { name = "default", data = { width = 120, height = 80, freezeStrength = 0.001 } },
    { name = "slow_motion", data = { width = 120, height = 80, freezeStrength = 0.1 } }
}
BridgeFreezeTrigger.fieldInformation = {
    freezeStrength = { fieldType = "number", minimumValue = 0.001, maximumValue = 1.0 }
}
BridgeFreezeTrigger.fieldOrder = { "x", "y", "width", "height", "freezeStrength" }
return BridgeFreezeTrigger
