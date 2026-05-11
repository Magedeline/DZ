local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local textures = {
    default = "objects/Maggy/DesoloZantas/temple/dashButton00",
    mirror = "objects/Maggy/DesoloZantas/temple/dashButtonMirror00",
    tesseract = "objects/Maggy/DesoloZantas/temple/dashButtonTesseract00",
}
local textureOptions = {}

for texture, _ in pairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

-- Up, right, down, left
local dashSwitchDirectionLookup = {
    {"TesseractSwitchV", "ceiling", false},
    {"TesseractSwitchV", "ceiling", true},
}

local function rotateCommon(entity, sideIndex, direction)
    -- Only two directions are defined, so use modulo 2
    local targetIndex = utils.mod1(sideIndex + direction, 2)

    if sideIndex ~= targetIndex then
        local newName, attribute, value = table.unpack(dashSwitchDirectionLookup[targetIndex])

        entity._name = newName

        entity.ceiling = nil
        entity.leftSide = nil

        entity[attribute] = value
    end

    return sideIndex ~= targetIndex
end

local dashSwitchVertical = {}

dashSwitchVertical.name = "MaggyHelper/TesseractSwitch"
dashSwitchVertical.depth = 0
dashSwitchVertical.justification = {0.5, 0.5}
dashSwitchVertical.fieldInformation = {
    sprite = {
        options = textureOptions
    }
}
dashSwitchVertical.placements = {}

function dashSwitchVertical.sprite(room, entity)
    local ceiling = entity.ceiling
    local texture = textures[entity.sprite] or textures["default"]
    local sprite = drawableSprite.fromTexture(texture, entity)

    if ceiling then
        sprite:addPosition(8, 0)
        sprite.rotation = -math.pi / 2
    else
        sprite:addPosition(8, 8)
        sprite.rotation = math.pi / 2
    end

    return sprite
end

function dashSwitchVertical.flip(room, entity, horizontal, vertical)
    if vertical then
        entity.ceiling = not entity.ceiling
    end

    return vertical
end

function dashSwitchVertical.rotate(room, entity, direction)
    local sideIndex = entity.ceiling and 2 or 1

    return rotateCommon(entity, sideIndex, direction)
end

local placementsInfo = {
    {"up", "ceiling", false},
    {"down", "ceiling", true},
}

for name, texture in pairs(textures) do
    for _, info in ipairs(placementsInfo) do
        local direction, key, value = table.unpack(info)
        local placement = {
            name = string.format("%s_%s", direction, name),
            data = {
                persistent = false,
                sprite = name,
                allGates = false
            }
        }

        if key then
            placement.data[key] = value
        end

        table.insert(dashSwitchVertical.placements, placement)
    end
end

return dashSwitchVertical
