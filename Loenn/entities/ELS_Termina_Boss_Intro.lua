local utils = require("utils")
local npcSessionFlags = require("ui.forms.fields.npc_session_flags")

local elsTerminaBossIntro = {}

elsTerminaBossIntro.name = "MaggyHelper/ELS_Termina_Boss_Intro"
elsTerminaBossIntro.depth = 0
elsTerminaBossIntro.justification = {0.5, 1.0}
elsTerminaBossIntro.texture = "objects/Ingeste/sampleEntity/idle00"

elsTerminaBossIntro.placements = {
    {
        name = "default",
        data = {
            activationMode = "touch",
            requireFlag = "",
            completionFlag = "ch21_els_termina_boss_intro",
            removeAfterTrigger = true,
            showSprite = false,
            texturePath = "objects/Ingeste/sampleEntity/idle00"
        }
    }
}

elsTerminaBossIntro.fieldInformation = {
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

elsTerminaBossIntro.fieldOrder = {
    "x",
    "y",
    "activationMode",
    "requireFlag",
    "completionFlag",
    "removeAfterTrigger",
    "showSprite",
    "texturePath"
}

function elsTerminaBossIntro.texture(room, entity)
    return entity.texturePath or "objects/Ingeste/sampleEntity/idle00"
end

function elsTerminaBossIntro.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return elsTerminaBossIntro
