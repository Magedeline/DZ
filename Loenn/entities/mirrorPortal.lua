local mirrorPortal = {}
mirrorPortal.name = "DZ/MirrorPortal"
mirrorPortal.depth = -100
mirrorPortal.placements = {
    { name = "MirrorPortal", data = { portalId = "portal_a", linkedId = "portal_b", width = 16, height = 32 } }
}
mirrorPortal.fieldInformation = {
    portalId = { fieldType = "string" },
    linkedId = { fieldType = "string" }
}
mirrorPortal.fieldOrder = { "x", "y", "width", "height", "portalId", "linkedId" }
return mirrorPortal
