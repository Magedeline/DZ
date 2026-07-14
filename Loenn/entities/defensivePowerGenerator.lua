local defensivePowerGenerator = {}

defensivePowerGenerator.name = "DZ/DefensivePowerGenerator"
defensivePowerGenerator.depth = 0
defensivePowerGenerator.texture = "objects/DZ/defensive_power_generator"

defensivePowerGenerator.placements = {
    name = "defensive_power_generator",
    data = {
        flipX = false,
        beamEnabled = true,
        laserInterval = 2.5,
        rotationSpeed = 1.5,
        beamRadius = 48.0
    }
}

defensivePowerGenerator.fieldOrder = {
    "x", "y", "flipX", "beamEnabled", "laserInterval", "rotationSpeed", "beamRadius"
}

return defensivePowerGenerator
