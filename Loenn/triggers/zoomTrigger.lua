local zoomTrigger = {}
zoomTrigger.name = "DZ/ZoomTrigger"
zoomTrigger.placements = {
    { name = "zoom_in", data = { width = 16, height = 16, targetZoom = 2.0, zoomDuration = 1.0 } },
    { name = "zoom_out", data = { width = 16, height = 16, targetZoom = 0.5, zoomDuration = 1.0 } }
}
zoomTrigger.fieldInformation = {
    targetZoom = { fieldType = "number", minimumValue = 0.1, maximumValue = 5.0 },
    zoomDuration = { fieldType = "number", minimumValue = 0.1 }
}
zoomTrigger.fieldOrder = { "x", "y", "width", "height", "targetZoom", "zoomDuration" }
return zoomTrigger
