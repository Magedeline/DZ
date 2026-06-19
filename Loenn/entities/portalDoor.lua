local portalDoor = {}

portalDoor.name = "DZ/PortalDoor"
portalDoor.depth = -500
portalDoor.texture = "objects/door/door00"
portalDoor.justification = {0.5, 1.0}
portalDoor.nodeLimits = {0, 1}
portalDoor.nodeLineRenderType = "line"
portalDoor.placements = {
    {
        name = "main",
        data = { portalId = "portal_A", color = "00ffff" }
    },
    {
        name = "portal_B",
        data = { portalId = "portal_B", color = "ff00ff" }
    }
}
portalDoor.fieldInformation = {
    portalId = { fieldType = "string" },
    color = { fieldType = "color" }
}
portalDoor.fieldOrder = { "x", "y", "portalId", "color" }

return portalDoor
