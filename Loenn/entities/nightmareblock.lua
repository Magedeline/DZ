local dreamBlock = {}

dreamBlock.name = "DZ/NightmareBlock"
dreamBlock.fillColor = {0.0, 0.0, 0.0}
dreamBlock.borderColor = {1.0, 1.0, 1.0}
dreamBlock.nodeLineRenderType = "line"
dreamBlock.nodeLimits = {0, 1}
dreamBlock.placements = {
    {
        name = "nightmare_block",
        alternativeName = "space_jam",
        data = {
            fastMoving = false,
            below = false,
            oneUse = false,
            active = false,
            escapeTime = 1.5,
            width = 8,
            height = 8
        }
    }
}

dreamBlock.fieldInformation = {
    escapeTime = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0,
        description = "Time in seconds player has to escape through the block before dying"
    },
    active = {
        fieldType = "boolean",
        description = "Whether the block is active (can be dream dashed through)"
    }
}

dreamBlock.fieldOrder = {
    "x", "y", "width", "height",
    "active",
    "escapeTime",
    "fastMoving",
    "oneUse",
    "below"
}

function dreamBlock.depth(room, entity)
    return entity.below and 5000 or -11000
end

-- Color changes based on active state
function dreamBlock.color(room, entity)
    if entity.active then
        return {0.8, 0.0, 0.0, 0.6}  -- Red when active (dangerous)
    else
        return {0.3, 0.3, 0.3, 0.5}  -- Gray when inactive
    end
end

return dreamBlock