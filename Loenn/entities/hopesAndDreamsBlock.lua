local HopesAndDreamsBlock = {}

HopesAndDreamsBlock.name = "DZ/HopesAndDreamsBlock"
HopesAndDreamsBlock.placements = {
    {
        name = "HopesAndDreamsBlock",
        data = {
            width = 16,
            height = 16,
            fastMoving = false,
            oneUse = false,
            below = false,
            primaryColor = "FFD700",
            secondaryColor = "FF69B4",
            tertiaryColor = "FF4500",
            showStars = true
        }
    }
}

HopesAndDreamsBlock.fieldInformation = {
    primaryColor = {
        fieldType = "color"
    },
    secondaryColor = {
        fieldType = "color"
    },
    tertiaryColor = {
        fieldType = "color"
    }
}

HopesAndDreamsBlock.nodeLimits = {0, 1}
HopesAndDreamsBlock.nodeLineRenderType = "line"

return HopesAndDreamsBlock
