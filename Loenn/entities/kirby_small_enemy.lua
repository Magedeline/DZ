local kirbySmallEnemy = {}

kirbySmallEnemy.name = "DZ/KirbySmallEnemy"
kirbySmallEnemy.depth = -100

kirbySmallEnemy.fieldInformation = {
    variant = {
        options = {
            ["Waddle Dee"] = 0,
            ["Waddle Doo"] = 1,
            ["Hot Head"] = 2,
            ["Chilly"] = 3,
            ["Sparky"] = 4,
            ["Rocky"] = 5,
            ["Sir Kibble"] = 6,
            ["Poppy"] = 7,
            ["Wheelie"] = 8,
            ["Needlous"] = 9,
            ["Simirror"] = 10,
            ["Scarfy"] = 11,
            ["Gordo"] = 12
        },
        editable = false
    },
    powerType = {
        options = {
            ["None"] = 0,
            ["Fire"] = 1,
            ["Ice"] = 2,
            ["Spark"] = 3,
            ["Stone"] = 4,
            ["Sword"] = 5,
            ["Beam"] = 6,
            ["Cutter"] = 7,
            ["Bomb"] = 8,
            ["Wheel"] = 9,
            ["Needle"] = 10,
            ["Mirror"] = 11
        },
        editable = false
    }
}

kirbySmallEnemy.placements = {
    {
        name = "Waddle Dee",
        data = {
            variant = 0,
            powerType = 0,
            maxHealth = 1,
            moveSpeed = 30,
            canBeInhaled = true,
            detectionRange = 100,
            attackRange = 30,
            facingRight = true,
            patrolDistance = 64
        }
    },
    {
        name = "Waddle Doo (Beam)",
        data = {
            variant = 1,
            powerType = 6,
            maxHealth = 2,
            moveSpeed = 25,
            canBeInhaled = true,
            detectionRange = 100,
            attackRange = 80,
            facingRight = true,
            patrolDistance = 64
        }
    },
    {
        name = "Hot Head (Fire)",
        data = {
            variant = 2,
            powerType = 1,
            maxHealth = 2,
            moveSpeed = 35,
            canBeInhaled = true,
            detectionRange = 100,
            attackRange = 60,
            facingRight = true,
            patrolDistance = 64
        }
    },
    {
        name = "Chilly (Ice)",
        data = {
            variant = 3,
            powerType = 2,
            maxHealth = 2,
            moveSpeed = 20,
            canBeInhaled = true,
            detectionRange = 100,
            attackRange = 40,
            facingRight = true,
            patrolDistance = 64
        }
    },
    {
        name = "Sparky (Spark)",
        data = {
            variant = 4,
            powerType = 3,
            maxHealth = 2,
            moveSpeed = 40,
            canBeInhaled = true,
            detectionRange = 100,
            attackRange = 50,
            facingRight = true,
            patrolDistance = 64
        }
    },
    {
        name = "Rocky (Stone)",
        data = {
            variant = 5,
            powerType = 4,
            maxHealth = 4,
            moveSpeed = 15,
            canBeInhaled = true,
            detectionRange = 100,
            attackRange = 30,
            facingRight = true,
            patrolDistance = 32
        }
    },
    {
        name = "Sir Kibble (Cutter)",
        data = {
            variant = 6,
            powerType = 7,
            maxHealth = 2,
            moveSpeed = 30,
            canBeInhaled = true,
            detectionRange = 100,
            attackRange = 100,
            facingRight = true,
            patrolDistance = 64
        }
    },
    {
        name = "Poppy (Bomb)",
        data = {
            variant = 7,
            powerType = 8,
            maxHealth = 2,
            moveSpeed = 25,
            canBeInhaled = true,
            detectionRange = 100,
            attackRange = 70,
            facingRight = true,
            patrolDistance = 64
        }
    },
    {
        name = "Wheelie (Wheel)",
        data = {
            variant = 8,
            powerType = 9,
            maxHealth = 3,
            moveSpeed = 80,
            canBeInhaled = true,
            detectionRange = 180,
            attackRange = 30,
            facingRight = true,
            patrolDistance = 128
        }
    },
    {
        name = "Gordo (Invincible)",
        data = {
            variant = 12,
            powerType = 0,
            maxHealth = 999,
            moveSpeed = 0,
            canBeInhaled = false,
            detectionRange = 0,
            attackRange = 0,
            facingRight = true,
            patrolDistance = 0
        }
    }
}

-- Texture based on variant
function kirbySmallEnemy.texture(room, entity)
    local variant = entity.variant or 0
    local textures = {
        [0] = "characters/kirby/waddle_dee/idle00",
        [1] = "characters/kirby/waddle_doo/idle00",
        [2] = "characters/kirby/hot_head/idle00",
        [3] = "characters/kirby/chilly/idle00",
        [4] = "characters/kirby/sparky/idle00",
        [5] = "characters/kirby/rocky/idle00",
        [6] = "characters/kirby/sir_kibble/idle00",
        [7] = "characters/kirby/poppy/idle00",
        [8] = "characters/kirby/wheelie/idle00",
        [9] = "characters/kirby/needlous/idle00",
        [10] = "characters/kirby/simirror/idle00",
        [11] = "characters/kirby/scarfy/idle00",
        [12] = "characters/kirby/gordo/idle00"
    }
    return textures[variant] or "characters/Kglobal::Player/sitDown00"
end

return kirbySmallEnemy
