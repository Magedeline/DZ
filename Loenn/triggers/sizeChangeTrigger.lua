local sizeChangeTrigger = {}
sizeChangeTrigger.name = "DZ/SizeChangeTrigger"
sizeChangeTrigger.placements = {
    { name = "shrink", data = { width = 16, height = 16, scaleFactor = 0.5 } },
    { name = "grow", data = { width = 16, height = 16, scaleFactor = 2.0 } }
}
sizeChangeTrigger.fieldInformation = {
    scaleFactor = { fieldType = "number", minimumValue = 0.1, maximumValue = 4.0 }
}
sizeChangeTrigger.fieldOrder = { "x", "y", "width", "height", "scaleFactor" }
return sizeChangeTrigger
