local birdNpcMod = {}

birdNpcMod.name = "MaggyHelper/BirdNPC"
birdNpcMod.depth = -1000000
birdNpcMod.nodeLineRenderType = "line"
birdNpcMod.justification = {0.5, 1.0}
birdNpcMod.nodeLimits = {0, -1}

-- Bird type to sprite path mapping
local birdTypeTextures = {
    Default = "characters/bird/crow00",
    Clover = "characters/birdgoner/clover/crow00",
    Cody = "characters/birdgoner/cody/crow00",
    Emily = "characters/birdgoner/emily/crow00",
    Odin = "characters/birdgoner/odin/crow00",
    Robin = "characters/birdgoner/robin/crow00",
    Sabel = "characters/birdgoner/sabel/crow00"
}

function birdNpcMod.texture(room, entity)
    local birdType = entity.birdType or "Default"
    return birdTypeTextures[birdType] or birdTypeTextures.Default
end

birdNpcMod.fieldInformation = {
    mode = {
        fieldType = "string",
        options = {
            "ClimbingTutorial",
            "DashingTutorial", 
            "DreamJumpTutorial",
            "SuperWallJumpTutorial",
            "HyperJumpTutorial",
            "FlyAway",
            "None",
            "Sleeping",
            "MoveToNodes",
            "WaitForLightningOff"
        },
        editable = true
    },
    birdType = {
        fieldType = "string",
        options = {
            "Default",
            "Clover",
            "Cody",
            "Emily",
            "Odin",
            "Robin",
            "Sabel"
        },
        editable = false
    },
    autoFly = {
        fieldType = "boolean"
    },
    flyAwayUp = {
        fieldType = "boolean"
    },
    waitForLightningPostDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    disableFlapSfx = {
        fieldType = "boolean"
    },
    onlyOnce = {
        fieldType = "boolean"
    },
    onlyIfKglobal::PlayerLeft = {
        fieldType = "boolean"
    }
}

birdNpcMod.placements = {
    -- Default bird placements
    {
        name = "sleeping",
        data = {
            mode = "Sleeping",
            birdType = "Default",
            autoFly = false,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = false,
            onlyIfKglobal::PlayerLeft = false
        }
    },
    {
        name = "climbing_tutorial",
        data = {
            mode = "ClimbingTutorial",
            birdType = "Default",
            autoFly = false,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = true,
            onlyIfKglobal::PlayerLeft = false
        }
    },
    {
        name = "dashing_tutorial",
        data = {
            mode = "DashingTutorial",
            birdType = "Default",
            autoFly = false,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = true,
            onlyIfKglobal::PlayerLeft = false
        }
    },
    {
        name = "fly_away",
        data = {
            mode = "FlyAway",
            birdType = "Default",
            autoFly = true,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = false,
            onlyIfKglobal::PlayerLeft = false
        }
    },
    -- Bird Goner: Clover
    {
        name = "clover",
        data = {
            mode = "None",
            birdType = "Clover",
            autoFly = false,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = false,
            onlyIfKglobal::PlayerLeft = false
        }
    },
    -- Bird Goner: Cody
    {
        name = "cody",
        data = {
            mode = "None",
            birdType = "Cody",
            autoFly = false,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = false,
            onlyIfKglobal::PlayerLeft = false
        }
    },
    -- Bird Goner: Emily
    {
        name = "emily",
        data = {
            mode = "None",
            birdType = "Emily",
            autoFly = false,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = false,
            onlyIfKglobal::PlayerLeft = false
        }
    },
    -- Bird Goner: Odin
    {
        name = "odin",
        data = {
            mode = "None",
            birdType = "Odin",
            autoFly = false,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = false,
            onlyIfKglobal::PlayerLeft = false
        }
    },
    -- Bird Goner: Robin
    {
        name = "robin",
        data = {
            mode = "None",
            birdType = "Robin",
            autoFly = false,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = false,
            onlyIfKglobal::PlayerLeft = false
        }
    },
    -- Bird Goner: Sabel
    {
        name = "sabel",
        data = {
            mode = "None",
            birdType = "Sabel",
            autoFly = false,
            flyAwayUp = true,
            waitForLightningPostDelay = 0.0,
            disableFlapSfx = false,
            onlyOnce = false,
            onlyIfKglobal::PlayerLeft = false
        }
    }
}

local modeFacingScale = {
    climbingtutorial = -1,
    dashingtutorial = 1,
    dreamjumptutorial = 1,
    superwalljumptutorial = -1,
    hyperjumptutorial = -1,
    movetonodes = -1,
    waitforlightningoff = -1,
    flyaway = -1,
    sleeping = 1,
    none = -1
}

function birdNpcMod.scale(room, entity)
    local mode = string.lower(entity.mode or "sleeping")
    return modeFacingScale[mode] or -1, 1
end

return birdNpcMod
