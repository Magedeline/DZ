local fakeHeartGem = {}

fakeHeartGem.name = "DZ/FakeHeartGem"
fakeHeartGem.depth = 0
fakeHeartGem.texture = "objects/DZ/DZ/DZ/fake_heart_gem"

fakeHeartGem.placements = {
    name = "fake_heart_gem",
    data = {
        collectMessage = "It's fake!",
        persistent = false,
        respawnTime = 3.0
    }
}

return fakeHeartGem