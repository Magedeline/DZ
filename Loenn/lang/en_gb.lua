-- Loenn language file for MaggyHelper entities and triggers

return {
    -- Entity names
    entities = {
        ["MaggyHelper/KirbySpawnPoint"] = {
            placements = {
                default = "Kirby Spawn Point"
            },
            attributes = {
                spawnAsKirby = "Spawn as Kirby",
                startingAbility = "Starting Ability"
            }
        },
        ["MaggyHelper/AbilityStar"] = {
            placements = {
                fire = "Ability Star (Fire)",
                ice = "Ability Star (Ice)",
                sword = "Ability Star (Sword)",
                beam = "Ability Star (Beam)",
                spark = "Ability Star (Spark)",
                stone = "Ability Star (Stone)",
                bomb = "Ability Star (Bomb)",
                hammer = "Ability Star (Hammer)",
                ninja = "Ability Star (Ninja)",
                cutter = "Ability Star (Cutter)"
            },
            attributes = {
                ability = "Copy Ability Type"
            }
        },
        ["MaggyHelper/KirbyBoss"] = {
            placements = {
                default = "Shadow Kirby Boss"
            },
            attributes = {
                health = "Boss Health",
                attackCooldown = "Attack Cooldown",
                bossMusic = "Boss Music Event"
            }
        },
        ["MaggyHelper/DededeBoss"] = {
            placements = {
                default = "King Dedede Boss"
            },
            attributes = {
                health = "Boss Health",
                attackCooldown = "Attack Cooldown",
                bossMusic = "Boss Music Event"
            }
        },
        ["MaggyHelper/MetaKnightBoss"] = {
            placements = {
                default = "Meta Knight Boss"
            },
            attributes = {
                health = "Boss Health",
                attackCooldown = "Attack Cooldown",
                bossMusic = "Boss Music Event"
            }
        },
        ["MaggyHelper/GalactaKnightClone"] = {
            placements = {
                default = "Galacta Knight Clone"
            },
            attributes = {
                health = "Boss Health",
                moveSpeed = "Move Speed",
                attackCooldown = "Attack Cooldown",
                orbitRadius = "Orbit Radius",
                dashSpeed = "Dash Speed",
                playMusicOnStart = "Play Music On Start",
                bossMusic = "Boss Music Event",
                spritePath = "Sprite Root Path",
                idleAnimationPath = "Idle Animation Path",
                moveAnimationPath = "Move Animation Path",
                chargeAnimationPath = "Charge Animation Path",
                slashAnimationPath = "Slash Animation Path",
                warpAnimationPath = "Warp Animation Path"
            }
        },
        ["MaggyHelper/GalacticKnightClone"] = {
            placements = {
                default = "Galacta Knight Clone"
            },
            attributes = {
                health = "Boss Health",
                moveSpeed = "Move Speed",
                attackCooldown = "Attack Cooldown",
                orbitRadius = "Orbit Radius",
                dashSpeed = "Dash Speed",
                playMusicOnStart = "Play Music On Start",
                bossMusic = "Boss Music Event",
                spritePath = "Sprite Root Path",
                idleAnimationPath = "Idle Animation Path",
                moveAnimationPath = "Move Animation Path",
                chargeAnimationPath = "Charge Animation Path",
                slashAnimationPath = "Slash Animation Path",
                warpAnimationPath = "Warp Animation Path"
            }
        },
        ["MaggyHelper/MorphoKnightClone"] = {
            placements = {
                default = "Morpho Knight Clone"
            },
            attributes = {
                health = "Boss Health",
                moveSpeed = "Move Speed",
                attackCooldown = "Attack Cooldown",
                orbitRadius = "Orbit Radius",
                dashSpeed = "Dash Speed",
                playMusicOnStart = "Play Music On Start",
                bossMusic = "Boss Music Event",
                spritePath = "Sprite Root Path",
                idleAnimationPath = "Idle Animation Path",
                moveAnimationPath = "Move Animation Path",
                chargeAnimationPath = "Charge Animation Path",
                slashAnimationPath = "Slash Animation Path",
                warpAnimationPath = "Warp Animation Path"
            }
        },
        ["MaggyHelper/WaddleDee"] = {
            placements = {
                default = "Waddle Dee"
            },
            attributes = {
                health = "Health",
                moveSpeed = "Move Speed",
                patrolDistance = "Patrol Distance",
                canBeInhaled = "Can Be Inhaled"
            }
        },
        ["MaggyHelper/WaddleDoo"] = {
            placements = {
                default = "Waddle Doo"
            },
            attributes = {
                health = "Health",
                moveSpeed = "Move Speed",
                attackCooldown = "Attack Cooldown",
                canBeInhaled = "Can Be Inhaled"
            }
        },
        ["MaggyHelper/Gordo"] = {
            placements = {
                stationary = "Gordo (Stationary)",
                horizontal = "Gordo (Horizontal)",
                vertical = "Gordo (Vertical)",
                diagonal = "Gordo (Diagonal)",
                circular = "Gordo (Circular)"
            },
            attributes = {
                movementType = "Movement Type",
                moveDistance = "Move Distance",
                moveSpeed = "Move Speed",
                pauseDuration = "Pause Duration"
            }
        },
        ["MaggyHelper/ScarfyEnemy"] = {
            placements = {
                default = "Scarfy"
            },
            attributes = {
                health = "Health",
                moveSpeed = "Move Speed",
                chaseSpeed = "Chase Speed",
                canBeInhaled = "Can Be Inhaled"
            }
        },
        ["MaggyHelper/KirbyTutorialBird"] = {
            placements = {
                tutorial_bird = "Kirby Tutorial Bird",
                aqua_hook_intro = "Kirby Tutorial Bird (Aqua Hook Intro)",
                aqua_hook_swing = "Kirby Tutorial Bird (Aqua Hook Swing)"
            },
            attributes = {
                birdId = "Bird ID",
                dialogs = "Tutorial Dialog IDs",
                controls = "Tutorial Controls",
                startupIndex = "Startup Tutorial Index",
                triggerOnce = "Trigger Each Tutorial Once",
                faceLeft = "Face Left",
                caw = "Caw Before Showing Tutorial",
                onlyOnce = "Disappear Permanently After Closing"
            }
        },
        ["MaggyHelper/AscendManagerBeyond"] = {
            placements = {
                AscendManagerBeyond = "Ascend Manager (Normal)",
                AscendManagerBeyond_dark = "Ascend Manager (Dark)",
                AscendManagerBeyond_ch19_ending = "Ascend Manager (CH19 Ending)"
            },
            attributes = {
                width = "Width",
                height = "Height",
                index = "Index",
                cutscene = "Cutscene ID",
                intro_launch = "Intro Launch",
                dark = "Dark Mode",
                arrivial = "Arrival Mode",
                ambience = "Ambience Event"
            }
        }
    },
    
    -- Trigger names
    triggers = {
        ["MaggyHelper/BossFightTrigger"] = {
            placements = {
                default = "Boss Fight Trigger"
            },
            attributes = {
                bossType = "Boss Type",
                lockRoom = "Lock Room",
                playMusic = "Play Boss Music",
                bossMusic = "Boss Music Event"
            }
        },
        ["MaggyHelper/KirbyAbilityTrigger"] = {
            placements = {
                give_ability = "Kirby Ability (Give)",
                remove_ability = "Kirby Ability (Remove)",
                toggle_float = "Kirby Ability (Toggle Float)",
                toggle_inhale = "Kirby Ability (Toggle Inhale)"
            },
            attributes = {
                action = "Action",
                ability = "Ability",
                onlyOnce = "Only Once"
            }
        },
        ["MaggyHelper/EnemySpawnTrigger"] = {
            placements = {
                waddle_dee = "Enemy Spawn (Waddle Dee)",
                waddle_doo = "Enemy Spawn (Waddle Doo)",
                gordo = "Enemy Spawn (Gordo)",
                scarfy = "Enemy Spawn (Scarfy)"
            },
            attributes = {
                enemyType = "Enemy Type",
                count = "Spawn Count",
                spawnDelay = "Spawn Delay",
                respawn = "Respawn on Death"
            }
        },
        ["MaggyHelper/BridgeFreezeTrigger"] = {
            placements = {
                default = "Bridge Freeze (Nearly Frozen)",
                slow_motion = "Bridge Freeze (Slow Motion)"
            },
            attributes = {
                freezeStrength = "Freeze Strength (0.001 = nearly frozen, 1.0 = normal)"
            }
        },
        ["MaggyHelper/KirbyTutorialBirdTrigger"] = {
            placements = {
                tutorial_bird_trigger = "Kirby Tutorial Bird Trigger",
                show_when_aqua_hook_fixed = "Kirby Tutorial Trigger (Aqua Hook Fixed)",
                show_when_kirby_aqua_swinging = "Kirby Tutorial Trigger (Kirby Aqua Swinging)",
                close_when_aqua_attracted = "Kirby Tutorial Trigger (Close On Aqua Attracted)"
            },
            attributes = {
                birdId = "Target Bird ID",
                tutorialIndex = "Tutorial Index (-1 to Close)",
                conditionFunction = "Condition Function (mod:...)",
                width = "Width",
                height = "Height"
            }
        }
    }
}
