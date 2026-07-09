local kirbyFinalBattleScene = {}

kirbyFinalBattleScene.name = "DZ/KirbyFinalBattleScene"
kirbyFinalBattleScene.depth = -10000
kirbyFinalBattleScene.texture = "objects/DZ/warpstars/idle00"

kirbyFinalBattleScene.placements = {
    name = "kirby_final_battle_scene",
    data = {
        totalZeroForms = 6,
        healthPerZeroForm = 80.0,
        hardMode = false,
        warpStarBobAmplitude = 3.0,
        warpStarBobSpeed = 2.5,
        warpStarRideDuration = 8.0,
        phase2ScrollSpeed = 350.0,
        allyFormationOffsetX = -40.0,
        allyFormationOffsetY = -20.0,
        allySpacing = 26.0,
        activeAllies = "Kirby,Madeline,Badeline,Asriel,Magolor,BandanaDee,Marx,Gooey,Susie",
        victoryMusicEvent = "event:/DZ/new_content/music/lvl21/victory",
        completionFlag = "ch21_els_termina_final_battle_done"
    }
}

kirbyFinalBattleScene.fieldInformation = {
    totalZeroForms = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 6
    },
    healthPerZeroForm = {
        minimumValue = 1.0
    },
    warpStarBobAmplitude = {
        minimumValue = 0.0
    },
    warpStarBobSpeed = {
        minimumValue = 0.0
    },
    warpStarRideDuration = {
        minimumValue = 0.0
    },
    phase2ScrollSpeed = {
        minimumValue = 0.0
    },
    allySpacing = {
        minimumValue = 0.0
    }
}

return kirbyFinalBattleScene
