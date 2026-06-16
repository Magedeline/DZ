local dialogNPC = require("utils").deepcopy(require("mods").requireFromPlugin("entities.customNPC", "DoonvHelper"))

dialogNPC.name = "DZ/DialogNPC"

-- Enhanced field information to match C# DialogNPC, including all flags
for k, v in pairs({
    talkBoundsWidth = { fieldType = "integer", minimumValue = 1, tooltip = "Width of the talk interaction bounds" },
    talkBoundsHeight = { fieldType = "integer", minimumValue = 1, tooltip = "Height of the talk interaction bounds" },
    talkIndicatorX = { fieldType = "number" },
    talkIndicatorY = { fieldType = "number" },
    basicDialogID = { fieldType = "string" },
    luaCutscene = { fieldType = "string" },
    csEventID = { fieldType = "string", tooltip = "C# Event ID for enhanced cutscene activation" },
    cutsceneModeEnabled = { fieldType = "boolean", tooltip = "Enable cutscene mode for this DialogNPC" },

    -- NPC Flags
    isActive = { fieldType = "boolean", tooltip = "true if the NPC is currently active in the scene" },
    isVisible = { fieldType = "boolean", tooltip = "true if the NPC is currently visible" },
    isInteractable = { fieldType = "boolean", tooltip = "true if the NPC can be interacted with" },
    isMoving = { fieldType = "boolean", tooltip = "true if the NPC is currently moving" },
    isPatrolling = { fieldType = "boolean", tooltip = "true if the NPC is currently in a patrol state" },
    isTalking = { fieldType = "boolean", tooltip = "true if the NPC is currently in a talking state" },
    isInCutscene = { fieldType = "boolean", tooltip = "true if the NPC is currently in a cutscene state" },
    isStunned = { fieldType = "boolean", tooltip = "true if the NPC is currently stunned or unable to act" },
    isHostile = { fieldType = "boolean", tooltip = "true if the NPC is currently hostile to the Kglobal::Player" },
    isFriendly = { fieldType = "boolean", tooltip = "true if the NPC is currently friendly to the Kglobal::Player" },
    isFollowingKglobal::global::Celeste.Player= { fieldType = "boolean", tooltip = "true if the NPC is currently following the Kglobal::Player" },
    isInvincible = { fieldType = "boolean", tooltip = "true if the NPC is currently invincible" },
    isGrounded = { fieldType = "boolean", tooltip = "true if the NPC is currently grounded" },
    isAirborne = { fieldType = "boolean", tooltip = "true if the NPC is currently airborne" },
    aiEnabled = { fieldType = "boolean", tooltip = "true if the NPC is currently enabled for AI updates" },

    -- Cutscene (CS) Flags
    cutsceneActive = { fieldType = "boolean", tooltip = "true if a cutscene is currently playing for this NPC" },
    cutsceneSkippable = { fieldType = "boolean", tooltip = "true if the cutscene can be skipped by the Kglobal::Player" },
    cutscenePlayed = { fieldType = "boolean", tooltip = "true if the cutscene has already been played" },
    cutscenePaused = { fieldType = "boolean", tooltip = "true if the cutscene is currently paused" },
    cutsceneWaitingForInput = { fieldType = "boolean", tooltip = "true if the cutscene is waiting for Kglobal::global::Celeste.Playerinput to continue" },
    cutsceneLocked = { fieldType = "boolean", tooltip = "true if the cutscene is currently locked (cannot be interrupted)" }
}) do dialogNPC.fieldInformation[k] = v end

-- Enhanced placements data to match C# constructor parameters and all flags
for k, v in pairs({
    -- Talk bounds (matching Point talkBoundsSize)
    talkBoundsWidth = 80,
    talkBoundsHeight = 40,

    -- Talk indicator offset (matching Vector2 talkIndicatorOffset)
    talkIndicatorX = 0,
    talkIndicatorY = 0,

    -- Dialog and event IDs
    basicDialogID = "",
    luaCutscene = "",
    csEventID = "",

    -- Cutscene mode (matching cutsceneModeEnabled)
    cutsceneModeEnabled = false,

    -- NPC Flags (default values from C#)
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

    -- Cutscene (CS) Flags (default values from C#)
    cutsceneActive = false,
    cutsceneSkippable = true,
    cutscenePlayed = false,
    cutscenePaused = false,
    cutsceneWaitingForInput = false,
    cutsceneLocked = false
}) do dialogNPC.placements.data[k] = v end

return dialogNPC