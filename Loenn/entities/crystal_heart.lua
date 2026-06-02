local heart = {}

heart.name = "MaggyHelper/HeartGem"
heart.depth = -2000000
heart.texture = "collectables/heartGem/0/00"
heart.placements = {
    name = "real_crystal_heart",
    data = {
        fake = false,
        removeCameraTriggers = false,
        fakeHeartDialog = "CH19_WRONG_HEART",
        keepGoingDialog = "CH19_KEEP_GOING_KIRBY"
    }
}

return heart