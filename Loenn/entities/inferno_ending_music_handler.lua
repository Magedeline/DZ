local drawableRectangle = require("structs.drawable_rectangle")

local infernoEndingMusicHandler = {}

infernoEndingMusicHandler.name = "MaggyHelper/InfernoEndingMusicHandler"
infernoEndingMusicHandler.depth = 0

infernoEndingMusicHandler.placements = {
    {
        name = "normal",
        data = {
            startLevel = "e-01",
            endLevel = "e-09",
            roomPattern = "e-*",
            music = "guid://{7e1c842a-b3d8-440a-9c39-196826b8bee2}",
            fadeOutLayer = 1,
            fadeInLayer = 5
        }
    }
}

infernoEndingMusicHandler.fieldInformation = {
    fadeOutLayer = { fieldType = "integer", minimumValue = 1, maximumValue = 6 },
    fadeInLayer = { fieldType = "integer", minimumValue = 1, maximumValue = 6 }
}

infernoEndingMusicHandler.fieldOrder = {
    "x", "y", "startLevel", "endLevel", "roomPattern", "music", "fadeOutLayer", "fadeInLayer"
}

function infernoEndingMusicHandler.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x or 0, entity.y or 0

    -- Visual indicator in the editor (music note icon placeholder)
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x - 8, y - 8, 16, 16, {0.2, 0.6, 0.8, 0.7}))
    table.insert(sprites, drawableRectangle.fromRectangle("line", x - 8, y - 8, 16, 16, {0.3, 0.8, 1.0, 1}))

    return sprites
end

function infernoEndingMusicHandler.selection(room, entity)
    local utils = require("utils")
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return infernoEndingMusicHandler

