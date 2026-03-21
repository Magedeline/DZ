local kirbyTutorialBirdTrigger = {}

kirbyTutorialBirdTrigger.name = "MaggyHelper/KirbyTutorialBirdTrigger"
kirbyTutorialBirdTrigger.fieldInformation = {
    tutorialIndex = {
        fieldType = "integer"
    }
}

kirbyTutorialBirdTrigger.placements = {
    {
        name = "tutorial_bird_trigger",
        data = {
            birdId = "",
            tutorialIndex = 0,
            conditionFunction = "",
            width = 8,
            height = 8
        }
    },
    {
        name = "show_when_aqua_hook_fixed",
        data = {
            birdId = "kirby_aqua_intro",
            tutorialIndex = 1,
            conditionFunction = "mod:MaggyHelper.Extensions.Kirby.ModCompat.AquaTutorialCompat.IsAquaHookFixed",
            width = 8,
            height = 8
        }
    },
    {
        name = "show_when_kirby_aqua_swinging",
        data = {
            birdId = "kirby_aqua_swing",
            tutorialIndex = 1,
            conditionFunction = "mod:MaggyHelper.Extensions.Kirby.ModCompat.AquaTutorialCompat.IsKirbyAquaSwinging",
            width = 8,
            height = 8
        }
    },
    {
        name = "close_when_aqua_attracted",
        data = {
            birdId = "kirby_aqua_swing",
            tutorialIndex = -1,
            conditionFunction = "mod:MaggyHelper.Extensions.Kirby.ModCompat.AquaTutorialCompat.IsKirbyAquaAttracted",
            width = 8,
            height = 8
        }
    }
}

return kirbyTutorialBirdTrigger