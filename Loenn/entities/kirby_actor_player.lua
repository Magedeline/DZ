local kirbyActorKglobal::global::Celeste.Player= {}
local drawableSprite = require("structs.drawable_sprite")

kirbyActorPlayer.name = "DZ/KirbyActorKglobal::Player"
kirbyActorPlayer.depth = 0
kirbyActorPlayer.texture = "characters/kirby/idle00"
kirbyActorPlayer.justification = {0.5, 1.0}

kirbyActorPlayer.placements = {
    {
        name = "main",
        data = {
            startAnimation = "idle",
            faceLeft = false,
            showSweat = false,
            sweatAnimation = "idle"
        }
    },
    {
        name = "sweat_danger",
        data = {
            startAnimation = "idle",
            faceLeft = false,
            showSweat = true,
            sweatAnimation = "danger"
        }
    }
}

kirbyActorPlayer.fieldInformation = {
    startAnimation = {
        fieldType = "string",
        options = {
            "idle",
            "walk",
            "runFast",
            "jumpSlow",
            "fall",
            "dash",
            "dreamDashIn",
            "climbup",
            "swimIdle",
            "inhale",
            "spit",
            "parry",
            "transform_in",
            "transformToKirby",
            "combat_punchA"
        },
        editable = true
    },
    faceLeft = {
        fieldType = "boolean"
    },
    showSweat = {
        fieldType = "boolean"
    },
    sweatAnimation = {
        fieldType = "string",
        options = {
            "idle",
            "still",
            "danger",
            "jump",
            "climb"
        },
        editable = true
    }
}

function kirbyActorPlayer.sprite(room, entity)
    local faceLeft = entity.faceLeft or false
    local scaleX = faceLeft and -1 or 1
    local showSweat = entity.showSweat or false

    local sprites = {}

    -- Main actor preview sprite.
    local kirbySprite = drawableSprite.fromTexture("characters/kirby/idle00", entity)
    kirbySprite:setJustification(0.5, 1.0)
    kirbySprite:setScale(scaleX, 1.0)
    table.insert(sprites, kirbySprite)

    -- Dedicated editor badge so this entity is visually distinct from other Kirby entities.
    local badgeSprite = drawableSprite.fromTexture("@Internal@/starting_inventory_kirby", entity)
    badgeSprite:setJustification(0.5, 0.5)
    badgeSprite:setScale(0.55, 0.55)
    badgeSprite:addPosition(10, -18)
    badgeSprite:setColor(showSweat and {1.0, 0.45, 0.45, 0.95} or {1.0, 1.0, 0.4, 0.95})
    table.insert(sprites, badgeSprite)

    -- Show sweat state at a glance in Loenn.
    if showSweat then
        local sweatSprite = drawableSprite.fromTexture("characters/kirby/sweat/idle00", entity)
        sweatSprite:setJustification(0.5, 1.0)
        sweatSprite:setScale(scaleX, 1.0)
        sweatSprite:setColor({1.0, 1.0, 1.0, 0.75})
        table.insert(sprites, sweatSprite)
    end

    return sprites
end

return kirbyActorPlayer
