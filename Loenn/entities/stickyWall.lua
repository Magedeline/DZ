local stickyWall = {}

stickyWall.name = "DZ/StickyWall"
stickyWall.depth = 0
stickyWall.fillColor = {0.2, 0.6, 0.2, 0.3}
stickyWall.borderColor = {0.3, 0.8, 0.3, 0.6}
stickyWall.placements = {
    {
        name = "main",
        data = { width = 8, height = 32, stickDuration = 5.0, infiniteStick = false }
    },
    {
        name = "StickyWall_infinite",
        data = { width = 8, height = 32, stickDuration = 5.0, infiniteStick = true }
    }
}
stickyWall.fieldInformation = {
    stickDuration = { fieldType = "number", minimumValue = 0.0 },
    infiniteStick = { fieldType = "boolean" }
}
stickyWall.fieldOrder = { "x", "y", "width", "height", "stickDuration", "infiniteStick" }

return stickyWall
