local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local tesseractGateway = {}

local placementPresets = {
    {"maddy", "HoldingMaddy"},
    {"default", "CloseBehindPlayer"},
    {"mirror", "CloseBehindPlayer"},
    {"default", "NearestSwitch"},
    {"mirror", "NearestSwitch"},
    {"default", "TouchSwitches"}
}

local textures = {
    default = "objects/DZ/door/TesseractDoorA00",
    tesseract = "objects/DZ/door/TesseractDoorB00",
    maddy = "objects/DZ/door/TesseractDoorC00"
}

local typeOptions = {
    "NearestSwitch",
    "CloseBehindPlayer",
    "CloseBehindPlayerAlways",
    "HoldingMaddy",
    "TouchSwitches",
    "CloseBehindPlayerAndMaddy"
}

local textureOptions = {}

for texture, _ in pairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

tesseractGateway.name = "DZ/TesseractGateway"
tesseractGateway.depth = -9000
tesseractGateway.canResize = {false, false}
tesseractGateway.fieldInformation = {
    sprite = {
        options = textureOptions,
        editable = false
    },
    type = {
        options = typeOptions,
        editable = false
    }
}
tesseractGateway.placements = {}

for _, preset in ipairs(placementPresets) do
    table.insert(tesseractGateway.placements, {
        name = string.format("%s_%s", preset[1], string.lower(preset[2])),
        placementType = "point",
        data = {
            height = 48,
            sprite = preset[1],
            type = preset[2]
        }
    })
end

function tesseractGateway.sprite(room, entity)
    local variant = entity.sprite or "default"
    local texture = textures[variant] or textures["default"]
    local sprite = drawableSprite.fromTexture(texture, entity)
    local height = entity.height or 48

    -- Weird offset from the code, justifications are from sprites.xml
    sprite:setJustification(0.5, 0.0)
    sprite:addPosition(4, height - 48)

    return sprite
end

return tesseractGateway