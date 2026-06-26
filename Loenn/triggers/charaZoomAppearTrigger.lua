local charaZoomAppearTrigger = {}

charaZoomAppearTrigger.name = "DZ/CharaZoomAppearTrigger"
charaZoomAppearTrigger.depth = 0
charaZoomAppearTrigger.placements = {
    name = "chara_zoom_appear_trigger",
    data = {
        onlyOnce = true,
        affectChara = true,
        affectBadeline = true,
        showOnEnter = true,
        hideOnLeave = true,
        resetZoomOnLeave = true,
        targetZoom = 2,
        zoomSpeed = 2
    }
}

return charaZoomAppearTrigger