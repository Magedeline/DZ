local utils = require("utils")
local fakeTilesHelper = require("helpers.fake_tiles")

local charamovinglavaBlock = {}

charamovinglavaBlock.name = "DZ/MovingLavaBlock"
charamovinglavaBlock.depth = 0
charamovinglavaBlock.nodeLineRenderType = "line"
charamovinglavaBlock.nodeLimits = {1, 1}
charamovinglavaBlock.fieldInformation = {
    nodeIndex = {
        fieldType = "integer",
    }
}
charamovinglavaBlock.placements = {
    {
        name = "main",
        data = {
        nodeIndex = 0,
        width = 8,
        height = 8
        }
    }
}

function charamovinglavaBlock.nodeRectangle(room, entity, node)
    return utils.rectangle(node.x or 0, node.y or 0, entity.width or 8, entity.height or 8)
end

return charamovinglavaBlock