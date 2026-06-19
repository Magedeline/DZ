local utils = require("utils")
local fakeTilesHelper = require("helpers.fake_tiles")

local charamovingBlock = {}

charamovingBlock.name = "DZ/CharaBossMovingBlocks"
charamovingBlock.depth = 0
charamovingBlock.nodeLineRenderType = "line"
charamovingBlock.nodeLimits = {1, 1}
charamovingBlock.fieldInformation = {
    nodeIndex = {
        fieldType = "integer",
    }
}
charamovingBlock.placements = {
    {
        name = "main",
        data = {
        nodeIndex = 0,
        width = 8,
        height = 8
        }
    }
}

charamovingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("G", false)
charamovingBlock.nodeSprite = fakeTilesHelper.getEntitySpriteFunction("g", false)

function charamovingBlock.nodeRectangle(room, entity, node)
    return utils.rectangle(node.x or 0, node.y or 0, entity.width or 8, entity.height or 8)
end

return charamovingBlock