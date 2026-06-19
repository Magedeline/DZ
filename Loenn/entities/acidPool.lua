local acidPool = {}
acidPool.name = "DZ/AcidPool"
acidPool.depth = -50
acidPool.placements = {
    { name = "main", data = { width = 32, height = 8, riseSpeed = 0.0 } },
    { name = "rising", data = { width = 32, height = 8, riseSpeed = 20.0 } }
}
acidPool.fieldInformation = {
    riseSpeed = { fieldType = "number", minimumValue = 0.0 }
}
acidPool.fieldOrder = { "x", "y", "width", "height", "riseSpeed" }
return acidPool
