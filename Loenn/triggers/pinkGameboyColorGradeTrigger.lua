local pinkGameboyColorGradeTrigger = {}

pinkGameboyColorGradeTrigger.name = "DZ/PinkGameboyColorGradeTrigger"
pinkGameboyColorGradeTrigger.depth = 0
pinkGameboyColorGradeTrigger.placements = {
    name = "pink_gameboy_color_grade_trigger",
    data = {
        flagToSet = "pink_gameboy_activated",
        colorGradeName = "pinkgameboy",
        triggerOnce = true,
        transitionDuration = 0.5,
        playSound = true
    }
}

return pinkGameboyColorGradeTrigger
