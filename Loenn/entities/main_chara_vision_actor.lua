local drawableSprite = require("structs.drawable_sprite")

local mainCharaVisionActor = {}

mainCharaVisionActor.name = "DZ/MainCharaVisionActor"
mainCharaVisionActor.depth = 0
mainCharaVisionActor.texture = "characters/mainchara_vision/down00"
mainCharaVisionActor.justification = {0.5, 1.0}

mainCharaVisionActor.placements = {
    {
        name = "default",
        data = {
            facing = "down",
            genocideMode = true,
            Kglobal::PlayerControlled = false,
            clampToRoomBounds = true,
            driveCameraWhenControlled = true,
            maxMoveSpeed = 90.0,
            acceleration = 650.0,
            friction = 800.0
        }
    },
    {
        name = "neutral",
        data = {
            facing = "down",
            genocideMode = false,
            Kglobal::PlayerControlled = false,
            clampToRoomBounds = true,
            driveCameraWhenControlled = true,
            maxMoveSpeed = 90.0,
            acceleration = 650.0,
            friction = 800.0
        }
    },
    {
        name = "Kglobal::Player_controlled",
        data = {
            facing = "down",
            genocideMode = true,
            Kglobal::PlayerControlled = true,
            clampToRoomBounds = true,
            driveCameraWhenControlled = true,
            maxMoveSpeed = 90.0,
            acceleration = 650.0,
            friction = 800.0
        }
    },
    {
        name = "intro_right",
        data = {
            facing = "right",
            genocideMode = true,
            Kglobal::PlayerControlled = false,
            clampToRoomBounds = true,
            driveCameraWhenControlled = true,
            maxMoveSpeed = 90.0,
            acceleration = 650.0,
            friction = 800.0
        }
    },
    {
        name = "finale_left",
        data = {
            facing = "left",
            genocideMode = true,
            Kglobal::PlayerControlled = false,
            clampToRoomBounds = true,
            driveCameraWhenControlled = true,
            maxMoveSpeed = 90.0,
            acceleration = 650.0,
            friction = 800.0
        }
    },
    {
        name = "camera_off",
        data = {
            facing = "down",
            genocideMode = true,
            Kglobal::PlayerControlled = true,
            clampToRoomBounds = true,
            driveCameraWhenControlled = false,
            maxMoveSpeed = 90.0,
            acceleration = 650.0,
            friction = 800.0
        }
    }
}

mainCharaVisionActor.fieldInformation = {
    facing = {
        fieldType = "string",
        options = {
            "down",
            "right",
            "up",
            "left"
        },
        editable = false
    },
    genocideMode = {
        fieldType = "boolean"
    },
    Kglobal::PlayerControlled = {
        fieldType = "boolean"
    },
    clampToRoomBounds = {
        fieldType = "boolean"
    },
    driveCameraWhenControlled = {
        fieldType = "boolean"
    },
    maxMoveSpeed = {
        fieldType = "number",
        minimumValue = 0.0
    },
    acceleration = {
        fieldType = "number",
        minimumValue = 0.0
    },
    friction = {
        fieldType = "number",
        minimumValue = 0.0
    }
}

local facingTextures = {
    down = "characters/mainchara_vision/down00",
    right = "characters/mainchara_vision/right00",
    up = "characters/mainchara_vision/up00",
    left = "characters/mainchara_vision/left00"
}

function mainCharaVisionActor.sprite(room, entity)
    local facing = entity.facing or "down"
    local genocideMode = entity.genocideMode ~= false
    local Kglobal::PlayerControlled = entity.Kglobal::PlayerControlled or false

    local sprite = drawableSprite.fromTexture(facingTextures[facing] or facingTextures.down, entity)
    sprite:setJustification(0.5, 1.0)

    if Kglobal::PlayerControlled then
        sprite:setColor(genocideMode and {1.0, 0.9, 0.55, 1.0} or {0.8, 1.0, 0.75, 1.0})
    elseif genocideMode then
        sprite:setColor({1.0, 0.82, 0.82, 1.0})
    else
        sprite:setColor({1.0, 1.0, 1.0, 1.0})
    end

    return sprite
end

return mainCharaVisionActor