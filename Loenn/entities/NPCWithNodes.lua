-- NPC with Nodes and Actions Entity for L�nn Map Editor
-- Advanced NPC system with node-based movement and action sequences

local npcWithNodes = {}

npcWithNodes.name = "MaggyHelper/NPCWithNodes"
npcWithNodes.depth = -100
npcWithNodes.justification = {0.5, 1.0}
npcWithNodes.nodeLimits = {0, -1}

npcWithNodes.placements = {
    {
        name = "patrol_npc",
        data = {
            spriteId = "theo",
            npcName = "Theo",
            movementType = "patrol",
            movementSpeed = 50,
            waitTimeAtNodes = 2.0,
            faceMovementDirection = true,
            interactionDialog = "THEO_PATROL",
            interactionRange = 48,
            canInteractWhileMoving = false,
            actionSequence = "greet,wave,idle",
            actionTriggers = "proximity,interaction,timer",
            loopActions = true,
            flagToSet = "met_patrol_theo",
            requiredFlag = "",
            removeAfterInteraction = false,
            animationIdle = "idle",
            animationWalk = "walk",
            animationTalk = "talk",
            soundFootsteps = "",
            soundInteraction = ""
        }
    },
    {
        name = "quest_giver_npc",
        data = {
            spriteId = "granny",
            npcName = "Granny",
            movementType = "stationary",
            movementSpeed = 0,
            waitTimeAtNodes = 0.0,
            faceMovementDirection = false,
            interactionDialog = "GRANNY_QUEST",
            interactionRange = 64,
            canInteractWhileMoving = false,
            actionSequence = "idle,beckon,point,nod",
            actionTriggers = "interaction",
            loopActions = false,
            flagToSet = "quest_received",
            requiredFlag = "",
            removeAfterInteraction = false,
            animationIdle = "idle",
            animationWalk = "",
            animationTalk = "talk",
            soundFootsteps = "",
            soundInteraction = ""
        }
    },
    {
        name = "guide_npc",
        data = {
            spriteId = "madeline",
            npcName = "Madeline",
            movementType = "follow_path",
            movementSpeed = 80,
            waitTimeAtNodes = 1.0,
            faceMovementDirection = true,
            interactionDialog = "MADELINE_GUIDE",
            interactionRange = 56,
            canInteractWhileMoving = true,
            actionSequence = "point,wave,walk,stop",
            actionTriggers = "proximity,node_reached",
            loopActions = true,
            flagToSet = "following_madeline",
            requiredFlag = "chapter_unlocked",
            removeAfterInteraction = false,
            animationIdle = "idle",
            animationWalk = "walk",
            animationTalk = "talk",
            soundFootsteps = "",
            soundInteraction = ""
        }
    },
    {
        name = "boss_npc",
        data = {
            spriteId = "chara",
            npcName = "Chara",
            movementType = "aggressive",
            movementSpeed = 120,
            waitTimeAtNodes = 0.5,
            faceMovementDirection = true,
            interactionDialog = "CHARA_BOSS",
            interactionRange = 40,
            canInteractWhileMoving = true,
            actionSequence = "stalk,charge,attack,retreat",
            actionTriggers = "proximity,combat,health",
            loopActions = true,
            flagToSet = "chara_encountered",
            requiredFlag = "boss_fight_ready",
            removeAfterInteraction = false,
            animationIdle = "menacing",
            animationWalk = "stalk",
            animationTalk = "threaten",
            soundFootsteps = "",
            soundInteraction = ""
        }
    },
    {
        name = "cutscene_npc",
        data = {
            spriteId = "oshiro",
            npcName = "Oshiro",
            movementType = "scripted",
            movementSpeed = 60,
            waitTimeAtNodes = 3.0,
            faceMovementDirection = false,
            interactionDialog = "OSHIRO_CUTSCENE",
            interactionRange = 72,
            canInteractWhileMoving = false,
            actionSequence = "bow,gesture,panic,compose",
            actionTriggers = "cutscene_start,dialog_end",
            loopActions = false,
            flagToSet = "cutscene_viewed",
            requiredFlag = "entered_hotel",
            removeAfterInteraction = true,
            animationIdle = "nervous",
            animationWalk = "walk",
            animationTalk = "excited",
            soundFootsteps = "",
            soundInteraction = ""
        }
    }
}

npcWithNodes.fieldInformation = {
    spriteId = {
        options = {"theo", "granny", "madeline", "oshiro", "chara", "kirby", "ralsei", "sans", "papyrus", "toriel", "custom"},
        editable = false
    },
    movementType = {
        options = {"stationary", "patrol", "follow_path", "follow_Kglobal::Player", "aggressive", "scripted", "random"},
        editable = false
    },
    movementSpeed = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 200
    },
    waitTimeAtNodes = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    interactionRange = {
        fieldType = "integer",
        minimumValue = 16,
        maximumValue = 128
    },
    actionSequence = {
        fieldType = "string"
    },
    actionTriggers = {
        options = {"proximity", "interaction", "timer", "cutscene_start", "dialog_end", "node_reached", "combat", "health"},
        editable = true
    }
}

