local stormCloud = {}
stormCloud.name = "DZ/StormCloud"
stormCloud.depth = -200
stormCloud.placements = {
    { name = "StormCloud", data = { strikeInterval = 4.0, strikeRadius = 32.0, warningTime = 1.0 } },
    { name = "StormCloud_frequent", data = { strikeInterval = 2.0, strikeRadius = 48.0, warningTime = 0.5 } }
}
stormCloud.fieldInformation = {
    strikeInterval = { fieldType = "number", minimumValue = 0.5 },
    strikeRadius = { fieldType = "number", minimumValue = 10.0 },
    warningTime = { fieldType = "number", minimumValue = 0.1 }
}
stormCloud.fieldOrder = { "x", "y", "strikeInterval", "strikeRadius", "warningTime" }
return stormCloud
