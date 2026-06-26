local bountyReward = {}

bountyReward.name = "DZ/BountyReward"
bountyReward.depth = 0
bountyReward.texture = "objects/DZ/bounty_reward"

bountyReward.placements = {
    name = "bounty_reward",
    data = {
        bountyName = "OUTLAW",
        enemyType = "DZ/BanditoRoller",
        bountyReward = 100,
        enemyCount = 3
    }
}

return bountyReward