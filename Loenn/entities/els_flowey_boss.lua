-- Els Flowey Boss Entity
-- The corrupted organic nightmare boss from Chapter 16

local elsFloweyBoss = {}

elsFloweyBoss.name = "DZ/ElsFloweyBoss"

elsFloweyBoss.placements = {
    {
        name = "elsflowey",
        data = {
            startingPhase = "Intro",
            maxHealth = 1000.0,
            isVulnerable = false,
            enableOrganicPhase = true,
            enableBonePhase = true,
            enableFleshPhase = true,
            enableWeaponPhase = true,
            enableSoulCollection = true,
            enableFinalForm = true,
            corruptionLevel = 5.0,
            nightmareIntensity = 3.0,
            autoStartBattle = false,
            showHealthBar = true,
            enableSpecialAttacks = true,
            allowMercy = true
        }
    },
    {
        name = "no_mercy",
        data = {
            startingPhase = "Intro",
            maxHealth = 1500.0,
            isVulnerable = false,
            enableOrganicPhase = true,
            enableBonePhase = true,
            enableFleshPhase = true,
            enableWeaponPhase = true,
            enableSoulCollection = true,
            enableFinalForm = true,
            corruptionLevel = 10.0,
            nightmareIntensity = 5.0,
            autoStartBattle = true,
            showHealthBar = true,
            enableSpecialAttacks = true,
            allowMercy = false
        }
    }
}

elsFloweyBoss.fieldInformation = {
    startingPhase = {
        fieldType = "string",
        options = {
            "Intro",
            "OrganGarden", 
            "BoneCathedral",
            "FleshLabyrinth",
            "WeaponArsenal",
            "SoulCollection",
            "FinalForm",
            "Defeated"
        }
    },
    maxHealth = {
        fieldType = "number",
        minimumValue = 100.0,
        maximumValue = 5000.0
    },
    isVulnerable = {
        fieldType = "boolean"
    },
    enableOrganicPhase = {
        fieldType = "boolean"
    },
    enableBonePhase = {
        fieldType = "boolean"
    },
    enableFleshPhase = {
        fieldType = "boolean"
    },
    enableWeaponPhase = {
        fieldType = "boolean"
    },
    enableSoulCollection = {
        fieldType = "boolean"
    },
    enableFinalForm = {
        fieldType = "boolean"
    },
    corruptionLevel = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 10.0
    },
    nightmareIntensity = {
        fieldType = "number", 
        minimumValue = 1.0,
        maximumValue = 10.0
    },
    autoStartBattle = {
        fieldType = "boolean"
    },
    showHealthBar = {
        fieldType = "boolean"
    },
    enableSpecialAttacks = {
        fieldType = "boolean"
    },
    allowMercy = {
        fieldType = "boolean"
    }
}

function elsFloweyBoss.sprite(room, entity)
    local phase = entity.startingPhase or "Intro"
    local corruptionLevel = entity.corruptionLevel or 5.0
    
    -- Choose sprite based on phase and corruption level
    local spritePath = "characters/flowey/"
    
    if phase == "Intro" then
        spritePath = spritePath .. "flowey_glitchy_idle"
    elseif phase == "OrganGarden" then
        spritePath = spritePath .. "flowey_organic_form"
    elseif phase == "BoneCathedral" then
        spritePath = spritePath .. "flowey_bone_form"
    elseif phase == "FleshLabyrinth" then
        spritePath = spritePath .. "flowey_flesh_form"
    elseif phase == "WeaponArsenal" then
        spritePath = spritePath .. "flowey_weapon_form"
    elseif phase == "SoulCollection" then
        spritePath = spritePath .. "flowey_soul_form"
    elseif phase == "FinalForm" then
        spritePath = spritePath .. "flowey_nightmare_form"
    else
        spritePath = spritePath .. "flowey_glitchy_idle"
    end
    
    return spritePath
end

function elsFloweyBoss.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 16, 32, 32)
end

-- Boss phase colors for visual identification
local phaseColors = {
    Intro = {0.8, 0.2, 0.8, 0.8},           -- Purple
    OrganGarden = {0.8, 0.2, 0.2, 0.8},     -- Red
    BoneCathedral = {0.9, 0.9, 0.9, 0.8},   -- White
    FleshLabyrinth = {0.8, 0.4, 0.4, 0.8},  -- Dark Red
    WeaponArsenal = {0.6, 0.6, 0.6, 0.8},   -- Gray
    SoulCollection = {0.9, 0.9, 0.2, 0.8},  -- Yellow
    FinalForm = {0.1, 0.1, 0.1, 0.9},       -- Black
    Defeated = {0.2, 0.8, 0.2, 0.8}         -- Green
}

function elsFloweyBoss.nodeSprite(room, entity, node, nodeIndex)
    -- Nodes can represent soul positions or attack points
    return "objects/DesoloZantas/soul_position", {0.9, 0.9, 0.2, 0.9}
end

function elsFloweyBoss.nodeSelection(room, entity, node)
    return utils.rectangle(node.x - 4, node.y - 4, 8, 8)
end

function elsFloweyBoss.nodeLimits()
    return {0, 8} -- 0 to 8 soul positions
end

-- Visual indicators for the boss arena
function elsFloweyBoss.rectangle(room, entity)
    local phase = entity.startingPhase or "Intro"
    local color = phaseColors[phase] or phaseColors.Intro
    
    local width = 32
    local height = 32
    
    -- Larger size for final form
    if phase == "FinalForm" then
        width = 48
        height = 48
    end
    
    return utils.rectangle(entity.x - width/2, entity.y - height/2, width, height), color
end

return elsFloweyBoss
