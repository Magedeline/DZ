local rewindCrystal = {}
rewindCrystal.name = "DZ/RewindCrystal"
rewindCrystal.depth = -100
rewindCrystal.placements = {
    { name = "RewindCrystal", data = { rewindDuration = 3.0 } },
    { name = "RewindCrystallong", data = { rewindDuration = 6.0 } }
}
rewindCrystal.fieldInformation = {
    rewindDuration = { fieldType = "number", minimumValue = 1.0 }
}
rewindCrystal.fieldOrder = { "x", "y", "rewindDuration" }
return rewindCrystal
