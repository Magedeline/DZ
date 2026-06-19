-- Movement & Traversal Entity Plugins for Lönn

local gravityFlipPlatform = {}
gravityFlipPlatform.name = "DZ/GravityFlipPlatform"
gravityFlipPlatform.depth = -10
gravityFlipPlatform.fillColor = {0.4, 0.2, 0.8, 0.4}
gravityFlipPlatform.borderColor = {0.6, 0.3, 1.0, 0.8}
gravityFlipPlatform.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 8,
            cooldown = 2.0,
            togglable = true
        }
    }
}
gravityFlipPlatform.fieldInformation = {
    cooldown = { fieldType = "number", minimumValue = 0.0 },
    togglable = { fieldType = "boolean" }
}
gravityFlipPlatform.fieldOrder = { "x", "y", "width", "height", "cooldown", "togglable" }

return gravityFlipPlatform
