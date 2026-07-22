local rainbowBlackholeTrigger = {}

rainbowBlackholeTrigger.name = "DZ/RainbowBlackholeTrigger"
rainbowBlackholeTrigger.depth = 0
rainbowBlackholeTrigger.fieldInformation = {
    strength = {
        options = { "Mild", "Medium", "High", "Wild", "Insane" },
        editable = false
    }
}

rainbowBlackholeTrigger.placements = {
    name = "rainbow_blackhole_trigger",
    data = {
        action = "Enable",
        strength = "Medium",
        alpha = 1,
        scale = 1,
        direction = 1,
        triggerOnce = false,
        fadeTime = 1,
        flag = "",
        onlyIfFlag = false
    }
}

return rainbowBlackholeTrigger
