local drawableSprite = require("structs.drawable_sprite")

local fakeHeartGem = {}

fakeHeartGem.name = "DZ/FakeHeartGem"
fakeHeartGem.depth = -100
fakeHeartGem.justification = {0.5, 0.5}
fakeHeartGem.placements = {
    {
        name = "main",
        data = {
            persistent = false,
            collectMessage = "It's fake!",
            respawnTime = 3.0
        }
    },
    {
        name = "persistent", 
        data = {
            persistent = true,
            collectMessage = "Gotcha!",
            respawnTime = 3.0
        }
    }
}

fakeHeartGem.fieldInformation = {
    collectMessage = {
        fieldType = "string"
    },
    respawnTime = {
        fieldType = "number",
        minimumValue = 0.0
    }
}

function fakeHeartGem.sprite(room, entity)
    return drawableSprite.fromTexture("collectables/heartgem/0/00", entity)
end

return fakeHeartGem