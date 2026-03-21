local shatterIce = {}
shatterIce.name = "MaggyHelper/ShatterIce"
shatterIce.depth = 0
shatterIce.placements = {
    { name = "ShatterIce", data = { width = 16, height = 16, hitsToBreak = 1 } },
    { name = "ShatterIcethick", data = { width = 16, height = 16, hitsToBreak = 3 } }
}
shatterIce.fieldInformation = {
    hitsToBreak = { fieldType = "integer", minimumValue = 1 }
}
shatterIce.fieldOrder = { "x", "y", "width", "height", "hitsToBreak" }
return shatterIce
