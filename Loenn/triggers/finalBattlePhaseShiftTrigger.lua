local finalBattlePhaseShiftTrigger = {}

finalBattlePhaseShiftTrigger.name = "DZ/FinalBattlePhaseShiftTrigger"
finalBattlePhaseShiftTrigger.depth = 0
finalBattlePhaseShiftTrigger.placements = {
    name = "final_battle_phase_shift_trigger",
    data = {
        width = 8,
        height = 8,
        phaseIndex = 0,
        colorGrade = "",
        triggerOnce = true,
        requiredFlag = "",
        setFlag = "",
        shakeScreen = true,
        shakeIntensity = 0.3,
        flashScreen = true,
        flashAlpha = 0.4,
        scrollSpeedBoost = 0.0
    }
}

finalBattlePhaseShiftTrigger.fieldInformation = {
    phaseIndex = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 4
    },
    shakeIntensity = {
        minimumValue = 0.0
    },
    flashAlpha = {
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    scrollSpeedBoost = {
        minimumValue = 0.0
    }
}

return finalBattlePhaseShiftTrigger
