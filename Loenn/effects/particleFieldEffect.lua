local particleFieldEffect = {}

particleFieldEffect.name = "DZ/ParticleFieldEffect"
particleFieldEffect.depth = 10000
particleFieldEffect.texture = "objects/DZ/effects/particlefield"
particleFieldEffect.placements = {
    name = "particle_field_effect",
    data = {
        particleType = "sparkle",
        density = 1.0,
        active = true
    }
}

return particleFieldEffect