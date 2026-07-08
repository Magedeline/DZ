local damageTrigger = {}

damageTrigger.name = "DZ/DamageTrigger"
damageTrigger.depth = 0
damageTrigger.placements = {
    name = "damage_trigger",
    data = {
        damage = 1,
        cooldown = 1,
        removeAfterHit = false
    }
}

return damageTrigger
