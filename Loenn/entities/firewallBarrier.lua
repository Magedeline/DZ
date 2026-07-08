local firewallBarrier = {}

firewallBarrier.name = "DZ/FirewallBarrier"
firewallBarrier.depth = 0
firewallBarrier.texture = "objects/DZ/DZ/DZ/firewall_barrier"

firewallBarrier.placements = {
    name = "firewall_barrier",
    data = {
        startActive = true,
        maxEnergy = 100,
        energyDrain = 5
    }
}

return firewallBarrier