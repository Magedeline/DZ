local vesselVoidBackdrop = {}

vesselVoidBackdrop.name = "DZ/VesselVoidBackdrop"
vesselVoidBackdrop.depth = 10000
vesselVoidBackdrop.texture = "objects/DZ/DZ/DZ/effects/vesselvoid"

vesselVoidBackdrop.fieldInformation = {
    innerColor = { fieldType = "color" },
    outerColor = { fieldType = "color" },
    seed = { fieldType = "integer" }
}

vesselVoidBackdrop.placements = {
    name = "vessel_void_backdrop",
    data = {
        alpha = 1.0,
        pulseSpeed = 1.4,
        innerColor = "FF6FCF",
        outerColor = "3AC8FF",
        seed = 42
    }
}

return vesselVoidBackdrop
