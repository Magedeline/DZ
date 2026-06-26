local bountyBoard = {}

bountyBoard.name = "DZ/BountyBoard"
bountyBoard.depth = 0
bountyBoard.texture = "objects/DZ/bounty_board"

bountyBoard.placements = {
    name = "bounty_board",
    data = {
        bountyName = "OUTLAW",
        enemyType = "DZ/BanditoRoller",
        bountyReward = 100,
        enemyCount = 3
    }
}

return bountyBoard