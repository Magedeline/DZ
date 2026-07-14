local firewallSwitch = {}

firewallSwitch.name = "DZ/FirewallSwitch"
firewallSwitch.depth = 0
firewallSwitch.texture = "objects/DZ/firewall_switch"

firewallSwitch.placements = {
    name = "firewall_switch",
    data = {
        startActive = true,
        maxEnergy = 100,
        energyDrain = 5
    }
}

return firewallSwitch