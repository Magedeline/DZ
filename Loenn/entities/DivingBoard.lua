local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local divingBoard = {}

divingBoard.name = "DZ/DivingBoard"
divingBoard.depth = -1

divingBoard.placements = {
    {
        name = "normal",
        data = {
            launchSpeed = -300.0
        }
    },
    {
        name = "high_launch",
        data = {
            launchSpeed = -400.0
        }
    }
}

divingBoard.fieldInformation = {
    launchSpeed = {
        fieldType = "number",
        description = "Vertical launch speed when Kglobal::global::Celeste.Playerjumps (negative = up)"
    }
}

divingBoard.fieldOrder = {
    "x", "y", "launchSpeed"
}

function divingBoard.sprite(room, entity)
    local sprites = {}
    
    local base = drawableSprite.fromTexture("objects/divingBoardBase", entity)
    base:setPosition(entity.x + 12, entity.y + 8)
    base:setJustification(0.5, 0.5)
    table.insert(sprites, base)
    
    local board = drawableSprite.fromTexture("objects/divingBoard", entity)
    board:setPosition(entity.x + 12, entity.y)
    board:setJustification(0.5, 0.5)
    table.insert(sprites, board)
    
    return sprites
end

function divingBoard.selection(room, entity)
    return utils.rectangle(entity.x, entity.y - 4, 24, 16)
end

return divingBoard
