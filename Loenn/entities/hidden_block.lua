local utils = require("utils")
local utils = require("utils")
local fakeTilesHelper = require("helpers.fake_tiles")

local hiddenBlock = {}

hiddenBlock.name = "DZ/HiddenBlock"
hiddenBlock.depth = -13000
hiddenBlock.minimumSize = {8, 8}

hiddenBlock.fieldInformation = {
    tiletype = {
        options = fakeTilesHelper.getTilesOptions(),
        editable = false
    },
    permanent = {
        fieldType = "boolean"
    }
}

hiddenBlock.fieldOrder = {
    "x", "y", "width", "height",
    "tiletype", "permanent"
}

hiddenBlock.placements = {
    {
        name = "normal",
        data = {
            width = 16,
            height = 16,
            tiletype = "3",
            permanent = true
        }
    }
}

hiddenBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
hiddenBlock.fieldInformation = fakeTilesHelper.addTileFieldInformation(hiddenBlock.fieldInformation or {}, "tiletype", false)

function hiddenBlock.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 16)
end

return hiddenBlock
