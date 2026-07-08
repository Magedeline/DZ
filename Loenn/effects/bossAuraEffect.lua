local bossAuraEffect = {}

bossAuraEffect.name = "DZ/BossAuraEffect"
bossAuraEffect.depth = 10000
bossAuraEffect.texture = "objects/DZ/DZ/DZ/effects/bossaura"
bossAuraEffect.placements = {
    name = "boss_aura_effect",
    data = {
        auraColor = "red",
        intensity = 1.0
    }
}

return bossAuraEffect