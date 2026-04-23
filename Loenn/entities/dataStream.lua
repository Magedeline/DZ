local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local dataStream = {}

dataStream.name = "MaggyHelper/DataStream"
dataStream.depth = -50
dataStream.fieldInformation = {
    direction = {
        options = {"Right", "Left", "Up", "Down"},
        editable = false
    },
    flowSpeed = {
        minimumValue = 0
    },
    streamWidth = {
        minimumValue = 1
    },
    streamLength = {
        minimumValue = 1
    }
}
dataStream.placements = {
    {
        name = "right",
        data = {
            direction = "Right",
            flowSpeed = 150,
            streamWidth = 32,
            streamLength = 200
        }
    },
    {
        name = "left",
        data = {
            direction = "Left",
            flowSpeed = 150,
            streamWidth = 32,
            streamLength = 200
        }
    },
    {
        name = "up",
        data = {
            direction = "Up",
            flowSpeed = 150,
            streamWidth = 32,
            streamLength = 200
        }
    },
    {
        name = "down",
        data = {
            direction = "Down",
            flowSpeed = 150,
            streamWidth = 32,
            streamLength = 200
        }
    }
}

function dataStream.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/data_stream/flowing", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {0.0, 1.0, 0.5, 0.6}
    return sprite
end

function dataStream.selection(room, entity)
    local w = entity.streamWidth or 32
    local l = entity.streamLength or 200
    
    if entity.direction == "Right" or entity.direction == "Left" then
        return utils.rectangle(entity.x - l/2, entity.y - w/2, l, w)
    else
        return utils.rectangle(entity.x - w/2, entity.y - l/2, w, l)
    end
end

return dataStream