-- Required form field includes
local npcDialogKeys = require("ui.forms.fields.npc_dialog_keys")
local npcSessionFlags = require("ui.forms.fields.npc_session_flags")
local nodeActions = require("ui.forms.fields.node_actions")

-- Override field information with form field data
npcWithNodes.fieldInformation.interactionDialog = npcDialogKeys
npcWithNodes.fieldInformation.flagToSet = npcSessionFlags
npcWithNodes.fieldInformation.requiredFlag = npcSessionFlags

npcWithNodes.texture = "characters/Maggy/DesoloZantas/theo/idle00"

-- Sprite texture lookup for characters that don't use idle00 naming
local spriteTextureLookup = {
    theo = "characters/Maggy/DesoloZantas/theo/idle00",
    oshiro = "characters/Maggy/DesoloZantas/oshiro/idle00",
    granny = "characters/Maggy/DesoloZantas/oldlady/idle00",
    madeline = "characters/Maggy/DesoloZantas/Kglobal::Player/idle00",
    asgore = "characters/Maggy/DesoloZantas/asgore/idle00",
    bird = "characters/Maggy/DesoloZantas/bird/idle00",
    flowey = "characters/Maggy/DesoloZantas/flowey/idle00",
    sans = "characters/Maggy/DesoloZantas/sans/idle00",
    maggy = "characters/Maggy/DesoloZantas/magolor/idle00",
    magolor = "characters/Maggy/DesoloZantas/magolor/idle00",
    monsters = "characters/Maggy/DesoloZantas/monsters/idle00"
}

local function getSpriteTexture(spriteId)
    if spriteTextureLookup[spriteId] then
        return spriteTextureLookup[spriteId]
    end
    return "characters/" .. spriteId .. "/idle00"
end

function npcWithNodes.sprite(room, entity)
    local spriteId = entity.spriteId or "theo"
    local sprites = {}
    
    -- Main NPC sprite
    local mainSprite = {
        texture = getSpriteTexture(spriteId),
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0
    }
    table.insert(sprites, mainSprite)
    
    -- Interaction range circle
    if entity.interactionRange and entity.interactionRange > 0 then
        local range = entity.interactionRange
        local rangeSprite = {
            texture = "objects/IngesteHelper/ui/interaction_range",
            x = entity.x,
            y = entity.y,
            scaleX = range / 32,
            scaleY = range / 32,
            alpha = 0.3,
            justificationX = 0.5,
            justificationY = 1.0
        }
        table.insert(sprites, rangeSprite)
    end
    
    -- Movement type indicator
    local movementIcon = {
        texture = "objects/IngesteHelper/icons/movement_" .. (entity.movementType or "stationary"),
        x = entity.x + 20,
        y = entity.y - 30,
        scaleX = 0.5,
        scaleY = 0.5
    }
    table.insert(sprites, movementIcon)
    
    -- Action sequence indicator
    if entity.actionSequence and entity.actionSequence ~= "" then
        local actionIcon = {
            texture = "objects/IngesteHelper/icons/action_sequence",
            x = entity.x - 20,
            y = entity.y - 30,
            scaleX = 0.5,
            scaleY = 0.5
        }
        table.insert(sprites, actionIcon)
    end
    
    return sprites
end

function npcWithNodes.selection(room, entity)
    local range = entity.interactionRange or 48
    return utils.rectangle(
        entity.x - range/2,
        entity.y - range,
        range,
        range
    )
end

-- Node system for movement and actions
npcWithNodes.nodeLimits = {0, -1} -- Unlimited nodes
npcWithNodes.nodeLineRenderType = "line"

function npcWithNodes.nodeSprite(room, entity, node, nodeIndex)
    local movementType = entity.movementType or "stationary"
    local texture = "objects/IngesteHelper/nodes/movement_node"
    
    -- Different node types based on movement
    if movementType == "patrol" then
        texture = "objects/IngesteHelper/nodes/patrol_node"
    elseif movementType == "follow_path" then
        texture = "objects/IngesteHelper/nodes/path_node"
    elseif movementType == "scripted" then
        texture = "objects/IngesteHelper/nodes/script_node"
    elseif movementType == "aggressive" then
        texture = "objects/IngesteHelper/nodes/combat_node"
    end
    
    return {
        texture = texture,
        x = node.x,
        y = node.y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

function npcWithNodes.nodeRectangle(room, entity, node, nodeIndex)
    return utils.rectangle(node.x - 8, node.y - 8, 16, 16)
end

return npcWithNodes

