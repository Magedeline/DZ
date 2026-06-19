local HopesAndDreamsBlock = {}

HopesAndDreamsBlock.name = "DZ/HopesAndDreamsBlock"
HopesAndDreamsBlock.placements = {
    {
        name = "main",
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
        fieldType = "color",
        description = "Primary color for block particles (default: Gold FFD700)"
    },
    secondaryColor = {
        fieldType = "color",
        description = "Secondary color - Kirby's pink (default: FF69B4)"
    },
    tertiaryColor = {
        fieldType = "color",
        description = "Tertiary color - Madeline's hair (default: FF4500)"
    },
    fastMoving = {
        fieldType = "boolean",
        description = "Whether the block moves quickly between nodes"
    },
    oneUse = {
        fieldType = "boolean",
        description = "Whether the block destroys after one use"
    },
    below = {
        fieldType = "boolean",
        description = "Render below player (higher depth value)"
    },
    showStars = {
        fieldType = "boolean",
        description = "Show star-shaped particles"
    }
}

HopesAndDreamsBlock.fieldOrder = {
    "x", "y", "width", "height",
    "primaryColor",
    "secondaryColor",
    "tertiaryColor",
    "showStars",
    "fastMoving",
    "oneUse",
    "below"
}

HopesAndDreamsBlock.nodeLimits = {0, 1}
HopesAndDreamsBlock.nodeLineRenderType = "line"

-- Visual representation in editor
HopesAndDreamsBlock.fillColor = {1.0, 0.84, 0.0, 0.3}  -- Gold transparent
HopesAndDreamsBlock.borderColor = {1.0, 0.41, 0.71, 0.8}  -- Pink border

function HopesAndDreamsBlock.depth(room, entity)
    return entity.below and 5000 or -11000
end

return HopesAndDreamsBlock
