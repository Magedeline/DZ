local bigBonfire = {}

bigBonfire.name = "DZ/BigBonfire"

-- Use the same campfire texture used by the vanilla Bonfire for preview
bigBonfire.texture = "objects/campfire/idle00"
bigBonfire.depth   = -5

bigBonfire.placements = {
    {
        name = "unlit",
        data = {
            mode        = "Unlit",
            scale       = 2.0,
            lightInner  = 64.0,
            lightOuter  = 128.0,
            bloomRadius = 64.0
        }
    },
    {
        name = "lit",
        data = {
            mode        = "Lit",
            scale       = 2.0,
            lightInner  = 64.0,
            lightOuter  = 128.0,
            bloomRadius = 64.0
        }
    },
    {
        name = "smoking",
        data = {
            mode        = "Smoking",
            scale       = 2.0,
            lightInner  = 64.0,
            lightOuter  = 128.0,
            bloomRadius = 64.0
        }
    }
}

bigBonfire.fieldInformation = {
    mode = {
        options    = { "Unlit", "Lit", "Smoking" },
        editable   = false
    },
    scale = {
        fieldType    = "number",
        minimumValue = 1.0,
        maximumValue = 5.0
    },
    lightInner = {
        fieldType    = "number",
        minimumValue = 16.0,
        maximumValue = 256.0
    },
    lightOuter = {
        fieldType    = "number",
        minimumValue = 32.0,
        maximumValue = 512.0
    },
    bloomRadius = {
        fieldType    = "number",
        minimumValue = 8.0,
        maximumValue = 256.0
    }
}

return bigBonfire
