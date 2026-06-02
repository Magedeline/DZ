local charaZoomAppearTrigger = {}

charaZoomAppearTrigger.name = "MaggyHelper/CharaZoomAppearTrigger"

charaZoomAppearTrigger.placements = {
    {
        name = "chara_zoom_appear",
        data = {
            width = 16,
            height = 16,
            targetZoom = 2.0,
            zoomSpeed = 2.0,
            onlyOnce = true,
            affectChara = true,
            affectBadeline = true,
            showOnEnter = true,
            hideOnLeave = true,
            resetZoomOnLeave = true
        }
    }
}

charaZoomAppearTrigger.fieldInformation = {
    targetZoom = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },
    zoomSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 20.0
    },
    onlyOnce = {
        fieldType = "boolean"
    },
    affectChara = {
        fieldType = "boolean"
    },
    affectBadeline = {
        fieldType = "boolean"
    },
    showOnEnter = {
        fieldType = "boolean"
    },
    hideOnLeave = {
        fieldType = "boolean"
    },
    resetZoomOnLeave = {
        fieldType = "boolean"
    }
}

charaZoomAppearTrigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "targetZoom",
    "zoomSpeed",
    "onlyOnce",
    "affectChara",
    "affectBadeline",
    "showOnEnter",
    "hideOnLeave",
    "resetZoomOnLeave"
}

return charaZoomAppearTrigger
