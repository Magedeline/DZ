local magneticField = {}
magneticField.name = "DZ/MagneticField"
magneticField.depth = -50
magneticField.placements = {
    { name = "main", data = { width = 64, height = 64, polarity = "Attract", strength = 100.0 } },
    { name = "repel", data = { width = 64, height = 64, polarity = "Repel", strength = 100.0 } }
}
magneticField.fieldInformation = {
    polarity = { fieldType = "string", options = { "Attract", "Repel" }, editable = false },
    strength = { fieldType = "number", minimumValue = 10.0 }
}
magneticField.fieldOrder = { "x", "y", "width", "height", "polarity", "strength" }
return magneticField
