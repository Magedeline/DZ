-- Blackhole Flare Blocker Trigger - blocks BlackholeFlareSideway and BlackholeRiser entities
local blackholeFlareBlocker = {}

blackholeFlareBlocker.name = "DZ/BlackholeFlareBlocker"
blackholeFlareBlocker.placements = {
    {
        name = "stop",
        data = {
            width = 16,
            height = 64,
            behavior = "Stop",
            affectsSideway = true,
            affectsRiser = true
        }
    },
    {
        name = "reverse",
        data = {
            width = 16,
            height = 64,
            behavior = "Reverse",
            affectsSideway = true,
            affectsRiser = true
        }
    },
    {
        name = "destroy",
        data = {
            width = 16,
            height = 64,
            behavior = "Destroy",
            affectsSideway = true,
            affectsRiser = true
        }
    },
    {
        name = "sideway_only",
        data = {
            width = 16,
            height = 64,
            behavior = "Stop",
            affectsSideway = true,
            affectsRiser = false
        }
    },
    {
        name = "riser_only",
        data = {
            width = 64,
            height = 16,
            behavior = "Stop",
            affectsSideway = false,
            affectsRiser = true
        }
    }
}

blackholeFlareBlocker.fieldInformation = {
    behavior = {
        options = { "Stop", "Reverse", "Destroy" },
        editable = false
    },
    affectsSideway = {
        fieldType = "boolean"
    },
    affectsRiser = {
        fieldType = "boolean"
    }
}

return blackholeFlareBlocker
