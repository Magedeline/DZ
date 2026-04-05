local utils = require("utils")
local npcSessionFlags = require("ui.forms.fields.npc_session_flags")
local eventOptions = require("libraries.cutscene_event_ids")

local cutsceneEventEntity = {}

cutsceneEventEntity.name = "DesoloZantas/CutsceneEventEntity"
cutsceneEventEntity.depth = 0
cutsceneEventEntity.justification = {0.5, 1.0}
cutsceneEventEntity.texture = "objects/Ingeste/sampleEntity/idle00"

cutsceneEventEntity.placements = {
    {
        name = "interact",
        data = {
            eventId = "",
            activationMode = "interact",
            requireFlag = "",
            completionFlag = "",
            removeAfterTrigger = true,
            showSprite = false,
            texturePath = "objects/Ingeste/sampleEntity/idle00"
        }
    },
    {
        name = "touch",
        data = {
            eventId = "",
            activationMode = "touch",
            requireFlag = "",
            completionFlag = "",
            removeAfterTrigger = true,
            showSprite = false,
            texturePath = "objects/Ingeste/sampleEntity/idle00"
        }
    },
    {
        name = "room_enter",
        data = {
            eventId = "",
            activationMode = "roomEnter",
            requireFlag = "",
            completionFlag = "",
            removeAfterTrigger = true,
            showSprite = false,
            texturePath = "objects/Ingeste/sampleEntity/idle00"
        }
    }
}

cutsceneEventEntity.fieldInformation = {
    eventId = {
        fieldType = "string",
        editable = true,
        options = eventOptions
    },
    activationMode = {
        fieldType = "string",
        editable = false,
        options = {
            "interact",
            "touch",
            "roomEnter"
        }
    },
    requireFlag = npcSessionFlags,
    completionFlag = npcSessionFlags,
    removeAfterTrigger = {
        fieldType = "boolean"
    },
    showSprite = {
        fieldType = "boolean"
    },
    texturePath = {
        fieldType = "string"
    }
}

cutsceneEventEntity.fieldOrder = {
    "x",
    "y",
    "eventId",
    "activationMode",
    "requireFlag",
    "completionFlag",
    "removeAfterTrigger",
    "showSprite",
    "texturePath"
}

function cutsceneEventEntity.texture(room, entity)
    return entity.texturePath or "objects/Ingeste/sampleEntity/idle00"
end

function cutsceneEventEntity.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return cutsceneEventEntity
