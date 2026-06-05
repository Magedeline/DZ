local enhancedBirdNPC = {}

enhancedBirdNPC.name = "MaggyHelper/KirbyTutorialBird"
enhancedBirdNPC.depth = -1000000
enhancedBirdNPC.nodeLineRenderType = "line"
enhancedBirdNPC.justification = {0.5, 1.0}
enhancedBirdNPC.texture = "characters/bird/crow00"
enhancedBirdNPC.nodeLimits = {0, -1}

enhancedBirdNPC.fieldInformation = {
    onlyOnce = {
        fieldType = "boolean"
    },
    onlyIfPlayerLeft = {
        fieldType = "boolean"
    },
    allowPlayerInteraction = {
        fieldType = "boolean"
    },
    tutorialTimeout = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 999.0
    },
    faceLeft = {
        fieldType = "boolean"
    }
}

enhancedBirdNPC.placements = {
    -- Tutorial Modes
    {
        name = "climbing_tutorial",
        description = "Teaches wall climbing mechanics",
        data = {
            mode = "ClimbingTutorial",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "dashing_tutorial",
        description = "Teaches dashing mechanics",
        data = {
            mode = "DashingTutorial",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "dream_jump_tutorial",
        description = "Teaches dream jump ability",
        data = {
            mode = "DreamJumpTutorial",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "super_wall_jump_tutorial",
        description = "Teaches super wall jump",
        data = {
            mode = "SuperWallJumpTutorial",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "hyper_jump_tutorial",
        description = "Teaches hyper jump",
        data = {
            mode = "HyperJumpTutorial",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "kirby_copy_ability_tutorial",
        description = "Kirby: Copy ability tutorial",
        data = {
            mode = "KirbyCopyAbilityTutorial",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "kirby_inhale_ability_tutorial",
        description = "Kirby: Inhale ability tutorial",
        data = {
            mode = "KirbyInhaleAbilityTutorial",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "kirby_float_ability_tutorial",
        description = "Kirby: Float ability tutorial",
        data = {
            mode = "KirbyFloatAbilityTutorial",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    -- Behavior Modes
    {
        name = "fly_away",
        description = "Bird waits then flies away",
        data = {
            mode = "FlyAway",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "idle",
        description = "Bird in idle state",
        data = {
            mode = "Idle",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "sleeping",
        description = "Bird is sleeping",
        data = {
            mode = "Sleeping",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "curious",
        description = "Bird pecking curiously",
        data = {
            mode = "Curious",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "distressed",
        description = "Bird in distressed state",
        data = {
            mode = "Distressed",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "none",
        description = "No specific behavior",
        data = {
            mode = "None",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    -- Advanced Modes
    {
        name = "move_to_nodes",
        description = "Bird flies through waypoint nodes",
        data = {
            mode = "MoveToNodes",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    },
    {
        name = "wait_for_lightning_off",
        description = "Bird waits for lightning to cease",
        data = {
            mode = "WaitForLightningOff",
            onlyOnce = false,
            onlyIfPlayerLeft = false,
            allowPlayerInteraction = true,
            tutorialTimeout = 30.0,
            faceLeft = true
        }
    }
}

function enhancedBirdNPC.scale(room, entity)
    return (entity.faceLeft and -1 or 1), 1
end

return enhancedBirdNPC
