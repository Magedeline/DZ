-- Flowey Portrait Entity
-- Handles the various glitchy Flowey portrait states mentioned in Chapter 16 dialog

local floweyPortrait = {}

floweyPortrait.name = "DZ/FloweyPortrait"

floweyPortrait.placements = {
    {
        name = "glitchy_freak",
        data = {
            portraitType = "glitchyfreak",
            displayDuration = 5.0,
            autoTrigger = false,
            triggerOnDialog = true,
            dialogTrigger = "{portrait flowey glitchyfreak}",
            glitchIntensity = 5.0,
            enableDistortion = true,
            enableParticles = true,
            soundEffect = ""
        }
    },
    {
        name = "glitchy_creepy",
        data = {
            portraitType = "glitchycreepy", 
            displayDuration = 7.0,
            autoTrigger = false,
            triggerOnDialog = true,
            dialogTrigger = "{portrait flowey glitchycreepy}",
            glitchIntensity = 6.0,
            enableDistortion = true,
            enableParticles = true,
            soundEffect = ""
        }
    },
    {
        name = "glitchy_panic",
        data = {
            portraitType = "glitchypanic",
            displayDuration = 4.0,
            autoTrigger = false,
            triggerOnDialog = true,
            dialogTrigger = "{portrait flowey glitchypanic}",
            glitchIntensity = 8.0,
            enableDistortion = true,
            enableParticles = true,
            soundEffect = ""
        }
    },
    {
        name = "glitchy_revenge",
        data = {
            portraitType = "glitchyrevenge",
            displayDuration = 6.0,
            autoTrigger = false,
            triggerOnDialog = true,
            dialogTrigger = "{portrait flowey glitchyrevenge}",
            glitchIntensity = 9.0,
            enableDistortion = true,
            enableParticles = true,
            soundEffect = ""
        }
    },
    {
        name = "glitchy_angry",
        data = {
            portraitType = "glitchyangry",
            displayDuration = 5.0,
            autoTrigger = false,
            triggerOnDialog = true,
            dialogTrigger = "{portrait flowey glitchyangry}",
            glitchIntensity = 7.0,
            enableDistortion = true,
            enableParticles = true,
            soundEffect = ""
        }
    },
    {
        name = "glitchy_smirk",
        data = {
            portraitType = "glitchysmirk",
            displayDuration = 3.0,
            autoTrigger = false,
            triggerOnDialog = true,
            dialogTrigger = "{portrait flowey glitchysmirk}",
            glitchIntensity = 4.0,
            enableDistortion = true,
            enableParticles = false,
            soundEffect = ""
        }
    },
    {
        name = "glitchy_generic",
        data = {
            portraitType = "glitchy",
            displayDuration = 4.0,
            autoTrigger = false,
            triggerOnDialog = true,
            dialogTrigger = "{portrait flowey glitchy}",
            glitchIntensity = 6.0,
            enableDistortion = true,
            enableParticles = true,
            soundEffect = ""
        }
    }
}

floweyPortrait.fieldInformation = {
    portraitType = {
        fieldType = "string",
        options = {
            "glitchyfreak",
            "glitchycreepy",
            "glitchypanic", 
            "glitchyrevenge",
            "glitchyangry",
            "glitchysmirk",
            "glitchy",
            "normal",
            "evil",
            "laughing",
            "finalmoment",
            "skibididoop",
            "normal",
            "wtf",
            "wtfalt",
            "freakout"
        }
    },
    displayDuration = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 30.0
    },
    autoTrigger = {
        fieldType = "boolean"
    },
    triggerOnDialog = {
        fieldType = "boolean"
    },
    dialogTrigger = {
        fieldType = "string"
    },
    glitchIntensity = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 10.0
    },
    enableDistortion = {
        fieldType = "boolean"
    },
    enableParticles = {
        fieldType = "boolean"
    },
    soundEffect = {
        fieldType = "string"
    }
}

