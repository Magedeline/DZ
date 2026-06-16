-- Loenn plugin for DZ - Ability Star Entity
local drawableSprite = require("structs.drawable_sprite")

local abilityStar = {}

abilityStar.name = "DZ/AbilityStar"
abilityStar.depth = -100
abilityStar.placements = {
    {
        name = "fire",
        data = {
            ability = "Fire"
        }
    },
    {
        name = "ice",
        data = {
            ability = "Ice"
        }
    },
    {
        name = "sword",
        data = {
            ability = "Sword"
        }
    },
    {
        name = "beam",
        data = {
            ability = "Beam"
        }
    },
    {
        name = "spark",
        data = {
            ability = "Spark"
        }
    },
    {
        name = "stone",
        data = {
            ability = "Stone"
        }
    },
    {
        name = "bomb",
        data = {
            ability = "Bomb"
        }
    },
    {
        name = "hammer",
        data = {
            ability = "Hammer"
        }
    },
    {
        name = "ninja",
        data = {
            ability = "Ninja"
        }
    },
    {
        name = "cutter",
        data = {
            ability = "Cutter"
        }
    }
}

abilityStar.fieldInformation = {
    ability = {
        options = {
            "None",
            "Fire",
            "Ice",
            "Spark",
            "Sword",
            "Cutter",
            "Beam",
            "Stone",
            "Needle",
            "Parasol",
            "Wheel",
            "Bomb",
            "Fighter",
            "Suplex",
            "Ninja",
            "Mirror",
            "Hammer",
            "Wing",
            "UFO",
            "Sleep"
        },
        editable = false
    }
}

function abilityStar.sprite(room, entity)
    local ability = entity.ability or "Fire"
    local texture = "objects/abilityStar/" .. string.lower(ability)
    return drawableSprite.fromTexture(texture, entity)
end

return abilityStar
