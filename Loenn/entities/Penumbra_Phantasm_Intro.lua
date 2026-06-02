local utils = require("utils")
local npcSessionFlags = require("ui.forms.fields.npc_session_flags")

local penumbraPhantasmIntro = {}

penumbraPhantasmIntro.name = "MaggyHelper/Penumbra_Phantasm_Intro"
penumbraPhantasmIntro.depth = 0
penumbraPhantasmIntro.justification = {0.5, 1.0}
penumbraPhantasmIntro.texture = "objects/Ingeste/sampleEntity/idle00"

penumbraPhantasmIntro.placements = {
    {
        name = "default",
        data = {
            activationMode = "touch",
            requireFlag = "",
            completionFlag = "ch20_penumbra_phantasm_intro",
            removeAfterTrigger = true,
            showSprite = false,
            texturePath = "objects/Ingeste/sampleEntity/idle00"
        }
    }
}

penumbraPhantasmIntro.fieldInformation = {
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

penumbraPhantasmIntro.fieldOrder = {
    "x",
    "y",
    "activationMode",
    "requireFlag",
    "completionFlag",
    "removeAfterTrigger",
    "showSprite",
    "texturePath"
}

function penumbraPhantasmIntro.texture(room, entity)
    return entity.texturePath or "objects/Ingeste/sampleEntity/idle00"
end

function penumbraPhantasmIntro.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return penumbraPhantasmIntro
