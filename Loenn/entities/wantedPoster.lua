local wantedPoster = {}

wantedPoster.name = "DZ/WantedPoster"
wantedPoster.depth = 0
wantedPoster.texture = "objects/DZ/wanted_poster"

wantedPoster.placements = {
    name = "wanted_poster",
    data = {
        bountyName = "OUTLAW",
        enemyType = "DZ/BanditoRoller",
        bountyReward = 100,
        enemyCount = 3
    }
}

return wantedPoster