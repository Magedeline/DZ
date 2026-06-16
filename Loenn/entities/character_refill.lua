local characterRefill = {}

characterRefill.name = "DZ/CharacterRefill"
characterRefill.depth = -100

-- Character type options for the dropdown
characterRefill.fieldInformation = {
    characterType = {
        options = {
            ["Kirby"] = 0,
            ["Madeline"] = 1,
            ["Badeline"] = 2,
            ["Theo"] = 3,
            ["Granny"] = 4,
            ["Oshiro"] = 5,
            ["Chara"] = 6,
            ["Frisk"] = 7,
            ["Ralsei"] = 8,
            ["Asriel"] = 9,
            ["Meta Knight"] = 10,
            ["King Dedede"] = 11,
            ["Magolor"] = 12,
            ["Mage Kirby"] = 13,
            ["Custom"] = 14
        },
        editable = false
    },
    dashCount = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    }
}

characterRefill.fieldOrder = {
    "x", "y",
    "characterType",
    "dashCount",
    "oneUse",
    "grantsSpecialAbility",
    "refillStamina",
    "characterModeOnly",
    "customSpritePath",
    "customSoundEvent"
}

-- Multiple placements for quick character selection
characterRefill.placements = {
    {
        name = "kirby",
        alternativeName = "kirby_refill",
        data = {
            characterType = 0,
            dashCount = 1,
            oneUse = false,
            grantsSpecialAbility = true,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "madeline",
        alternativeName = "madeline_refill",
        data = {
            characterType = 1,
            dashCount = 1,
            oneUse = false,
            grantsSpecialAbility = false,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "badeline",
        alternativeName = "badeline_refill",
        data = {
            characterType = 2,
            dashCount = 2,
            oneUse = false,
            grantsSpecialAbility = true,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "theo",
        alternativeName = "theo_refill",
        data = {
            characterType = 3,
            dashCount = 1,
            oneUse = false,
            grantsSpecialAbility = false,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "granny",
        alternativeName = "granny_refill",
        data = {
            characterType = 4,
            dashCount = 1,
            oneUse = false,
            grantsSpecialAbility = false,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "oshiro",
        alternativeName = "oshiro_refill",
        data = {
            characterType = 5,
            dashCount = 1,
            oneUse = false,
            grantsSpecialAbility = false,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "chara",
        alternativeName = "chara_refill",
        data = {
            characterType = 6,
            dashCount = 1,
            oneUse = false,
            grantsSpecialAbility = true,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "frisk",
        alternativeName = "frisk_refill",
        data = {
            characterType = 7,
            dashCount = 1,
            oneUse = false,
            grantsSpecialAbility = false,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "ralsei",
        alternativeName = "ralsei_refill",
        data = {
            characterType = 8,
            dashCount = 1,
            oneUse = false,
            grantsSpecialAbility = true,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "asriel",
        alternativeName = "asriel_refill",
        data = {
            characterType = 9,
            dashCount = 3,
            oneUse = false,
            grantsSpecialAbility = true,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "meta_knight",
        alternativeName = "meta_knight_refill",
        data = {
            characterType = 10,
            dashCount = 2,
            oneUse = false,
            grantsSpecialAbility = true,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "king_dedede",
        alternativeName = "dedede_refill",
        data = {
            characterType = 11,
            dashCount = 2,
            oneUse = false,
            grantsSpecialAbility = false,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "magolor",
        alternativeName = "magolor_refill",
        data = {
            characterType = 12,
            dashCount = 2,
            oneUse = false,
            grantsSpecialAbility = false,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    },
    {
        name = "mage_kirby",
        alternativeName = "mage_kirby_refill",
        data = {
            characterType = 13,
            dashCount = 2,
            oneUse = false,
            grantsSpecialAbility = true,
            refillStamina = true,
            characterModeOnly = false,
            customSpritePath = "",
            customSoundEvent = ""
        }
    }
}

-- Get the texture based on character type
function characterRefill.texture(room, entity)
    local characterType = entity.characterType or 0
    
    -- Map character types to their texture paths
    local textures = {
        [0]  = "objects/characterRefill/kirby/idle00",      -- Kirby
        [1]  = "objects/characterRefill/madeline/idle00",   -- Madeline
        [2]  = "objects/characterRefill/badeline/idle00",   -- Badeline
        [3]  = "objects/characterRefill/theo/idle00",       -- Theo
        [4]  = "objects/characterRefill/granny/idle00",     -- Granny
        [5]  = "objects/characterRefill/oshiro/idle00",     -- Oshiro
        [6]  = "objects/characterRefill/chara/idle00",      -- Chara
        [7]  = "objects/characterRefill/frisk/idle00",      -- Frisk
        [8]  = "objects/characterRefill/ralsei/idle00",     -- Ralsei
        [9]  = "objects/characterRefill/asriel/idle00",     -- Asriel
        [10] = "objects/characterRefill/metaknight/idle00", -- Meta Knight
        [11] = "objects/characterRefill/dedede/idle00",     -- King Dedede
        [12] = "objects/characterRefill/magolor/idle00",    -- Magolor
        [13] = "objects/characterRefill/magekirby/idle00",  -- Mage Kirby
        [14] = "objects/refill/idle00"                      -- Custom/fallback
    }
    
    -- Fallback textures based on dash count
    local fallbackTextures = {
        [0]  = "objects/refill/idle00",       -- 1 dash
        [1]  = "objects/refill/idle00",
        [2]  = "objects/refillTwo/idle00",    -- 2 dash
        [3]  = "objects/refillTwo/idle00",
        [4]  = "objects/refillTwo/idle00",
        [5]  = "objects/refillTwo/idle00",
        [6]  = "objects/refill/idle00",
        [7]  = "objects/refill/idle00",
        [8]  = "objects/refill/idle00",
        [9]  = "objects/refillTwo/idle00",
        [10] = "objects/refillTwo/idle00",
        [11] = "objects/refillTwo/idle00",
        [12] = "objects/refillTwo/idle00",
        [13] = "objects/refillTwo/idle00",
        [14] = "objects/refill/idle00"
    }
    
    local texture = textures[characterType] or textures[14]
    
    -- Try to load character-specific texture, fall back if not found
    -- In Loenn, we'll use the fallback for now until sprites are created
    return fallbackTextures[characterType] or "objects/refill/idle00"
end

-- Get color tint based on character for visual distinction in editor
function characterRefill.color(room, entity)
    local characterType = entity.characterType or 0
    
    local colors = {
        [0]  = {1.0, 0.4, 0.7, 1.0},   -- Kirby - Pink
        [1]  = {0.9, 0.4, 0.4, 1.0},   -- Madeline - Red
        [2]  = {0.55, 0.27, 1.0, 1.0}, -- Badeline - Purple
        [3]  = {0.12, 0.56, 1.0, 1.0}, -- Theo - Blue
        [4]  = {0.75, 0.75, 0.75, 1.0},-- Granny - Silver
        [5]  = {0.4, 0.8, 0.7, 1.0},   -- Oshiro - Teal
        [6]  = {1.0, 0.0, 0.0, 1.0},   -- Chara - Red
        [7]  = {1.0, 0.84, 0.0, 1.0},  -- Frisk - Gold
        [8]  = {0.2, 0.8, 0.2, 1.0},   -- Ralsei - Green
        [9]  = {1.0, 1.0, 1.0, 1.0},   -- Asriel - White
        [10] = {0.0, 0.0, 0.55, 1.0},  -- Meta Knight - Dark Blue
        [11] = {0.25, 0.41, 0.88, 1.0},-- King Dedede - Royal Blue
        [12] = {0.4, 0.2, 0.6, 1.0},   -- Magolor - Purple
        [13] = {0.0, 1.0, 1.0, 1.0},   -- Mage Kirby - Cyan
        [14] = {1.0, 1.0, 1.0, 1.0}    -- Custom - White
    }
    
    return colors[characterType] or colors[14]
end

return characterRefill
