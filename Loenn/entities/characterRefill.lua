local characterRefill = {}

characterRefill.name = "DZ/CharacterRefill"
characterRefill.depth = 0
characterRefill.texture = "objects/DZ/DZ/DZ/character_refill"

characterRefill.placements = {
    name = "character_refill",
    data = {
        customSpritePath = "",
        customSoundEvent = "",
        oneUse = false,
        characterModeOnly = false,
        refillStamina = true,
        characterType = 0,
        dashCount = 1
    }
}

return characterRefill