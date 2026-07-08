local kirbyDamageTrigger = {}

kirbyDamageTrigger.name = "DZ/KirbyDamageTrigger"
kirbyDamageTrigger.depth = 0
kirbyDamageTrigger.placements = {
    name = "kirby_damage_trigger",
    data = {
        oncePerPlayer = false,
        damage = 1,
        cooldown = 0.5
    }
}

return kirbyDamageTrigger