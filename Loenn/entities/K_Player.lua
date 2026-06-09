local K_Player = {}

K_Player.name = "MaggyHelper/K_Player"
K_Player.depth = -1
K_Player.texture = "characters/player/idle00"

K_Player.fieldInformation = {
    spriteMode = {
        fieldType = "string",
        options = {
            "Madeline",
            "MadelineNoBackpack",
            "Badeline",
            "MadelineAsBadeline",
            "BadelineAsMadeline"
        },
        editable = false
    },
    introType = {
        fieldType = "string",
        options = {
            "Transition",
            "Respawn",
            "WalkInRight",
            "WalkInLeft",
            "Jump",
            "WakeUp",
            "Fall",
            "TempleMirrorVoid",
            "None"
        },
        editable = false
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 20
    },
    kirbyMode = {
        fieldType = "boolean"
    },
    combatEnabled = {
        fieldType = "boolean"
    }
}

K_Player.fieldOrder = {
    "x", "y",
    "spriteMode",
    "introType",
    "maxHealth",
    "kirbyMode",
    "combatEnabled"
}

K_Player.placements = {
    {
        name = "K_Player (Madeline)",
        data = {
            spriteMode = "Madeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = false,
            combatEnabled = false
        }
    },
    {
        name = "K_Player (Madeline No Backpack)",
        data = {
            spriteMode = "MadelineNoBackpack",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = false,
            combatEnabled = false
        }
    },
    {
        name = "K_Player (Badeline)",
        data = {
            spriteMode = "Badeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = false,
            combatEnabled = false
        }
    },
    {
        name = "K_Player (Madeline as Badeline)",
        data = {
            spriteMode = "MadelineAsBadeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = false,
            combatEnabled = false
        }
    },
    {
        name = "K_Player (Badeline as Madeline)",
        data = {
            spriteMode = "BadelineAsMadeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = false,
            combatEnabled = false
        }
    },
    {
        name = "K_Player (Madeline - Kirby Mode)",
        data = {
            spriteMode = "Madeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = true,
            combatEnabled = false
        }
    },
    {
        name = "K_Player (Madeline - Combat Mode)",
        data = {
            spriteMode = "Madeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = false,
            combatEnabled = true
        }
    },
    {
        name = "K_Player (Madeline - Full Features)",
        data = {
            spriteMode = "Madeline",
            introType = "Transition",
            maxHealth = 10,
            kirbyMode = true,
            combatEnabled = true
        }
    }
}

return K_Player