function floweyPortrait.sprite(room, entity)
    local portraitType = entity.portraitType or "glitchyfreak"
    local glitchIntensity = entity.glitchIntensity or 5.0
    
    local spritePath = "characters/flowey/portraits/"
    
    if portraitType == "glitchyfreak" then
        spritePath = spritePath .. "flowey_glitchy_freak"
    elseif portraitType == "glitchycreepy" then
        spritePath = spritePath .. "flowey_glitchy_creepy"
    elseif portraitType == "glitchypanic" then
        spritePath = spritePath .. "flowey_glitchy_panic"
    elseif portraitType == "glitchyrevenge" then
        spritePath = spritePath .. "flowey_glitchy_revenge"
    elseif portraitType == "glitchyangry" then
        spritePath = spritePath .. "flowey_glitchy_angry"
    elseif portraitType == "glitchysmirk" then
        spritePath = spritePath .. "flowey_glitchy_smirk"
    elseif portraitType == "glitchy" then
        spritePath = spritePath .. "flowey_glitchy"
    elseif portraitType == "freakout" then
        spritePath = spritePath .. "flowey_freakout"
    elseif portraitType == "finalmoment" then
        spritePath = spritePath .. "flowey_final_moment"
    elseif portraitType == "skibididoop" then
        spritePath = spritePath .. "flowey_skibidi"
    elseif portraitType == "wtf" then
        spritePath = spritePath .. "flowey_wtf"
    elseif portraitType == "wtfalt" then
        spritePath = spritePath .. "flowey_wtf_alt"
    else
        spritePath = spritePath .. "flowey_" .. portraitType
    end
    
    return spritePath
end

function floweyPortrait.selection(room, entity)
    return utils.rectangle(entity.x - 32, entity.y - 32, 64, 64)
end

-- Portrait emotion colors for visual identification
local portraitColors = {
    glitchyfreak = {0.8, 0.1, 0.8, 0.9},    -- Purple freak
    glitchycreepy = {0.6, 0.1, 0.1, 0.9},   -- Dark red creepy
    glitchypanic = {0.9, 0.5, 0.1, 0.9},    -- Orange panic
    glitchyrevenge = {0.9, 0.1, 0.1, 0.9},  -- Red revenge
    glitchyangry = {0.8, 0.2, 0.2, 0.9},    -- Angry red
    glitchysmirk = {0.7, 0.3, 0.7, 0.9},    -- Purple smirk
    glitchy = {0.6, 0.2, 0.8, 0.9},         -- Generic glitch purple
    normal = {0.2, 0.8, 0.2, 0.9},          -- Green normal
    evil = {0.1, 0.1, 0.1, 0.9},            -- Black evil
    laughing = {0.9, 0.9, 0.2, 0.9},        -- Yellow laughing
    finalmoment = {0.9, 0.3, 0.1, 0.9},     -- Orange final
    skibididoop = {0.8, 0.2, 0.9, 0.9},     -- Magenta meme
    wtf = {0.9, 0.6, 0.1, 0.9},             -- Orange confusion
    wtfalt = {0.9, 0.7, 0.2, 0.9},          -- Light orange alt
    freakout = {0.9, 0.1, 0.5, 0.9}         -- Pink freakout
}

function floweyPortrait.rectangle(room, entity)
    local portraitType = entity.portraitType or "glitchyfreak"
    local glitchIntensity = entity.glitchIntensity or 5.0
    
    local color = portraitColors[portraitType] or portraitColors.glitchyfreak
    
    -- Adjust intensity based on glitch level
    local intensity = glitchIntensity / 10.0
    color = {
        color[1] * intensity,
        color[2] * intensity,
        color[3] * intensity,
        color[4]
    }
    
    local size = 64 + (glitchIntensity - 5) * 4 -- Size varies with glitch intensity
    return utils.rectangle(entity.x - size/2, entity.y - size/2, size, size), color
end

-- Node support for portrait animation keyframes or effect points
function floweyPortrait.nodeSprite(room, entity, node, nodeIndex)
    local portraitType = entity.portraitType or "glitchyfreak"
    
    if portraitType:find("glitchy") then
        return "objects/Maggy_DesoloZantas/glitch_keyframe"
    else
        return "objects/Maggy_DesoloZantas/portrait_keyframe"
    end
end

function floweyPortrait.nodeSelection(room, entity, node)
    return utils.rectangle(node.x - 6, node.y - 6, 12, 12)
end

function floweyPortrait.nodeLimits()
    return {0, 4} -- Up to 4 animation keyframes
end

return floweyPortrait
