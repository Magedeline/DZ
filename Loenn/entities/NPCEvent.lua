local npcDialogKeys = require("ui.forms.fields.npc_dialog_keys")
local npcSessionFlags = require("ui.forms.fields.npc_session_flags")

local npcEvent = {}

local spriteOptions = {
    "theo",
    "chara",
    "kirby",
    "ralsei",
    "madeline",
    "badeline",
    "maggy",
    "magolor",
    "granny",
    "oshiro",
    "toriel",
    "asriel",
    "meta_knight",
    "roxus",
    "temmie",
    "axis",
    "els",
    "digital_guide",
    "phone",
    "titan_council_member"
}

local textureMap = {
    theo = "characters/Maggy/DesoloZantas/theo/idle00",
    chara = "characters/Maggy/DesoloZantas/chara/idle00",
    kirby = "characters/Maggy/DesoloZantas/kirby/idle00",
    ralsei = "characters/Maggy/DesoloZantas/ralsei/idle00",
    madeline = "characters/Maggy/DesoloZantas/madeline/idle00",
    badeline = "characters/badeline/idle00",
    maggy = "characters/Maggy/DesoloZantas/maggy/idle00",
    magolor = "characters/Maggy/DesoloZantas/magolor/idle00",
    granny = "characters/Maggy/DesoloZantas/granny/idle00",
    oshiro = "characters/Maggy/DesoloZantas/oshiro/oshiro00",
    toriel = "characters/Maggy/DesoloZantas/toriel/idle00",
    asriel = "characters/Maggy/DesoloZantas/asriel/idle00",
    meta_knight = "characters/metaknight/idle00",
    roxus = "characters/roxus/idle00",
    temmie = "characters/temmie/idle00",
    axis = "characters/Maggy/DesoloZantas/axis/idle00",
    els = "characters/Maggy/DesoloZantas/els/idle00",
    digital_guide = "characters/digitalguide/idle00",
    phone = "characters/phone/idle00",
    titan_council_member = "characters/titancouncil/idle00"
}

npcEvent.name = "MaggyHelper/NPC_Event"
npcEvent.depth = 1000
npcEvent.justification = {0.5, 1.0}
npcEvent.texture = "characters/Maggy/DesoloZantas/theo/idle00"

npcEvent.placements = {
    {
        name = "default",
        data = {
            dialogKey = "",
            flagName = "",
            spriteId = "theo",
            eventId = ""
        }
    },
    {
        name = "theo",
        data = {
            dialogKey = "CH0_THEO_A",
            flagName = "ch0_theo_npc_talked",
            spriteId = "theo",
            eventId = ""
        }
    },
    {
        name = "maggy",
        data = {
            dialogKey = "CH1_MAGGY_A",
            flagName = "ch1_maggy_npc_talked",
            spriteId = "maggy",
            eventId = ""
        }
    },
    {
        name = "chara",
        data = {
            dialogKey = "CH2_CHARA_INTRO",
            flagName = "ch2_chara_npc_talked",
            spriteId = "chara",
            eventId = ""
        }
    }
}

npcEvent.fieldInformation = {
    dialogKey = npcDialogKeys,
    flagName = npcSessionFlags,
    spriteId = {
        fieldType = "string",
        editable = true,
        options = spriteOptions
    },
    eventId = {
        fieldType = "string"
    }
}

function npcEvent.texture(room, entity)
    local spriteId = entity.spriteId or "theo"
    return textureMap[spriteId] or textureMap.theo
end

return npcEvent


