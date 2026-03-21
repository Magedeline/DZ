local utils = require("utils")
local utils = require("utils")
local fakeTilesHelper = require("helpers.fake_tiles")

local ultraDashBlock = {}

ultraDashBlock.name = "MaggyHelper/UltraDashBlock"
ultraDashBlock.depth = -13000
ultraDashBlock.minimumSize = {8, 8}

ultraDashBlock.fieldInformation = {
    tiletype = {
        options = fakeTilesHelper.getTilesOptions(),
        editable = false
    },
    permanent = {
        fieldType = "boolean"
    },
    breakableBySeeker = {
        fieldType = "boolean"
    },
    environment = {
        options = {"General", "Space", "Underwater"},
        editable = false
    }
}

ultraDashBlock.fieldOrder = {
    "x", "y", "width", "height",
    "tiletype", "environment", "permanent", "breakableBySeeker"
}

ultraDashBlock.placements = {
    {
        name = "normal",
        data = {
            width = 16,
            height = 16,
            tiletype = "m",
            permanent = false,
            breakableBySeeker = true,
            environment = "General"
        }
    },
    {
        name = "permanent",
        data = {
            width = 16,
            height = 16,
            tiletype = "m",
            permanent = true,
            breakableBySeeker = true,
            environment = "General"
        }
    }
}

ultraDashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
ultraDashBlock.fieldInformation = fakeTilesHelper.addTileFieldInformation(ultraDashBlock.fieldInformation or {}, "tiletype", false)

function ultraDashBlock.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 16)
end

return ultraDashBlock
