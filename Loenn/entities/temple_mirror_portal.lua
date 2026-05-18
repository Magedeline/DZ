local drawableSprite = require("structs.drawable_sprite")

local templeMirrorPortal = {}

templeMirrorPortal.name = "MaggyHelper/TesseractMirrorPortal"
templeMirrorPortal.depth = -1999
templeMirrorPortal.placements = {
    name = "tesseract_mirror",
    }

local frameTexture = "objects/Maggy/DesoloZantas/temple/portal/portalframe"
local curtainTexture = "objects/Maggy/DesoloZantas/temple/portal/portalcurtain00"
local torchTexture = "objects/Maggy/DesoloZantas/temple/portal/portaltorch00"

local torchOffset = 90

function templeMirrorPortal.sprite(room, entity)
    local frameSprite = drawableSprite.fromTexture(frameTexture, entity)
    local curtainSprite = drawableSprite.fromTexture(curtainTexture, entity)
    local torchSpriteLeft = drawableSprite.fromTexture(torchTexture, entity)
    local torchSpriteRight = drawableSprite.fromTexture(torchTexture, entity)

    torchSpriteLeft:addPosition(-torchOffset, 0)
    torchSpriteLeft:setJustification(0.5, 0.75)

    torchSpriteRight:addPosition(torchOffset, 0)
    torchSpriteRight:setJustification(0.5, 0.75)

    local sprites = {
        frameSprite,
        curtainSprite,
        torchSpriteLeft,
        torchSpriteRight
    }

    return sprites
end

return templeMirrorPortal
