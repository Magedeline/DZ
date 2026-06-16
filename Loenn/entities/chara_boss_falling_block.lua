local fakeTilesHelper = require("helpers.fake_tiles")

local charafallingBlock = {}

charafallingBlock.name = "DZ/CharaBossFallingBlocks"
charafallingBlock.depth = 0
charafallingBlock.placements = {
    name = "chara_falling_block",
    data = {
        width = 8,
        height = 8
    }
}

charafallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("G", false)

return charafallingBlock