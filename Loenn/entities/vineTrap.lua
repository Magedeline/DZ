local vineTrap = {}
vineTrap.name = "DZ/VineTrap"
vineTrap.depth = 0
vineTrap.placements = {
    { name = "VineTrap", data = { trapRadius = 24.0, holdTime = 1.5, retractSpeed = 100.0 } },
    { name = "VineTrap_fast", data = { trapRadius = 32.0, holdTime = 0.8, retractSpeed = 200.0 } }
}
vineTrap.fieldInformation = {
    trapRadius = { fieldType = "number", minimumValue = 10.0 },
    holdTime = { fieldType = "number", minimumValue = 0.1 },
    retractSpeed = { fieldType = "number", minimumValue = 20.0 }
}
vineTrap.fieldOrder = { "x", "y", "trapRadius", "holdTime", "retractSpeed" }
return vineTrap
