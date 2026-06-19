-- Pink Game Boy Color Grade Trigger for Lönn
local pinkGameboyTrigger = {}

pinkGameboyTrigger.name = "DZ/PinkGameboyColorGradeTrigger"

pinkGameboyTrigger.placements = {
    {
        name = "main",
        data = {
            flagToSet = "pink_gameboy_activated",
            colorGradeName = "pinkgameboy",
            triggerOnce = true,
            transitionDuration = 0.5,
            playSound = true
        }
    }
}

pinkGameboyTrigger.fieldInformation = {
    flagToSet = {
        fieldType = "string"
    },
    colorGradeName = {
        fieldType = "string",
        options = {
            "pinkgameboy",
            "golden",
            "none",
            "cold",
            "hot",
            "oldsite",
            "panicattack",
            "feelingdown",
            "templevoid",
            "credits",
            "reflection"
        }
    },
    transitionDuration = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0
    }
}

return pinkGameboyTrigger
