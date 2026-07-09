local siamoZeroFinalBoss = {}

siamoZeroFinalBoss.name = "DZ/SiamoZeroFinalBoss"
siamoZeroFinalBoss.depth = 0
siamoZeroFinalBoss.nodeLineRenderType = "line"
siamoZeroFinalBoss.nodeLimits = {0, -1}
siamoZeroFinalBoss.texture = "characters/DZ/els_true_final_boss/boss00"

siamoZeroFinalBoss.placements = {
    name = "siamo_zero_final_boss",
    data = {
        patternIndex = 4,
        dialog = true,
        startHit = false,
        attackSequence = "CrescentBeamShot,EnergySwordCombo,TornadoSlash,RevolutionSword,RisingSpine,DownThrust,DrillStab,EnergyShower,VortexStrike,DoubleSideSlash,MorphoEmerge,TimeborderCollapse",
        siamoVariant = "zero",
        siamoTier = "soulBlack"
    }
}

siamoZeroFinalBoss.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
        minimumValue = 0
    },
    siamoVariant = {
        options = {"zero", "delta", "celestial"},
        editable = true
    },
    siamoTier = {
        options = {"soulBlack", "stellarruss"},
        editable = true
    }
}

-- ── SiamoZeroDelta ────────────────────────────────────────────────────────────

local siamoZeroDelta = {}

siamoZeroDelta.name = "DZ/SiamoZeroDelta"
siamoZeroDelta.depth = 0
siamoZeroDelta.nodeLineRenderType = "line"
siamoZeroDelta.nodeLimits = {0, -1}
siamoZeroDelta.texture = "characters/DZ/els_true_final_boss/boss00"

siamoZeroDelta.placements = {
    name = "siamo_zero_delta",
    data = {
        patternIndex = 0,
        dialog = false,
        startHit = true,
        attackSequence = "CrescentBeamShot,EnergySwordCombo,ConqueredPeakCascade,TornadoSlash,EnergyShower,VortexStrike,DoubleSideSlash,MorphoEmerge,TimeborderCollapse",
        siamoVariant = "delta",
        siamoTier = "soulBlack"
    }
}

siamoZeroDelta.fieldInformation = siamoZeroFinalBoss.fieldInformation

-- ── CelestialZero ─────────────────────────────────────────────────────────────

local celestialZero = {}

celestialZero.name = "DZ/CelestialZero"
celestialZero.depth = 0
celestialZero.nodeLineRenderType = "line"
celestialZero.nodeLimits = {0, -1}
celestialZero.texture = "characters/DZ/els_true_final_boss/boss00"

celestialZero.placements = {
    name = "celestial_zero",
    data = {
        patternIndex = 0,
        dialog = false,
        startHit = true,
        attackSequence = "CrescentBeamShot,RevolutionSword,ConqueredPeakCascade,EnergyShower,VortexStrike,DoubleSideSlash,TimeborderCollapse,MorphoEmerge,ConqueredPeakCascade",
        siamoVariant = "celestial",
        siamoTier = "stellarruss"
    }
}

celestialZero.fieldInformation = siamoZeroFinalBoss.fieldInformation

-- ── ElsTrueFinalBoss (legacy ID) ──────────────────────────────────────────────

local elsTrueFinalBoss = {}

elsTrueFinalBoss.name = "DZ/ElsTrueFinalBoss"
elsTrueFinalBoss.depth = 0
elsTrueFinalBoss.nodeLineRenderType = "line"
elsTrueFinalBoss.nodeLimits = {0, -1}
elsTrueFinalBoss.texture = "characters/DZ/els_true_final_boss/boss00"

elsTrueFinalBoss.placements = {
    name = "els_true_final_boss",
    data = {
        patternIndex = 0,
        dialog = false,
        startHit = true,
        attackSequence = "",
        siamoVariant = "",
        siamoTier = ""
    }
}

elsTrueFinalBoss.fieldInformation = siamoZeroFinalBoss.fieldInformation

return {
    siamoZeroFinalBoss,
    siamoZeroDelta,
    celestialZero,
    elsTrueFinalBoss
}
