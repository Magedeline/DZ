-- Small Heart Door entity for Lnn map editor
-- Trigger area that checks chapter-specific mini heart gem collection and either
-- shows a "not enough" dialog or triggers the configured unlock cutscene.

local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local smallHeartDoor = {}

smallHeartDoor.name = "DZ/SmallHeartDoor"
smallHeartDoor.depth = 0
smallHeartDoor.nodeLimits = {0, 0}
smallHeartDoor.canResize = {true, true}

smallHeartDoor.placements = {
    {
        name = "main",
        data = {
            width = 40,
            height = 40,
            chapter = 10,
            requires = 3,
            notEnoughDialog = "",
            unlockCutscene = ""
        }
    }
}

smallHeartDoor.fieldInformation = {
    width = {
        fieldType = "integer",
        minimumValue = 8
    },
    height = {
        fieldType = "integer",
        minimumValue = 8
    },
    chapter = {
        fieldType = "integer",
        minimumValue = 10,
        maximumValue = 15
    },
    requires = {
        fieldType = "integer",
        minimumValue = 1
    }
}

function smallHeartDoor.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 40
    local height = entity.height or 40
    local chapter = entity.chapter or 10
    local requires = entity.requires or 3

    local sprites = {}

    -- Trigger area fill
    local fillRect = drawableRectangle.fromRectangle(
        "fill",
        x, y, width, height,
        {0.8, 0.2, 0.4, 0.25}
    )
    table.insert(sprites, fillRect)

    -- Trigger border
    local borderRect = drawableRectangle.fromRectangle(
        "line",
        x, y, width, height,
        {0.9, 0.3, 0.5, 0.8}
    )
    table.insert(sprites, borderRect)

    -- Small heart icon
    local heartSprite = drawableSprite.fromTexture("collectables/heartGem/0/00")
    heartSprite.x = x + width / 2
    heartSprite.y = y + height / 2
    heartSprite.scaleX = 0.5
    heartSprite.scaleY = 0.5
    table.insert(sprites, heartSprite)

    return sprites
end

function smallHeartDoor.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 40
    local height = entity.height or 40

    return utils.rectangle(x, y, width, height)
end

return smallHeartDoor
