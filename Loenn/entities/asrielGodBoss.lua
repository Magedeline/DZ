local asrielGodBoss = {}

asrielGodBoss.name = "DZ/AsrielGodBoss"
asrielGodBoss.depth = 0
asrielGodBoss.nodeLineRenderType = "line"
asrielGodBoss.nodeLimits = {0, -1}
asrielGodBoss.texture = "characters/DZ/asrielgodboss/idle00"

asrielGodBoss.placements = {
    name = "asriel_god_boss",
    data = {
        patternIndex = 0,
        cameraPastY = 120.0,
        dialog = false,
        startHit = false,
        cameraLockY = true,
        attackSequence = "",
        azzyboss15DialogDoNotWantToFight = "DZ_CH20_KIRBY_DO_NOT_WANT_TO_FIGHT",
        azzyboss15DialogFirstSpecialAttack = "DZ_CH20_ASRIEL_FIRST_SPECIAL_ATTACK",
        azzyboss15DialogMadelineBadelineSaveKirby = "DZ_CH20_MADELINE_AND_BADELINE_SAVE_KIRBY_FROM_ASRIEL_FIRST_SPECIAL_ATTACK",
    }
}

asrielGodBoss.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
    },
    cameraPastY = {
        fieldType = "number",
    },
    azzyboss15DialogDoNotWantToFight = {
        options = {"DZ_CH20_KIRBY_DO_NOT_WANT_TO_FIGHT"},
        editable = true
    },
    azzyboss15DialogFirstSpecialAttack = {
        options = {"DZ_CH20_ASRIEL_FIRST_SPECIAL_ATTACK"},
        editable = true
    },
    azzyboss15DialogMadelineBadelineSaveKirby = {
        options = {"DZ_CH20_MADELINE_AND_BADELINE_SAVE_KIRBY_FROM_ASRIEL_FIRST_SPECIAL_ATTACK"},
        editable = true
    },
}

return asrielGodBoss
