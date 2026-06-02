local utils = require("utils")

local dialogNPC = {}

dialogNPC.name = "MaggyHelper/DialogNPC"
dialogNPC.depth = 0

local aiTypes = {
    "None", "Swim", "Fly", "Fly (Tied)", "Smart Fly", "Node Walk", 
    "Chase Walk", "Wander", "Walk & Climb", "Chase Jump", "Pathfind",
    "Follow Kglobal::Player", "Patrol", "Guard Point", "Flee Kglobal::Player", "Circle Kglobal::Player", "Hover"
}

dialogNPC.placements = {
    {
        name = "normal",
        data = {
            sprite = "characters/Kglobal::Player/idle00",
            aiType = "None",
            hitboxXOffset = 0,
            hitboxYOffset = 0,
            XSpeed = 120.0,
            YSpeed = 160.0,
            talkBoundsWidth = 32,
            talkBoundsHeight = 32,
            talkIndicatorX = 0,
            talkIndicatorY = -24,
            basicDialogID = "",
            luaCutscene = "",
            csEventID = "",
            cutsceneModeEnabled = false,
            isActive = true,
            isVisible = true,
            isInteractable = true,
            isMoving = false,
            isPatrolling = false,
            isTalking = false,
            isInCutscene = false,
            isStunned = false,
            isHostile = false,
            isFriendly = true,
            isFollowingKglobal::global::Celeste.Player= false,
            isInvincible = false,
            isGrounded = true,
            isAirborne = false,
            aiEnabled = true,
            cutsceneActive = false,
            cutsceneSkippable = true,
            cutscenePlayed = false,
            cutscenePaused = false,
            cutsceneWaitingForInput = false,
            cutsceneLocked = false
        }
    },
    {
        name = "theo_style", 
        data = {
            sprite = "characters/theo/idle00",
            aiType = "Wander",
            basicDialogID = "THEO_GREETING",
            XSpeed = 80.0,
            YSpeed = 120.0,
            isFriendly = true,
            isInteractable = true
        }
    },
    {
        name = "chara_style",
        data = {
            sprite = "characters/chara/idle00",
            aiType = "Chase Walk",
            basicDialogID = "CHARA_TAUNT", 
            XSpeed = 150.0,
            isHostile = true,
            isFriendly = false
        }
    },
    {
        name = "cutscene_npc",
        data = {
            cutsceneModeEnabled = true,
            csEventID = "default_cutscene",
            cutsceneSkippable = true,
            isInteractable = true,
            aiType = "None"
        }
    }
}

dialogNPC.fieldInformation = {
    aiType = {
        options = aiTypes,
        editable = false
    },
    basicDialogID = {
        fieldType = "string"
    },
    luaCutscene = {
        fieldType = "string"  
    },
    csEventID = {
        fieldType = "string"
    },
    XSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 500.0
    },
    YSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 500.0
    }
}

function dialogNPC.sprite(room, entity)
    local texture = entity.sprite or "characters/Kglobal::Player/idle00"
    local color = {0.2, 0.8, 0.2, 1.0}
    
    if entity.isHostile then
        color = {0.8, 0.1, 0.1, 1.0}
    elseif entity.cutsceneModeEnabled then
        color = {0.8, 0.2, 0.8, 1.0}
    end
    
    return {
        {
            texture = texture,
            x = entity.x,
            y = entity.y,
            color = color
        }
    }
end

function dialogNPC.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return dialogNPC
