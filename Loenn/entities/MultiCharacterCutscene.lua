local multi_character_cutscene = {}

multi_character_cutscene.name = "DesoloZantas/MultiCharacterCutscene"
multi_character_cutscene.depth = 0
multi_character_cutscene.justification = {0.5, 0.5}
multi_character_cutscene.texture = "scenery/memorial/memorial"
multi_character_cutscene.nodeLimits = {0, 0}

multi_character_cutscene.placements = {
    {
        name = "main",
        data = {
            cutsceneId = "DZ_CH0_MODINTRO",
            autoStart = true
        }
    },
    {
        name = "Chapter End Cutscene",
        data = {
            cutsceneId = "DZ_CH0_END",
            autoStart = false
        }
    },
    {
        name = "Madeline Rest Cutscene",
        data = {
            cutsceneId = "DZ_CH1_ENDMADELINE",
            autoStart = false
        }
    },
    {
        name = "Nightmare Intro Cutscene",
        data = {
            cutsceneId = "DZ_CH2_INTRO",
            autoStart = true
        }
    },
    {
        name = "Wake Up Cutscene",
        data = {
            cutsceneId = "DZ_CH2_WAKEUP",
            autoStart = false
        }
    },
    {
        name = "Memorial Darkside",
        data = {
            cutsceneId = "MEMORIAL_DARKSIDE",
            autoStart = false
        }
    },
    {
        name = "Poem Cutscene",
        data = {
            cutsceneId = "DZ_CH2_POEM",
            autoStart = false
        }
    }
}

multi_character_cutscene.fieldInformation = {
    cutsceneId = {
        fieldType = "string",
        options = {
            "DZ_CH0_MODINTRO",
            "DZ_CH0_END",
            "DZ_CH1_ENDMADELINE",
            "DZ_CH2_INTRO",
            "DZ_CH2_WAKEUP",
            "MEMORIAL_DARKSIDE",
            "DZ_CH2_POEM"
        }
    },
    autoStart = {
        fieldType = "boolean"
    }
}

return multi_character_cutscene