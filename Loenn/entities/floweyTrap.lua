local floweyTrap = {}

floweyTrap.name = "DZ/FloweyTrap"
floweyTrap.depth = 0
floweyTrap.texture = "objects/DZ/flowey_trap"

floweyTrap.placements = {
    name = "flowey_trap",
    data = {
        health = 2,
        pelletCount = 5,
        detectionRange = 120,
        retractRange = 180,
        pelletSpeed = 150,
        attackInterval = 1.5
    }
}

return floweyTrap