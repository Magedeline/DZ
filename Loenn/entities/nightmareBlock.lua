local nightmareBlock = {}

nightmareBlock.name = "DZ/NightmareBlock"
nightmareBlock.fillColor = {0.0, 0.0, 0.0}
nightmareBlock.borderColor = {1.0, 1.0, 1.0}
nightmareBlock.nodeLineRenderType = "line"
nightmareBlock.nodeLimits = {0, 1}
nightmareBlock.placements = {
    name = "nightmare_block",
    data = {
        fastMoving = false,
        below = false,
        oneUse = false,
        width = 8,
        height = 8
    }
}

function nightmareBlock.depth(room, entity)
    return entity.below and 5000 or -11000
end

return nightmareBlock